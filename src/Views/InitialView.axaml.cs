using Avalonia.Controls;
using Avalonia.Interactivity;

namespace src.Views
{
    public partial class InitialView : UserControl
    {
        public InitialView()
        {
            InitializeComponent();
        }

        private void OnOpenDetailedViewClicked(object sender, RoutedEventArgs e)
        {
            ((Window)this.VisualRoot).FindControl<ContentControl>("MainContent").Content = new MainWindow();
        }
    }
}
