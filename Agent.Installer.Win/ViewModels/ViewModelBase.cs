﻿using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace iControl.Agent.Installer.Win.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void FirePropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
