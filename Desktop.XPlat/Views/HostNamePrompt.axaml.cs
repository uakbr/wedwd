using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using iControl.Desktop.XPlat.ViewModels;
using iControl.Desktop.XPlat.Views;

namespace iControl.Desktop.XPlat.Views
{
    public class HostNamePrompt : Window
    {
        public HostNamePrompt()
        {
            Owner = MainWindow.Current;
            InitializeComponent();
        }

        public HostNamePromptViewModel ViewModel => DataContext as HostNamePromptViewModel;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
