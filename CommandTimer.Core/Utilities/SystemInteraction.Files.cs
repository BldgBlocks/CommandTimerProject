using System;
using System.IO;

namespace CommandTimer.Core.Utilities;

public static partial class SystemInteraction {
    public static class Files {
        private static string GetLinuxConfigPath() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "CommandTimer");
        private static string GetWindowsConfigPath() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CommandTimer");
        private static string GetLinuxLogPath() => Path.Combine(GetLinuxConfigPath(), "Logs");
        private static string GetWindowsLogPath() => Path.Combine(GetWindowsConfigPath(), "Logs");

        public static string GetUserConfigPath() {
            string folderPath = string.Empty;

            if (Platform.IsLinux) {
                folderPath = GetLinuxConfigPath();
            }
            else if (Platform.IsWindows) {
                folderPath = GetWindowsConfigPath();
            }

            if (Directory.Exists(folderPath) is false) {
                Directory.CreateDirectory(folderPath);
            }

            return folderPath;
        }

        public static string GetPlatformLogFile() {
            string logDir = GetLogDirectory();
            if (!Directory.Exists(logDir)) {
                Directory.CreateDirectory(logDir);
            }

            //Debug.WriteLine($"Log Directory: {logDir}");

            return Path.Combine(logDir, GenerateLogFileName());
        }

        private static string GetLogDirectory() {
            if (Platform.IsWindows) {
                return GetWindowsLogPath();
            }
            else if (Platform.IsLinux) {
                return GetLinuxLogPath();
            }
            return string.Empty;
        }

        private static string GenerateLogFileName() {
            return $"LogOutput.log";
        }
    }
}
