using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Media;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using src.ViewModels;
using static Converter;
using static BM;
using static KMP;
using static LevenshteinDistance;
using static Alay;
using System.Text;

namespace src.Views
{
    public partial class MainWindow : UserControl
    {
        public Bitmap uploadedImage;
        private readonly DatabaseHelper dbHelper = new();
        private readonly string baseDirectory = AppContext.BaseDirectory;
        private string fullImageAscii;  // To store the full ASCII representation of the uploaded image
        private readonly ConcurrentBag<(ImageRecord imageEntity, string dbImageAscii)> dbImagesWithAscii = new();  // To store ASCII representations of DB images

        public MainWindow()
        {
            InitializeComponent();
            string truncatedBaseDirectory = baseDirectory.Substring(0, baseDirectory.IndexOf("src", StringComparison.Ordinal));
            imageUploaded.Fill = new ImageBrush { Source = new Bitmap(Path.Combine(truncatedBaseDirectory, "src/Assets/reference.png")) };
            matchedImage.Fill = new ImageBrush { Source = new Bitmap(Path.Combine(truncatedBaseDirectory, "src/Assets/result.png")) };
            DataContext = new MainWindowViewModel();
            answersData.IsVisible = false;
            loading.IsVisible = false;
            notFound.IsVisible = false;
        }

        private async void OnImageUploadClicked(object sender, RoutedEventArgs e)
        {
            var customBMPFileType = new FilePickerFileType("Only BMP Images")
            {
                Patterns = new[] { "*.BMP" },
                AppleUniformTypeIdentifiers = new[] { "org.webmproject.BMP" },
                MimeTypes = new[] { "image/BMP" }
            };

            var topLevel = TopLevel.GetTopLevel(this);

            var selectedFiles = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                AllowMultiple = false,
                FileTypeFilter = new[] { customBMPFileType },
                Title = "Select an image file"
            });

            if (selectedFiles != null && selectedFiles.Count > 0)
            {
                using var stream = await selectedFiles[0].OpenReadAsync();
                uploadedImage = new Bitmap(stream);
                imageUploaded.Fill = new ImageBrush { Source = uploadedImage };
                string truncatedBaseDirectory = baseDirectory.Substring(0, baseDirectory.IndexOf("src", StringComparison.Ordinal));
                matchedImage.Fill = new ImageBrush { Source = new Bitmap(Path.Combine(truncatedBaseDirectory, "src/Assets/result.png")) };


                EnableSearchButtonIfReady();
            }
        }

        private void OnAlgorithmChecked(object sender, RoutedEventArgs e) => EnableSearchButtonIfReady();

        private void EnableSearchButtonIfReady()
        {
            SearchButton.IsEnabled = uploadedImage != null && (BMRadioButton.IsChecked == true || KMPRadioButton.IsChecked == true);
        }

        private async void OnSearchClicked(object sender, RoutedEventArgs e)
        {
            answersData.IsVisible = false;
            matchedImage.Fill = new SolidColorBrush(Color.Parse("#917859"));
            if (uploadedImage == null) return;
            SearchButton.IsEnabled = false;
            executionTimeTextBlock.Text = "Waktu Pencarian:";
            similarityTextBlock.Text = "Persentase Kecocokan:";
            loading.IsVisible = true;
            string algorithm = BMRadioButton.IsChecked == true ? "BM" : "KMP";
            int numblack;
            bool[] imageBinary;
            string imageAscii;
            // For second and third chance :)
            bool found = false;

            var stopwatch = Stopwatch.StartNew();
            (imageBinary, numblack) = GetSelectedBinary(uploadedImage, "BOTTOM");

            if (numblack > 3)
            {
                imageAscii = ConvertBinaryToAscii(imageBinary);

                var imagesToCompare = dbHelper.GetAllImages();

                found = await SearchForExactMatchAsync(imagesToCompare, imageAscii, algorithm);
            }


            // Search atas
            if (!found)
            {
                (imageBinary, numblack) = GetSelectedBinary(uploadedImage, "MOSTBOT");

                if (numblack > 3 && numblack != 103)
                {
                    imageAscii = ConvertBinaryToAscii(imageBinary);
                    found = SearchForExact2(dbImagesWithAscii, imageAscii, algorithm);
                }
            }

            // Search tengah
            if (!found)
            {
                (imageBinary, numblack) = GetSelectedBinary(uploadedImage, "TOP");

                if (numblack > 3 && numblack != 103)
                {
                    imageAscii = ConvertBinaryToAscii(imageBinary);
                    found = SearchForExact2(dbImagesWithAscii, imageAscii, algorithm);
                }
            }

            if (!found)
            {
                fullImageAscii = ConvertBinaryToAscii(ConvertFullImageToBinary(uploadedImage));
                SearchForApproximateMatch(dbImagesWithAscii, fullImageAscii);
            }

            stopwatch.Stop();
            SearchButton.IsEnabled = true;
            loading.IsVisible = false;
            executionTimeTextBlock.Text = $"Waktu Pencarian: {stopwatch.ElapsedMilliseconds} ms";
        }

        private async Task<bool> SearchForExactMatchAsync(IEnumerable<ImageRecord> imagesToCompare, string imageAscii, string algorithm)
        {
            var tasks = new List<Task<(ImageRecord imageEntity, Bitmap dbImage, bool isMatch)>>();

            await Parallel.ForEachAsync(imagesToCompare, async (imageEntity, cancellationToken) =>
            {
                string imagePath = GetImagePath(imageEntity.BerkasCitra);
                if (!File.Exists(imagePath))
                {
                    Console.WriteLine($"File {imagePath} not found.");
                    tasks.Add(Task.FromResult((imageEntity, (Bitmap)null, false)));
                    return;
                }

                using var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
                var dbImage = new Bitmap(stream);

                var dbImageAscii = ConvertImageToAsciiFull(dbImage);

                dbImagesWithAscii.Add((imageEntity, dbImageAscii));

                int result = algorithm == "BM" ? BmMatch(dbImageAscii, imageAscii) : KmpMatch(dbImageAscii, imageAscii);

                bool isMatch = result != -1;
                tasks.Add(Task.FromResult((imageEntity, dbImage, isMatch)));
            });

            var results = await Task.WhenAll(tasks);

            foreach (var (imageEntity, dbImage, isMatch) in results)
            {
                if (isMatch)
                {
                    matchedImage.Fill = new ImageBrush { Source = dbImage };
                    DisplayPersonDetails(imageEntity.Name);
                    similarityTextBlock.Text = "Persentase Kecocokan: 100%";
                    return true;
                }
            }

            return false;
        }

        private bool SearchForExact2(IEnumerable<(ImageRecord imageEntity, string dbImageAscii)> imagesToCompare, string imageAscii, string algorithm)
        {
            foreach (var (imageEntity, dbImageAscii) in imagesToCompare)
            {
                int result = algorithm == "BM" ? BmMatch(dbImageAscii, imageAscii) : KmpMatch(dbImageAscii, imageAscii);
                if (result != -1)
                {
                    matchedImage.Fill = new ImageBrush { Source = new Bitmap(GetImagePath(imageEntity.BerkasCitra)) };
                    DisplayPersonDetails(imageEntity.Name);
                    similarityTextBlock.Text = "Persentase Kecocokan: 100%";
                    return true;
                }
            }

            return false;
        }

        private void SearchForApproximateMatch(IEnumerable<(ImageRecord imageEntity, string dbImageAscii)> imagesToCompare, string imageAscii)
        {
            double maxSimilarity = 0.0;
            ImageRecord bestMatch = null;
            object lockObject = new object(); // Used for thread-safe updates

            Parallel.ForEach(imagesToCompare, (item) =>
            {
                var (imageEntity, dbImageAscii) = item;
                double similarity = ComputeSimilarity(dbImageAscii, imageAscii);

                lock (lockObject)
                {
                    if (similarity > maxSimilarity)
                    {
                        maxSimilarity = similarity;
                        bestMatch = imageEntity;
                    }
                }
            });

            if (maxSimilarity > 60)
            {
                matchedImage.Fill = new ImageBrush { Source = new Bitmap(GetImagePath(bestMatch.BerkasCitra)) };
                DisplayPersonDetails(bestMatch.Name);
                similarityTextBlock.Text = $"Persentase Kecocokan: {maxSimilarity:F2}%";
            }
            else
            {
                notFound.IsVisible = true;
                similarityTextBlock.Text = "Persentase Kecocokan: 0%";
            }
        }

        private void DisplayPersonDetails(string name)
        {
            answersData.IsVisible = true;
            byte[] key = Encoding.UTF8.GetBytes("tubesstimaterakhirohyeah12345678");
            AES uhm = new AES(key);
            var person = dbHelper.GetAllPeople().FirstOrDefault(p => AlayMatcher.IsAlayVersion(name, uhm.Decrypt(p.Nama)));
            if (person != null)
            {
                namaTitle.Text = "NAMA";
                namaData.Text = $"{name}";

                nikTitle.Text = "NIK";
                nikData.Text = $"{uhm.Decrypt(person.NIK)}";

                tempatLahirTitle.Text = "TEMPAT LAHIR";
                tempatLahirData.Text = $"{uhm.Decrypt(person.Tempat_lahir)}";

                tanggalLahirTitle.Text = "TANGGAL LAHIR";
                tanggalLahirData.Text = $"{uhm.Decrypt(person.Tanggal_lahir)}";

                genderTitle.Text = "JENIS KELAMIN";
                genderData.Text = $"{person.Jenis_kelamin}";

                goldarTitle.Text = "GOLONGAN DARAH";
                goldarData.Text = $"{uhm.Decrypt(person.Golongan_darah)}";

                alamatTitle.Text = "ALAMAT";
                alamatData.Text = $"{uhm.Decrypt(person.Alamat)}";

                agamaTitle.Text = "AGAMA";
                agamaData.Text = $"{uhm.Decrypt(person.Agama)}";

                kawinTitle.Text = "STATUS PERKAWINAN";
                kawinData.Text = $"{uhm.Decrypt(person.Status_perkawinan)}";

                pekerjaanTitle.Text = "PEKERJAAN";
                pekerjaanData.Text = $"{uhm.Decrypt(person.Pekerjaan)}";

                wargaTitle.Text = "KEWARGANEGARAAN";
                wargaData.Text = $"{uhm.Decrypt(person.Kewarganegaraan)}";
            }
        }

        private string GetImagePath(string imageName)
        {
            string truncatedBaseDirectory = baseDirectory.Substring(0, baseDirectory.IndexOf("src", StringComparison.Ordinal));
            return Path.Combine(truncatedBaseDirectory, "test", "Real", imageName);
        }

        private void closeNotFoundPanel(object sender, RoutedEventArgs e)
        {
            notFound.IsVisible = false;
        }
    }
}
