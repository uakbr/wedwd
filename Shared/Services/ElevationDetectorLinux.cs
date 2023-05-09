﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace iControl.Shared.Services
{
    public class ElevationDetectorLinux : IElevationDetector
    {
        [DllImport("libc", SetLastError = true)]
        private static extern uint geteuid();

        public bool IsElevated()
        {
            return geteuid() == 0;
        }
    }
}
