using Avalonia.Controls;
using Avalonia.Threading;
using iControl.Desktop.Core.Interfaces;
using iControl.Desktop.XPlat.Views;

namespace iControl.Desktop.XPlat.Services
{
    public class SessionIndicatorLinux : ISessionIndicator
    {
        public void Show()
        {
            Dispatcher.UIThread.Post(() =>
            {
                var indicatorWindow = new SessionIndicatorWindow();
                indicatorWindow.Show();
            });
        }
    }
}
