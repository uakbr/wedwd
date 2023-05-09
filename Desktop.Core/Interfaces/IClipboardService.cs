﻿using System;
using System.Threading.Tasks;

namespace iControl.Desktop.Core.Interfaces
{
    public interface IClipboardService
    {
        event EventHandler<string> ClipboardTextChanged;

        void BeginWatching();

        Task SetText(string clipboardText);
    }
}
