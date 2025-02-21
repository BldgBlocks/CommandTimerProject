using Avalonia.Media;
using CommandTimer.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CommandTimer.Desktop.Utilities;

public class DefaultTimers_Simple(OSPlatform platform) : IDefaultTimerCollection {

    public IEnumerable<CommandTimerViewModel> Timers {
        get {
            return TimersByPlatform.TryGetValue(platform, out var timerFunc)
            ? timerFunc()
            : TimersByPlatform[OSPlatform.Linux]();
        }
    }

    private static Dictionary<OSPlatform, Func<IEnumerable<CommandTimerViewModel>>> TimersByPlatform => new() {
        { OSPlatform.Windows, () => new Windows_Simple().Timers },
        { OSPlatform.Linux,   () => new Linux_Simple().Timers },
    };

    //...

    private record Windows_Simple {
        public IEnumerable<CommandTimerViewModel> Timers => [
            new() {
                Name = "Clean Shutdown",
                Description = "Allows NTFS drives to be mounted by Linux after shutdown. '/t 3' delay, '/s' for clean shutdown, which involves: Writing all remaining data to disk. Closing open files and programs gracefully. Cleaning up temporary files.",
                ColorBarColor = new SolidColorBrush(Core.Colors.ParseHexToColor("#cc79a7")),
                Command = "shutdown /s /t 3",
            },
            new() {
                Name = "Show all open ports",
                Description = "Show detailed network information",
                ColorBarColor = new SolidColorBrush(Core.Colors.ParseHexToColor("#009e73")),
                Command = "netstat -an",
                IsShowTerminal = true,
            },
            new() {
                Name = "Show all WiFi passwords",
                Description = "Show detailed wifi information.",
                ColorBarColor = new SolidColorBrush(Core.Colors.ParseHexToColor("#009e73")),
                Command = "for /f \"skip=9 tokens=1,2 delims=:\" %i in ('netsh wlan show profiles') do @echo %j | findstr -i -v echo | netsh wlan show profiles %j key=clear",
                IsShowTerminal = true,
            },
            new() {
                Name = "Show connection information",
                Description = "Show detailed connection information.",
                ColorBarColor = new SolidColorBrush(Core.Colors.ParseHexToColor("#009e73")),
                Command = "ipconfig",
                IsShowTerminal = true,
            },
            new() {
                Name = "Scan for corrupted files",
                Description = "Scan and repair corrupted files.",
                ColorBarColor = new SolidColorBrush(Core.Colors.ParseHexToColor("#0072b2")),
                Command = "runas /user:<yourUser> 'sfc'",
                IsShowTerminal = true
            },
            new() {
                Name = "Defrag C drive",
                Description = "defragment the C: drive, perform free space consolidation, and provide verbose output.",
                ColorBarColor = new SolidColorBrush(Core.Colors.ParseHexToColor("#f0e442")),
                Command = "runas /user:<yourUser> 'defrag C: /X /V'",
                TimeMode = CommandTimerViewModel.TimeModeChoice.Time,
                TargetTimeSpanTillExecution = new TimeSpan(14,0,0),
                IsShowTerminal = true,
                IsLoop = true,
            },
        ];
    }

    private record Linux_Simple {
        public IEnumerable<CommandTimerViewModel> Timers => [
            new() {
                Name = "Vacuum Journal Entries - 3 Days",
                Description = "Clean journal entries and leave 3 days remaining.",
                ColorBarColor = new SolidColorBrush(Core.Colors.ParseHexToColor("#cc79a7")),
                Command = "sudo journalctl --vacuum-time=3d",
                IsFavorite = true,
                IsShowTerminal = true
            },
            new() {
                Name = "Record Start on DP-2",
                Description = "Record my screen with high quality.",
                Command = "gpu-screen-recorder -w DP-2 -f 60 -a default_output -c mkv -cr full -q ultra -k hevc_hdr -keyint 1.0 -fm cfr -ac opus -o /home/<user>/Downloads/Recordings/<RecordingName>.mkv",
                ColorBarColor = new SolidColorBrush(Core.Colors.ParseHexToColor("#009e73")),
                IsShowTerminal = true,
                IsFavorite = true,
                },
            new() {
                Name = "Record Stop on DP-2",
                Description = "Stop recording",
                Command = "killall -SIGINT gpu-screen-recorder &&",
                TargetTimeSpanTillExecution = new TimeSpan(1,45,0),
                ColorBarColor = new SolidColorBrush(Core.Colors.ParseHexToColor("#009e73")),
                IsFavorite = true,
                },
            new() {
                Name = "TimeShift Backup",
                Description = "Create a system backup using TimeShift. You will need to manage privileges for commands that require sudo, or type it in. You could create different backups to different locations at a specified time.",
                Command = "sudo timeshift --create --snapshot-device /mnt/backup --label 'Your note for a backup to a specific device.'",
                TimeMode = CommandTimerViewModel.TimeModeChoice.Time,
                TargetTimeSpanTillExecution = new TimeSpan(14,0,0),
                ColorBarColor = new SolidColorBrush(Core.Colors.ParseHexToColor("#0072b2")),
                IsFavorite = true,
                IsShowTerminal = true,
                IsLoop = true,
                IsAutoStart = true,
                },
            new() {
                Name = "Alphebetize Files",
                Description = "Run script to alphebetize files into folders.",
                ColorBarColor = new SolidColorBrush(Core.Colors.ParseHexToColor("#f0e442")),
                Command = "/home/<user>/Scripts/AlphebetizeFolders.sh --mock /directory/to/organize",
                IsShowTerminal = true
                },
            new() {
                Name = "Clean Pacman",
                Description = "Clean the cache",
                ColorBarColor = new SolidColorBrush(Core.Colors.ParseHexToColor("#d55e00")),
                Command = "sudo pacman -Scc",
                IsShowTerminal = true,
                IsPromptForExecute = true,
                TimeMode = CommandTimerViewModel.TimeModeChoice.Time,
                TargetTimeSpanTillExecution = TimeSpan.FromHours(2),
                },
            new() {
                Name = "Clean Paru",
                Description = "Clean the cache",
                ColorBarColor = new SolidColorBrush(Core.Colors.ParseHexToColor("#d55e00")),
                Command = "paru -Scc",
                IsShowTerminal = true,
                IsPromptForExecute = true,
                TimeMode = CommandTimerViewModel.TimeModeChoice.Time,
                TargetTimeSpanTillExecution = TimeSpan.FromHours(2),
                },
            new() {
                Name = "Delete Cache",
                Description = "Clean the cache",
                ColorBarColor = new SolidColorBrush(Core.Colors.ParseHexToColor("#d55e00")),
                Command = "sudo rm -rf /.cache/*",
                IsPromptForExecute = true,
                TimeMode = CommandTimerViewModel.TimeModeChoice.Time,
                TargetTimeSpanTillExecution = TimeSpan.FromHours(2),
                },
            new() {
                Name = "Delete temp files",
                Description = "Clean the cache",
                ColorBarColor = new SolidColorBrush(Core.Colors.ParseHexToColor("#d55e00")),
                Command = "sudo rm -rf /tmp/*",
                IsPromptForExecute = true,
                TimeMode = CommandTimerViewModel.TimeModeChoice.Time,
                TargetTimeSpanTillExecution = TimeSpan.FromHours(2),
            },
        ];
    }
}
