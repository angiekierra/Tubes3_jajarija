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
    public partial class MainWindow : Window
    {
        public Bitmap uploadedImage;
        private readonly DatabaseHelper dbHelper = new();
        private readonly string baseDirectory = AppContext.BaseDirectory;
        private string fullImageAscii;  // To store the full ASCII representation of the uploaded image
        private readonly ConcurrentBag<(ImageRecord imageEntity, string dbImageAscii)> dbImagesWithAscii = new();  // To store ASCII representations of DB images

        public MainWindow()
        {

            InitializeComponent();
            SoundPlayer musicPlayer = new SoundPlayer();
            string truncatedBaseDirectory = baseDirectory.Substring(0, baseDirectory.IndexOf("src", StringComparison.Ordinal));
            musicPlayer.SoundLocation = Path.Combine(truncatedBaseDirectory, "src", "Assets","song.wav");
            musicPlayer.PlayLooping();
            DataContext = new MainWindowViewModel();
        }

        private async void OnImageUploadClicked(object sender, RoutedEventArgs e)
        {
            var customBMPFileType = new FilePickerFileType("Only BMP Images")
            {
                Patterns = new[] { "*.BMP" },
                AppleUniformTypeIdentifiers = new[] { "org.webmproject.BMP" },
                MimeTypes = new[] { "image/BMP" }
            };

            var selectedFiles = await this.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
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
            if (uploadedImage == null) return;

            var stopwatch = Stopwatch.StartNew();
            string algorithm = BMRadioButton.IsChecked == true ? "BM" : "KMP";
            int numblack;
            bool[] imageBinary;
            string imageAscii;
            // For second and third chance :)
            bool found = false;
            (imageBinary, numblack) = GetSelectedBinary(uploadedImage, "BOTTOM");

            if (numblack > 8)
            {
                imageAscii = ConvertBinaryToAscii(imageBinary);


                var imagesToCompare = dbHelper.GetAllImages();

                found = await SearchForExactMatchAsync(imagesToCompare, imageAscii, algorithm);
            }

            // Search atas
            if (!found)
            {
                (imageBinary, numblack) = GetSelectedBinary(uploadedImage, "MOSTBOT");

                if (numblack > 8 && numblack != 103)
                {
                    imageAscii = ConvertBinaryToAscii(imageBinary);
                    found = SearchForExact2(dbImagesWithAscii, imageAscii, algorithm);
                }
            }

            // Search tengah
            if (!found)
            {
                (imageBinary, numblack) = GetSelectedBinary(uploadedImage, "TOP");

                if (numblack > 8 && numblack != 103)
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
            executionTimeTextBlock.Text = $"Waktu Pencarian: {stopwatch.ElapsedMilliseconds} ms";
        }

        private async Task<bool> SearchForExactMatchAsync(IEnumerable<ImageRecord> imagesToCompare, string imageAscii, string algorithm)
        {
            var tasks = imagesToCompare.Select(async imageEntity =>
            {
                string imagePath = GetImagePath(imageEntity.BerkasCitra);
                if (!File.Exists(imagePath))
                {
                    Console.WriteLine($"File {imagePath} not found.");
                    return (imageEntity, (Bitmap)null, false);
                }

                using var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
                var dbImage = new Bitmap(stream);
                var dbImageBinary = ConvertFullImageToBinary(dbImage);
                var dbImageAscii = ConvertBinaryToAscii(dbImageBinary);

                dbImagesWithAscii.Add((imageEntity, dbImageAscii));

                int result = algorithm == "BM" ? BmMatch(dbImageAscii, imageAscii) : KmpMatch(dbImageAscii, imageAscii);

                return result != -1 ? (imageEntity, dbImage, true) : (imageEntity, dbImage, false);
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
                similarityTextBlock.Text = "Persentase Kecocokan: 0%";
            }
        }

        private void DisplayPersonDetails(string name)
        {
            byte[] key = Encoding.UTF8.GetBytes("tubesstimaterakhirohyeah12345678");
            AES uhm = new AES(key);
            var person = dbHelper.GetAllPeople().FirstOrDefault(p => AlayMatcher.IsAlayVersion(name, uhm.Decrypt(p.Nama)));
            if (person != null)
            {
                var details = new TextBlock
                {
                    Text = $"Name: {name}\n" +
                           $"NIK: {uhm.Decrypt(person.NIK)}\n" +
                           $"Tempat Lahir: {uhm.Decrypt(person.Tempat_lahir)}\n" +
                           $"Tanggal Lahir: {uhm.Decrypt(person.Tanggal_lahir)}\n" +
                           $"Jenis Kelamin: {person.Jenis_kelamin}\n" +
                           $"Golongan Darah: {uhm.Decrypt(person.Golongan_darah)}\n" +
                           $"Alamat: {uhm.Decrypt(person.Alamat)}\n" +
                           $"Agama: {uhm.Decrypt(person.Agama)}\n" +
                           $"Status Perkawinan: {uhm.Decrypt(person.Status_perkawinan)}\n" +
                           $"Pekerjaan: {uhm.Decrypt(person.Pekerjaan)}\n" +
                           $"Kewarganegaraan: {uhm.Decrypt(person.Kewarganegaraan)}",
                    Foreground = Brushes.Black
                };
                personDetails.Text = details.Text;
            }
        }

        private string GetImagePath(string imageName)
        {
            string truncatedBaseDirectory = baseDirectory.Substring(0, baseDirectory.IndexOf("src", StringComparison.Ordinal));
            return Path.Combine(truncatedBaseDirectory, "test", "Real", imageName);
        }
    }
}
