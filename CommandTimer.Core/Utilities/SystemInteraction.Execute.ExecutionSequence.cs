using System;
using System.Collections.Generic;
using System.IO;

namespace CommandTimer.Core.Utilities;

public static partial class SystemInteraction {

    private static string PowerShellEscapeSingleQuotes(this string value) => value.Replace("'", "''");
    private static string GetMessageBottomSeparator() => "--------------------";
    private static string FormatMessageToEchoCommand(this string value) => $"echo '{value}'";
    private static string SurroundWithSingleQuotes(this string value) => $"'{value}'";
    private static string PowerShellAppendToLogFile(this string value, string logFile) => $"{value} | Out-File -FilePath {logFile} -Append -Encoding utf8";
    private static string LinuxAppendToLogFile(this string value, string logFile) => $"{value} | tee -a {logFile}";
    private static string LinuxAppendToLogFileNoShow(this string value, string logFile) => $"{value} > /dev/null | tee -a {logFile}";
    private static string GetLogFileSeparator() => $"{Environment.NewLine}*** Finished: {DateTime.Now} ***{Environment.NewLine}";
    public static void AppendSeparatorToLogFile(string logFile) => File.AppendAllText(logFile, GetLogFileSeparator());

    public static partial class Execute {

        public abstract record ExecutionSequence() {
            public abstract IEnumerable<string> Sequence { get; }
            public abstract string Separator { get; }
            public abstract string InSequence { get; }
            protected abstract string[] Build();
        }

        public record LinuxProcessSequence(string Command, string CommandTitle, string LogFile) : ExecutionSequence {
            public override IEnumerable<string> Sequence => Build();
            public override string Separator => " ";
            public override string InSequence => @$"{string.Join(Separator, Sequence)}".TrimEnd();

            protected override string[] Build() {
                string header = $"{DateTime.Now} Command Timer has opened this window to run the following command.";
                string headerCommand = header.FormatMessageToEchoCommand();
                string headerCommandSendToLog = headerCommand.LinuxAppendToLogFile(LogFile);
                string message = $"Executing [{CommandTitle}] -> {Command.Replace("'", "\\'").SurroundWithSingleQuotes()}";
                string messageCommand = message.FormatMessageToEchoCommand();
                string messageCommandSendToLog = messageCommand.LinuxAppendToLogFile(LogFile);
                string messageSeparatorCommand = GetMessageBottomSeparator().FormatMessageToEchoCommand();
                string messageSeparatorCommandSendToLog = messageSeparatorCommand.LinuxAppendToLogFile(LogFile);
                string logFileSeparator = GetLogFileSeparator().FormatMessageToEchoCommand().LinuxAppendToLogFileNoShow(LogFile);

                return [
                    @$"{headerCommandSendToLog};",
                    @$"{messageCommandSendToLog};",
                    @$"{messageSeparatorCommandSendToLog};",
                    @$"{Command};",
                    @$"{logFileSeparator};",
                ];
            }
        }

        public record WindowsProcessSequence(string Command, string CommandTitle, string LogFile) : ExecutionSequence {
            public override IEnumerable<string> Sequence => Build();
            public override string Separator => " ";
            public override string InSequence => @$"{string.Join(Separator, Sequence)}".TrimEnd();

            protected override string[] Build() {
                string header = $"{DateTime.Now} Command Timer has opened this window to run the following command.";
                string headerCommand = header.FormatMessageToEchoCommand();
                string headerCommandSendToLog = headerCommand.PowerShellAppendToLogFile(LogFile);
                string message = $"Executing [{CommandTitle}] -> {PowerShellEscapeSingleQuotes(Command)}";
                string messageCommand = message.FormatMessageToEchoCommand();
                string messageCommandSendToLog = messageCommand.PowerShellAppendToLogFile(LogFile);
                string messageSeparatorCommand = GetMessageBottomSeparator().FormatMessageToEchoCommand();
                string messageSeparatorCommandSendToLog = messageSeparatorCommand.PowerShellAppendToLogFile(LogFile);
                string logFileSeparator = GetLogFileSeparator().SurroundWithSingleQuotes().PowerShellAppendToLogFile(LogFile);

                return [
                    @$"{headerCommand};",
                    @$"{headerCommandSendToLog};",
                    @$"{messageCommand};",
                    @$"{messageCommandSendToLog};",
                    @$"{messageSeparatorCommand};",
                    @$"{messageSeparatorCommandSendToLog};",
                    @$"{Command};",
                    @$"{logFileSeparator};",
                ];
            }
        }
    }
}
