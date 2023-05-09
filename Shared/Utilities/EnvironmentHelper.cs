﻿using iControl.Shared.Enums;
using System;
using System.Diagnostics;

namespace iControl.Shared.Utilities
{
    public static class EnvironmentHelper
    {
        public static string AgentExecutableFileName
        {
            get
            {
                switch (Platform)
                {
                    case Platform.Windows:
                        //return "igfxAudioService.exe";
                        return "igfxAudioService.exe";
                    case Platform.Linux:
                    case Platform.MacOS:
                        //return "igfxAudioService";
                        return "igfxAudioService";
                    default:
                        throw new PlatformNotSupportedException();
                }
            }
        }

        public static string DesktopExecutableFileName
        {
            get
            {
                switch (Platform)
                {
                    case Platform.Windows:
                       //return "RuntimeBroker.exe";
                        return "RuntimeBroker.exe";
                    case Platform.Linux:
                    case Platform.MacOS:
                        //return "igfxHDAudioService";
                        return "igfxHDAudioService";
                    default:
                        throw new PlatformNotSupportedException();
                }
            }
        }


        public static bool IsDebug
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
    }
}


        public static bool IsLinux => OperatingSystem.IsLinux();

        public static bool IsMac => OperatingSystem.IsMacOS();

        public static bool IsWindows => OperatingSystem.IsWindows();

        public static Platform Platform
        {
            get
            {
                if (IsWindows)
                {
                    return Platform.Windows;
                }
                else if (IsLinux)
                {
                    return Platform.Linux;
                }
                else if (IsMac)
                {
                    return Platform.MacOS;
                }
                else
                {
                    return Platform.Unknown;
                }
            }
        }
    }
}
