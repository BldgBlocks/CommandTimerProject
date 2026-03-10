using System.Text.Json.Serialization;


namespace CommandTimer.Core.ViewModels;

/// <summary>
/// Pure serializable data for a command timer.
/// No behavior, no subscriptions, no UI bindings.
/// </summary>
public class CommandTimerData {

    //... Constants

    public const string PLACEHOLDER_COMMAND = "Enter a command here...";
    public const string PLACEHOLDER_NAME = "Enter a name";
    public const string PLACEHOLDER_DESCRIPTION = "Enter a description";

    public static readonly TimeSpan MinimumTimer = TimeSpan.FromSeconds(10);

    public enum TimeModeChoice { Duration, Time, Date }


    //... Serialized Properties

    [JsonPropertyName("Name")]
    public string Name { get; set; } = PLACEHOLDER_NAME;

    [JsonPropertyName("LibraryName")]
    public string LibraryName { get; set; } = Settings.Keys.DefaultLibrary;

    [JsonPropertyName("Description")]
    public string Description { get; set; } = PLACEHOLDER_DESCRIPTION;

    [JsonPropertyName("Command")]
    public string Command { get; set; } = PLACEHOLDER_COMMAND;

    [JsonPropertyName("IsLog")]
    public bool IsLog { get; set; } = true;

    [JsonPropertyName("IsShowTerminal")]
    public bool IsShowTerminal { get; set; }

    [JsonPropertyName("IsTimeMode")]
    public TimeModeChoice TimeMode { get; set; }

    [JsonPropertyName("IsLoop")]
    public bool IsLoop { get; set; }

    [JsonPropertyName("IsAutoStart")]
    public bool IsAutoStart { get; set; }

    [JsonPropertyName("IsPromptForExecute")]
    public bool IsPromptForExecute { get; set; }

    [JsonPropertyName("IsFavorite")]
    public bool IsFavorite { get; set; }

    [JsonPropertyName("ColorBarColor")]
    public AppColor ColorBarColor { get; set; } = ServiceProvider.Get<IColorProvider>().ApplicationBrush_Accent.Value;

    [JsonPropertyName("TargetTimeSpan")]
    public TimeSpan TargetTimeSpanTillExecution { get; set; } = MinimumTimer;

    [JsonPropertyName("TargetDate")]
    public DateTime TargetDateTillExecution { get; set; } = DateTime.Today.AddDays(1);

    [JsonPropertyName("StartTime")]
    public DateTime StartTime { get; set; }
}
