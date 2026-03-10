namespace CommandTimer.Core.Interfaces;

/// <summary>
/// Service contract for shared timer pool management.
/// Provides subscription-based timers to avoid per-instance timer proliferation.
/// Implementations manage timer lifecycle, cleanup, and diagnostics.
/// </summary>
public interface ITimerProvider {

    /// <summary>
    /// Subscribe to a shared timer identified by key and interval.
    /// Multiple subscribers to the same key+interval share one underlying timer.
    /// </summary>
    /// <param name="key">Unique identifier for the timer group.</param>
    /// <param name="milliseconds">Timer interval in milliseconds.</param>
    /// <param name="eventHandler">Handler to invoke on each tick.</param>
    /// <exception cref="ArgumentOutOfRangeException">Key exists but with different interval.</exception>
    void Subscribe(string key, int milliseconds, EventHandler eventHandler);

    /// <summary>
    /// Unsubscribe from a shared timer.
    /// Timer is stopped and removed when last subscriber unsubscribes (handled by cleanup).
    /// </summary>
    /// <param name="key">Unique identifier for the timer group.</param>
    /// <param name="milliseconds">Timer interval in milliseconds (must match subscription).</param>
    /// <param name="eventHandler">Handler to remove from tick event.</param>
    /// <exception cref="ArgumentOutOfRangeException">Key exists but with different interval.</exception>
    void Unsubscribe(string key, int milliseconds, EventHandler eventHandler);

    /// <summary>
    /// Stops and removes timers that have zero subscribers.
    /// </summary>
    /// <returns>Number of timers cleaned up.</returns>
    int CleanUp();

    /// <summary>
    /// Returns the number of subscribers for a given key, or -1 if the key does not exist.
    /// </summary>
    int GetSubscriberCount(string key);

    /// <summary>
    /// Returns the total number of active timer groups.
    /// </summary>
    int ActiveTimerCount { get; }
}
