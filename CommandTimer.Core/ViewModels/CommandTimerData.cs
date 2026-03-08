using Avalonia.Media;
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


    //... Serialized Fields & Properties

    [JsonInclude]
    [JsonPropertyName("Name")]
    private string _Name = PLACEHOLDER_NAME;
    [JsonIgnore]
    public string Name { get => _Name; set => _Name = value; }

    [JsonInclude]
    [JsonPropertyName("LibraryName")]
    private string _Library = Settings.Keys.DefaultLibrary;
    [JsonIgnore]
    public string LibraryName { get => _Library; set => _Library = value; }

    [JsonInclude]
    [JsonPropertyName("Description")]
    private string _Description = PLACEHOLDER_DESCRIPTION;
    [JsonIgnore]
    public string Description { get => _Description; set => _Description = value; }

    [JsonInclude]
    [JsonPropertyName("Command")]
    private string _Command = PLACEHOLDER_COMMAND;
    [JsonIgnore]
    public string Command { get => _Command; set => _Command = value; }

    [JsonInclude]
    [JsonPropertyName("IsLog")]
    private bool _IsLog = true;
    [JsonIgnore]
    public bool IsLog { get => _IsLog; set => _IsLog = value; }

    [JsonInclude]
    [JsonPropertyName("IsShowTerminal")]
    private bool _IsShowTerminal;
    [JsonIgnore]
    public bool IsShowTerminal { get => _IsShowTerminal; set => _IsShowTerminal = value; }

    [JsonInclude]
    [JsonPropertyName("IsTimeMode")]
    private TimeModeChoice _IsTimeMode;
    [JsonIgnore]
    public TimeModeChoice TimeMode { get => _IsTimeMode; set => _IsTimeMode = value; }

    [JsonInclude]
    [JsonPropertyName("IsLoop")]
    private bool _IsLoop;
    [JsonIgnore]
    public bool IsLoop { get => _IsLoop; set => _IsLoop = value; }

    [JsonInclude]
    [JsonPropertyName("IsAutoStart")]
    private bool _IsAutoStart;
    [JsonIgnore]
    public bool IsAutoStart { get => _IsAutoStart; set => _IsAutoStart = value; }

    [JsonInclude]
    [JsonPropertyName("IsPromptForExecute")]
    private bool _IsPromptForExecute;
    [JsonIgnore]
    public bool IsPromptForExecute { get => _IsPromptForExecute; set => _IsPromptForExecute = value; }

    [JsonInclude]
    [JsonPropertyName("IsFavorite")]
    private bool _IsFavorite;
    [JsonIgnore]
    public bool IsFavorite { get => _IsFavorite; set => _IsFavorite = value; }

    [JsonInclude]
    [JsonPropertyName("ColorBarColor")]
    private SolidColorBrush _ColorBarColor = ServiceProvider.Get<IColorProvider>().ApplicationBrush_Accent.Value;
    [JsonIgnore]
    public SolidColorBrush ColorBarColor { get => _ColorBarColor; set => _ColorBarColor = value; }

    [JsonInclude]
    [JsonPropertyName("TargetTimeSpan")]
    private TimeSpan _TargetTimeSpanTillExecution = MinimumTimer;
    [JsonIgnore]
    public TimeSpan TargetTimeSpanTillExecution { get => _TargetTimeSpanTillExecution; set => _TargetTimeSpanTillExecution = value; }

    [JsonInclude]
    [JsonPropertyName("TargetDate")]
    private DateTime _TargetDateTillExecution = DateTime.Today.AddDays(1);
    [JsonIgnore]
    public DateTime TargetDateTillExecution { get => _TargetDateTillExecution; set => _TargetDateTillExecution = value; }

    [JsonInclude]
    [JsonPropertyName("StartTime")]
    private DateTime _StartTime;
    [JsonIgnore]
    public DateTime StartTime { get => _StartTime; set => _StartTime = value; }
}
