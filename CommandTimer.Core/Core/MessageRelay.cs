using System;

namespace CommandTimer.Core;

public static class MessageRelay {

    /// <summary>
    /// Cascading priority selection, all categories with higher priority will be included.
    /// </summary>
    public enum MessageCategory {
        Any = 0,
        User = 1,
        Log = 2,
        Exception = 3
    }

    public static event EventHandler<MessageEventArgs>? MessagePosted;

    public static void OnMessagePosted(object sender, string message, MessageCategory category = MessageCategory.Any, int priority = 0) {
        OnMessagePosted(sender, new MessageEventArgs() {
            Message = message,
            Category = category,
            Priority = priority
        });
    }

    public static void OnMessagePosted(object sender, MessageEventArgs args) => MessagePosted?.Invoke(sender, args);

    public class MessageEventArgs : EventArgs {
        public required string Message { get; set; }
        public MessageCategory Category { get; set; }
        public int Priority { get; set; }
    }
}
