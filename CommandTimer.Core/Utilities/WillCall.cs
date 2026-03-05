using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CommandTimer.Core.Utilities;
public static class WillCall {

    private static readonly Dictionary<string, DispatcherTimer> _willCallDirectory = [];
    private static readonly Dictionary<string, int> _subscriberCounts = [];

    private const string CleanUpKey = "WillCall_CleanUp";
    private const int CleanUpIntervalMs = 600_000; // 10 minutes

    //...

    static WillCall() {
        var cleanUpTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(CleanUpIntervalMs) };
        _willCallDirectory[CleanUpKey] = cleanUpTimer;
        _subscriberCounts[CleanUpKey] = 1;
        cleanUpTimer.Tick += (_, _) => CleanUp();
        cleanUpTimer.Start();
    }

    public static void Subscribe(string key, int milliseconds, EventHandler eventHandler) {
        if (_willCallDirectory.TryGetValue(key, out var timer) is false) {
            timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(milliseconds) };
            _willCallDirectory[key] = timer;
            _subscriberCounts[key] = 0;
            timer.Start();
        }
        if (timer.Interval != TimeSpan.FromMilliseconds(milliseconds)) {
            throw new ArgumentOutOfRangeException($"The willcall service has recieved a key {key} with an unexpected interval {milliseconds}.");
        }

        timer.Tick += eventHandler;
        _subscriberCounts[key]++;
    }

    public static void Unsubscribe(string key, int milliseconds, EventHandler eventHandler) {
        if (_willCallDirectory.TryGetValue(key, out var timer) is false) return;
        if (timer.Interval != TimeSpan.FromMilliseconds(milliseconds)) {
            throw new ArgumentOutOfRangeException($"The willcall service has recieved a key {key} with an unexpected interval {milliseconds}.");
        }

        timer.Tick -= eventHandler;
        if (_subscriberCounts.TryGetValue(key, out var count)) {
            _subscriberCounts[key] = Math.Max(0, count - 1);
        }
    }

    //... Maintenance

    /// <summary>
    /// Stops and removes timers that have zero subscribers.
    /// Returns the number of timers cleaned up.
    /// </summary>
    public static int CleanUp() {
        var idle = _subscriberCounts
            .Where(kvp => kvp.Value <= 0)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in idle) {
            if (_willCallDirectory.TryGetValue(key, out var timer)) {
                timer.Stop();
                _willCallDirectory.Remove(key);
            }
            _subscriberCounts.Remove(key);
        }

        return idle.Count;
    }

    //... Diagnostics

    /// <summary>
    /// Returns the number of subscribers for a given key, or -1 if the key does not exist.
    /// </summary>
    public static int GetSubscriberCount(string key)
        => _subscriberCounts.TryGetValue(key, out var count) ? count : -1;

    /// <summary>
    /// Returns the total number of active timer groups.
    /// </summary>
    public static int ActiveTimerCount => _willCallDirectory.Count;

    /// <summary>
    /// Returns a snapshot of all keys and their subscriber counts.
    /// </summary>
    public static IReadOnlyDictionary<string, int> GetSnapshot()
        => new Dictionary<string, int>(_subscriberCounts);
}
