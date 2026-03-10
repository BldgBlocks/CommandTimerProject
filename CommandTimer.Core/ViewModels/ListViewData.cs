using System.Text.Json.Serialization;

namespace CommandTimer.Core.ViewModels;

/// <summary>
/// Pure serializable data for the list view.
/// No behavior, no subscriptions, no UI bindings.
/// </summary>
public class ListViewData {

    [JsonPropertyName("LibrarySelection")]
    public string LibrarySelection { get; set; } = Settings.Keys.DefaultLibrary;

    [JsonPropertyName("QuickFilter")]
    public string QuickFilter { get; set; } = string.Empty;

    [JsonPropertyName("SortStrategy")]
    public string SortStrategy { get; set; } = string.Empty;

    [JsonPropertyName("SortDirection")]
    public string SortDirection { get; set; } = string.Empty;
}
