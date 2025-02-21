using System;
using System.Runtime.InteropServices;

namespace CommandTimer.Core.Utilities;

public static partial class SystemInteraction {
    public static class Platform {
        public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        public static bool IsMacOS => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        public static OSPlatform Get() {
            if (IsWindows) return OSPlatform.Windows;
            else if (IsLinux) return OSPlatform.Linux;
            else if (IsMacOS) return OSPlatform.OSX;
            else return OSPlatform.Linux;
        }
    }
}
