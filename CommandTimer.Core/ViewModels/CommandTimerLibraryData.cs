using System.Text.Json.Serialization;

namespace CommandTimer.Core.ViewModels;

/// <summary>
/// Pure serializable data for a command timer library.
/// No runtime loaded timers, no events, no lifecycle behavior.
/// </summary>
public class CommandTimerLibraryData {

    [JsonPropertyName("LibraryName")]
    public string LibraryName { get; set; } = Settings.Keys.DefaultLibrary;

    [JsonPropertyName("TimersByName")]
    public HashSet<string> TimerNames { get; set; } = [];
}
