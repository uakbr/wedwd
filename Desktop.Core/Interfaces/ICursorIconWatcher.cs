using iControl.Shared.Models;
using System;

namespace iControl.Desktop.Core.Interfaces
{
    public interface ICursorIconWatcher
    {
        event EventHandler<CursorInfo> OnChange;

        CursorInfo GetCurrentCursor();
    }

}
