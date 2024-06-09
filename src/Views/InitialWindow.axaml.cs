using System;
using System.IO;
using System.Media;
using Avalonia.Controls;
using Avalonia.Interactivity;
using src.Views;

namespace MyAvaloniaApp
{
    public partial class InitialWindow : Window
    {
        public InitialWindow()
        {
            InitializeComponent();
            MainContent.Content = new InitialView();
            SoundPlayer musicPlayer = new SoundPlayer();
            string truncatedBaseDirectory = AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("src", StringComparison.Ordinal));
            musicPlayer.SoundLocation = Path.Combine(truncatedBaseDirectory, "src", "Assets", "song.wav");
            musicPlayer.PlayLooping();
        }


        private void OnOpenDetailedViewClicked(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new MainWindow();
        }
    }
}
