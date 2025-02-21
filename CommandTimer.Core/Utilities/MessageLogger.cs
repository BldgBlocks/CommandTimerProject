using System;
using System.IO;
using static CommandTimer.Core.MessageRelay;

namespace CommandTimer.Core.Utilities;
public class MessageLogger {
    
    //...
    private const int CATEGORY_MIN_WIDTH = 10;
    private const int SENDER_MIN_WIDTH = 25;
    private string _logPath = string.Empty;
    private MessageCategory _category = MessageCategory.Any;

    //...
    public static MessageLogger Create(string logFilePath, MessageCategory category = MessageCategory.Any) {
        if (Path.GetDirectoryName(logFilePath) is string directoryPath) {
            Core.IOUtils.CreateFileAtPath(directoryPath, logFilePath);
        }
        return new() {
            _logPath = logFilePath,
            _category = category
        };
    }

    public void StartLogging() 
        => MessagePosted += MessageRelay_MessagePosted;

    public void StopLogging() 
        => MessagePosted -= MessageRelay_MessagePosted;

    //...
    private async void MessageRelay_MessagePosted(object? sender, MessageEventArgs args) {
        if (File.Exists(_logPath) is false) return;
        if (args.Category < _category) return;
        if (Core.Settings.ShouldLog.Value is false && args.Category <= MessageCategory.User) return;

        try {
            var senderString = sender?.ToString() ?? string.Empty;
            var date = $"{DateTime.Now}:";
            var send = $"[Sender] {senderString.Substring(senderString.LastIndexOf('.') + 1).PadRight(SENDER_MIN_WIDTH)}";
            var category = $"[Category] {args.Category.ToString().PadRight(CATEGORY_MIN_WIDTH)}";
            var message = $"[Message] {args.Message}";

            await Core.IOUtils.AppendToFile(_logPath, $"{date} {send} {category} {message}");
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) {
            throw new Exception($"Method: {nameof(MessageRelay_MessagePosted)} has thrown the following.", ex);
        }
    }
}
