using iControl.Desktop.Core.ViewModels;

namespace iControl.Desktop.Win.ViewModels
{
    public class HostNamePromptViewModel : BrandedViewModelBase
    {
        private string _host = "https://";

        public string Host
        {
            get => _host;
            set
            {
                _host = value;
                FirePropertyChanged();
            }
        }
    }
}
