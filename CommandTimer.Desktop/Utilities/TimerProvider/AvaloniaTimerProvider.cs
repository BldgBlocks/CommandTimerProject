using Avalonia.Threading;
using CommandTimer.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CommandTimer.Desktop.Utilities.TimerProvider;

/// <summary>
/// Avalonia-specific implementation of ITimerProvider using DispatcherTimer.
/// Manages shared timer pool to avoid per-instance timer proliferation.
/// Self-cleaning: idle timers are removed every 10 minutes.
/// </summary>
public class AvaloniaTimerProvider : ITimerProvider {

    private readonly Dictionary<string, DispatcherTimer> _timers = [];
    private readonly Dictionary<string, int> _subscriberCounts = [];

    private const string CleanUpKey = "TimerProvider_CleanUp";
    private const int CleanUpIntervalMs = 600_000; // 10 minutes

    public AvaloniaTimerProvider() {
        var cleanUpTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(CleanUpIntervalMs) };
        _timers[CleanUpKey] = cleanUpTimer;
        _subscriberCounts[CleanUpKey] = 1;
        cleanUpTimer.Tick += (_, _) => CleanUp();
        cleanUpTimer.Start();
    }

    public void Subscribe(string key, int milliseconds, EventHandler eventHandler) {
        if (_timers.TryGetValue(key, out var timer) is false) {
            timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(milliseconds) };
            _timers[key] = timer;
            _subscriberCounts[key] = 0;
            timer.Start();
        }
        
        if (timer.Interval != TimeSpan.FromMilliseconds(milliseconds)) {
            throw new ArgumentOutOfRangeException($"TimerProvider: Key '{key}' already exists with interval {timer.Interval.TotalMilliseconds}ms, cannot subscribe with {milliseconds}ms.");
        }

        timer.Tick += eventHandler;
        _subscriberCounts[key]++;
    }

    public void Unsubscribe(string key, int milliseconds, EventHandler eventHandler) {
        if (_timers.TryGetValue(key, out var timer) is false) return;
        
        if (timer.Interval != TimeSpan.FromMilliseconds(milliseconds)) {
            throw new ArgumentOutOfRangeException($"TimerProvider: Key '{key}' exists with interval {timer.Interval.TotalMilliseconds}ms, cannot unsubscribe with {milliseconds}ms.");
        }

        timer.Tick -= eventHandler;
        if (_subscriberCounts.TryGetValue(key, out var count)) {
            _subscriberCounts[key] = Math.Max(0, count - 1);
        }
    }

    public int CleanUp() {
        var idle = _subscriberCounts
            .Where(kvp => kvp.Value <= 0)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in idle) {
            if (_timers.TryGetValue(key, out var timer)) {
                timer.Stop();
                _timers.Remove(key);
            }
            _subscriberCounts.Remove(key);
        }

        return idle.Count;
    }

    public int GetSubscriberCount(string key)
        => _subscriberCounts.TryGetValue(key, out var count) ? count : -1;

    public int ActiveTimerCount => _timers.Count;
}
