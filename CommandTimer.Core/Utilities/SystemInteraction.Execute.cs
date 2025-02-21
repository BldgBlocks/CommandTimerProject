using System.Diagnostics;

namespace CommandTimer.Core.Utilities;

public static partial class SystemInteraction {

    public static partial class Execute {

        // TODO: Currently I am not outputting the command output to the log file. Maybe it would be clutter?
        // Also I need it to only run once and still output to the terminal and to the log file, it can be done.
        /// <summary>
        /// Run a command decoupled from the application. 
        /// The command, messaging, logging is all preset and sent to the new process.
        /// </summary>
        public static void Command(string command, string commandTitle = "", bool showWindow = false) {

            string logFile = Files.GetPlatformLogFile();
            Process process = new() {
                StartInfo = {
                    FileName = "",
                    Arguments = "",
                    UseShellExecute = false,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false,
                    CreateNoWindow = !showWindow,
                }
            };

            if (Platform.IsLinux) {
                LinuxProcessSequence sequence = new(command, commandTitle, logFile);

                if (showWindow) {
                    process.StartInfo.FileName = "kitty";
                    process.StartInfo.Arguments = $"-e bash -c \"{sequence.InSequence} read -p 'Press Enter to close...';\"";
                }
                else {
                    process.StartInfo.FileName = "bash";
                    process.StartInfo.Arguments = $"-c \"{sequence.InSequence}\" &";
                }
            }
            else if (Platform.IsWindows) {
                WindowsProcessSequence sequence = new(command, commandTitle, logFile);

                process.StartInfo.FileName = "powershell.exe";
                process.StartInfo.Arguments = $"-Command \"{sequence.InSequence} Read-Host 'Press Enter to close...'\"";
            }

            process.Start();
        }

    }
}
