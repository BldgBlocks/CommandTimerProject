using System;

namespace CommandTimer.Core;
public static class ActionRelay {

    public static event EventHandler<ActionEventArgs>? ActionPosted;

    /// <summary>
    /// Global actions can be communicated via string key.
    /// Such as having static methods trigger events on instances throughout.
    /// </summary>
    public static void OnActionPosted(object sender, string actionKey, int priority = 0) {
        OnActionPosted(sender, new ActionEventArgs() {
            ActionKey = actionKey,
            Priority = priority
        });
    }

    public static void OnActionPosted(object sender, ActionEventArgs args) => ActionPosted?.Invoke(sender, args);

    public class ActionEventArgs : EventArgs {
        public required string ActionKey { get; set; }
        public int Priority { get; set; }
        public object? Data { get; set; }
    }
}
