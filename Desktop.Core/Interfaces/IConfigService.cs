﻿using iControl.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iControl.Desktop.Core.Interfaces
{
    public interface IConfigService
    {
        DesktopAppConfig GetConfig();
        void Save(DesktopAppConfig config);
    }
}
