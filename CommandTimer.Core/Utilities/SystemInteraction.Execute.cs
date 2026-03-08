using System;
using System.Diagnostics;

namespace CommandTimer.Core.Utilities;

public static partial class SystemInteraction {

    public static partial class Execute {

        private static string? _cachedTerminal;

        /// <summary>
        /// Resolves the user's preferred terminal emulator on Linux.
        /// Checks: $TERMINAL env var ? x-terminal-emulator ? xdg-terminal-exec ? common fallbacks.
        /// Result is cached after first successful lookup.
        /// </summary>
        private static string? DetectLinuxTerminal() {
            if (_cachedTerminal is not null) return _cachedTerminal;

            // 1. User's explicit preference via environment variable
            var envTerminal = Environment.GetEnvironmentVariable("TERMINAL");
            if (!string.IsNullOrWhiteSpace(envTerminal) && IsExecutableOnPath(envTerminal)) {
                _cachedTerminal = envTerminal;
                return _cachedTerminal;
            }

            // 2. Debian/Ubuntu alternatives system
            if (IsExecutableOnPath("x-terminal-emulator")) {
                _cachedTerminal = "x-terminal-emulator";
                return _cachedTerminal;
            }

            // 3. Freedesktop.org standard (newer distros)
            if (IsExecutableOnPath("xdg-terminal-exec")) {
                _cachedTerminal = "xdg-terminal-exec";
                return _cachedTerminal;
            }

            // 4. Minimal fallback for distros that set neither
            string[] fallbacks = ["gnome-terminal", "konsole", "xfce4-terminal", "xterm"];
            foreach (var terminal in fallbacks) {
                if (IsExecutableOnPath(terminal)) {
                    _cachedTerminal = terminal;
                    return _cachedTerminal;
                }
            }

            return null;
        }

        /// <summary>
        /// Checks if an executable exists on the system PATH using 'which'.
        /// </summary>
        private static bool IsExecutableOnPath(string executable) {
            try {
                using var process = Process.Start(new ProcessStartInfo {
                    FileName = "which",
                    Arguments = executable,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                process?.WaitForExit();
                return process?.ExitCode == 0;
            }
            catch {
                return false;
            }
        }

        /// <summary>
        /// Builds the arguments string for launching a command in a given terminal emulator.
        /// Most terminals accept '-e' to execute a command, with notable exceptions.
        /// </summary>
        private static (string FileName, string Arguments) BuildTerminalArgs(string terminal, string commandSequence) {
            string bashCommand = $"bash -c \"{commandSequence} read -p 'Press Enter to close...';\"";

            // gnome-terminal uses '--' instead of '-e'
            if (terminal.Contains("gnome-terminal")) {
                return (terminal, $"-- {bashCommand}");
            }

            // xdg-terminal-exec takes the command directly
            if (terminal.Contains("xdg-terminal-exec")) {
                return (terminal, $"bash -c \"{commandSequence} read -p 'Press Enter to close...';\"");
            }

            // Default: most terminals use '-e'
            return (terminal, $"-e {bashCommand}");
        }

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
                    var terminal = DetectLinuxTerminal();
                    if (terminal is null) {
                        MessageRelay.OnMessagePosted(nameof(Execute),
                            "No supported terminal emulator found. Set the $TERMINAL environment variable. Running command without a visible terminal.",
                            MessageRelay.MessageCategory.User);
                        process.StartInfo.FileName = "bash";
                        process.StartInfo.Arguments = $"-c \"{sequence.InSequence}\" &";
                    }
                    else {
                        var args = BuildTerminalArgs(terminal, sequence.InSequence);
                        process.StartInfo.FileName = args.FileName;
                        process.StartInfo.Arguments = args.Arguments;
                    }
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

            try {
                process.Start();
            }
            catch (Exception ex) {
                MessageRelay.OnMessagePosted(nameof(Execute),
                    $"Failed to start process: {ex.Message}",
                    MessageRelay.MessageCategory.Exception);
            }
        }

    }
}

