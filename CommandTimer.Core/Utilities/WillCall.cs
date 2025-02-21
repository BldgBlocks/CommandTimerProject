using Avalonia.Threading;
using System;
using System.Collections.Generic;

namespace CommandTimer.Core.Utilities;
public static class WillCall {

    private static readonly Dictionary<string, DispatcherTimer> _willCallDirectory = [];

    //...

    public static void Subscribe(string key, int milliseconds, EventHandler eventHandler) {
        if (_willCallDirectory.TryGetValue(key, out var timer) is false) {
            timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(milliseconds) };
            _willCallDirectory[key] = timer;
            timer.Start();
        }
        if (timer.Interval != TimeSpan.FromMilliseconds(milliseconds)) {
            throw new ArgumentOutOfRangeException($"The willcall service has recieved a key {key} with an unexpected interval {milliseconds}.");
        }

        timer.Tick += eventHandler;
    }

    public static void Unsubscribe(string key, int milliseconds, EventHandler eventHandler) {
        if (_willCallDirectory.TryGetValue(key, out var timer) is false) return;
        if (timer.Interval != TimeSpan.FromMilliseconds(milliseconds)) {
            throw new ArgumentOutOfRangeException($"The willcall service has recieved a key {key} with an unexpected interval {milliseconds}.");
        }

        timer.Tick -= eventHandler;
    }
}
