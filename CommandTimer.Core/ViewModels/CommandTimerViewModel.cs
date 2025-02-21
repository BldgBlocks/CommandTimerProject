using Avalonia;
using Avalonia.Media;
using CommandTimer.Core.Utilities;
using CommandTimer.Core.ViewModels.MenuItems;
using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading.Tasks;


namespace CommandTimer.Core.ViewModels;

public partial class CommandTimerViewModel : ViewModelBase {

    public CommandTimerViewModel() {
        Subscribe();

    }

    public static CommandTimerViewModel Create(string name = PLACEHOLDER_NAME) {
        var viewModel = new CommandTimerViewModel() { Name = name };
        return viewModel;
    }

    public static CommandTimerViewModel Clone(CommandTimerViewModel viewModel) {
        var clone = Create();
        Type type = typeof(CommandTimerViewModel);
        foreach (var propDest in type.GetProperties(BindingFlags.Instance | BindingFlags.Public)) {
            if (propDest.CanWrite is false) continue;
            if (type.GetProperty(propDest.Name) == null) continue;

            propDest.SetValue(clone, propDest.GetValue(viewModel));
        }
        return clone;
    }

    private void Subscribe() {
        /// Runs all the time for UI
        WillCall.Subscribe(Core.Settings.Keys.WillCall_Key_On100ms, Core.Settings.Keys.WillCall_Interval_On100ms, EventHandler_UpdateCountdown);
        /// Runs only when active.
        WillCall.Subscribe(WillCallExecutionKey, ExecutionUpdateInterval.Milliseconds, EventHandler_CheckExecution);
    }

    public void Unsubscribe() {
        WillCall.Unsubscribe(Core.Settings.Keys.WillCall_Key_On100ms, Core.Settings.Keys.WillCall_Interval_On100ms, EventHandler_UpdateCountdown);
        WillCall.Unsubscribe(WillCallExecutionKey, ExecutionUpdateInterval.Milliseconds, EventHandler_CheckExecution);
    }


    //... Fields

    public static readonly TimeSpan ExecutionThreshold = TimeSpan.FromMilliseconds(500);
    public static readonly TimeSpan ExecutionUpdateInterval = TimeSpan.FromMilliseconds(100);
    public static readonly TimeSpan MinimumTimer = TimeSpan.FromSeconds(10);
    public static readonly TimeSpan OneDay = TimeSpan.FromDays(1);

    public const string PLACEHOLDER_COMMAND = "Enter a command here...";
    public const string PLACEHOLDER_NAME = "Enter a name";
    public const string PLACEHOLDER_DESCRIPTION = "Enter a description";
    public string CountDownFormat => TimeSpanTillExecution.Duration() > OneDay ? @"dd\:hh\:mm\:ss" : @"hh\:mm\:ss";

    private const string WillCallExecutionKey = "CommandTimerItemExecution";

    private bool _executionGate;

    public enum TimeModeChoice { Duration, Time, Date }


    //...

    [JsonIgnore]
    public CommandTimerLibrary Library
        => LibraryManager.FindLibrary(LibraryName) ?? LibraryManager.CurrentLibrary;

    public override void Serialize()
        => ServiceProvider.Get<ISerializer>().Serialize($"{Core.Settings.Keys.CommandTimerPrefix}{Name}", this, LibraryName);

    public override void Unserialize()
        => ServiceProvider.Get<ISerializer>().Unserialize($"{Core.Settings.Keys.CommandTimerPrefix}{Name}", LibraryName);

    private void EventHandler_UpdateCountdown(object? sender, EventArgs args) {
        Update_Countdown();
    }

    private void EventHandler_CheckExecution(object? sender, EventArgs args) {
        CheckTimeTillExecution();
    }

    public void Update_Countdown()
        => CountdownTillExecution = TimeSpanTillExecution.ToString(CountDownFormat);

    private void CheckTimeTillExecution() {
        if (IsActive is false) return;
        if (TimeSpanTillExecution < ExecutionThreshold) {
            if (IsLoop && Enum.Equals(TimeMode, TimeModeChoice.Time | TimeModeChoice.Duration)) {
                StartTime = StartTime.Add(TargetTimeSpanTillExecution);

                if (StartTime < DateTime.Now) StartTime = DateTime.Now;

                IsActive = true;
            }
            else {
                /// Leave start time as the last start time.
                IsActive = false;
            }

            if (Core.Settings.ShouldExecuteOnTimer.Value) {
                Core.MessageRelay.OnMessagePosted(this, $"Executing [{Name}]", Core.MessageRelay.MessageCategory.User, Core.Settings.ShouldAutoNotificationsExpire.Value ? 0 : 100);
                ExecuteCommand();
            }
        }
    }

    public async void ExecuteCommand() {
        if (_executionGate) return;
        _executionGate = true;

        if (string.IsNullOrWhiteSpace(Command) || Command == CommandTimerViewModel.PLACEHOLDER_COMMAND) {
            Core.MessageRelay.OnMessagePosted(this, $"Executing [{Name}]: The command is empty...", Core.MessageRelay.MessageCategory.User);

            _executionGate = false;
            return;
        }

        SystemInteraction.Execute.Command(Command, Name, IsShowTerminal);

        /// Minimum time removes accidental double clicks and such.
        int minDelay = 1;
        /// If active timer should loop, then ensure time is not so fast that the user can not respond.
        if (IsLoop && IsActive && TargetTimeSpanTillExecution <= TimeSpan.FromSeconds(5)) {
            Core.MessageRelay.OnMessagePosted(this, $"Executing [{Name}]: A minimum execution timer is being enforced.", Core.MessageRelay.MessageCategory.User);
            minDelay = 5;
        }

        await Task.Delay(TimeSpan.FromSeconds(minDelay));

        _executionGate = false;
    }

    public void IndexBackgroundStripe(int index) {
        if (index % 2 == 0) {
            StripeBackground();
        }
        else {
            ResetBackground();
        }
    }

    public void StripeBackground() => Background = Core.Colors.ApplicationBrush_Stripe;
    public void ResetBackground() => Background = Core.Colors.ApplicationBrush_Transparent;


    //... Bindings

    [JsonInclude]
    [JsonPropertyName("Background")]
    private SolidColorBrush _Background = Core.Colors.ApplicationBrush_Transparent;
    [JsonIgnore]
    public SolidColorBrush Background { get => _Background; set => SetProperty(ref _Background, value, Save.No, Notify.Yes); }


    [JsonInclude]
    [JsonPropertyName("Accent")]
    private SolidColorBrush _Accent = Core.Colors.ApplicationBrush_Accent;
    [JsonIgnore]
    public SolidColorBrush Accent { get => _Accent; set => SetProperty(ref _Accent, value, Save.No, Notify.Yes); }

    [JsonIgnore]
    public ObservableCollection<MenuItemViewModel> CopyMoveSelections { get; } = [];

    [JsonInclude]
    [JsonPropertyName("Name")]
    private string _Name = PLACEHOLDER_NAME;
    [JsonIgnore]
    public string Name { get => _Name; set => SetProperty(ref _Name, value, Save.No, Notify.Yes); }

    [JsonInclude]
    [JsonPropertyName("LibraryName")]
    private string _Library = LibraryManager.DEFAULT_LIBRARY;
    [JsonIgnore]
    public string LibraryName { get => _Library; set => SetProperty(ref _Library, value, Save.No, Notify.Yes); }

    [JsonInclude]
    [JsonPropertyName("Description")]
    private string _Description = PLACEHOLDER_DESCRIPTION;
    [JsonIgnore]
    public string Description { get => _Description; set => SetProperty(ref _Description, value, Save.Yes, Notify.Yes); }


    [JsonInclude]
    [JsonPropertyName("Command")]
    private string _Command = PLACEHOLDER_COMMAND;
    [JsonIgnore]
    public string Command { get => _Command; set => SetProperty(ref _Command, value, Save.Yes, Notify.Yes); }


    [JsonInclude]
    [JsonPropertyName("IsLog")]
    private bool _IsLog = true;
    [JsonIgnore]
    public bool IsLog { get => _IsLog; set => SetProperty(ref _IsLog, value, Save.Yes, Notify.Yes); }


    [JsonInclude]
    [JsonPropertyName("IsShowTerminal")]
    private bool _IsShowTerminal;
    [JsonIgnore]
    public bool IsShowTerminal { get => _IsShowTerminal; set => SetProperty(ref _IsShowTerminal, value, Save.Yes, Notify.Yes); }


    [JsonInclude]
    [JsonPropertyName("IsTimeMode")]
    private TimeModeChoice _IsTimeMode;
    [JsonIgnore]
    public TimeModeChoice TimeMode { get => _IsTimeMode; set => SetProperty(ref _IsTimeMode, value, Save.Yes, Notify.Yes); }


    [JsonInclude]
    [JsonPropertyName("IsLoop")]
    private bool _IsLoop;
    [JsonIgnore]
    public bool IsLoop { get => _IsLoop; set => SetProperty(ref _IsLoop, value, Save.Yes, Notify.Yes); }


    [JsonInclude]
    [JsonPropertyName("IsAutoStart")]
    private bool _IsAutoStart;
    [JsonIgnore]
    public bool IsAutoStart { get => _IsAutoStart; set => SetProperty(ref _IsAutoStart, value, Save.Yes, Notify.Yes); }


    [JsonInclude]
    [JsonPropertyName("IsPromptForExecute")]
    private bool _IsPromptForExecute;
    [JsonIgnore]
    public bool IsPromptForExecute { get => _IsPromptForExecute; set => SetProperty(ref _IsPromptForExecute, value, Save.Yes, Notify.Yes); }


    [JsonIgnore]
    private bool _IsActive;
    [JsonIgnore]
    public bool IsActive {
        get => _IsActive;
        set {
            /// Prior to event call to avoid flicker in UI.
            if (value && !_IsActive) {
                StartTime = DateTime.Now;
            }
            SetProperty(ref _IsActive, value, Save.No, Notify.Yes);
        }
    }


    [JsonInclude]
    [JsonPropertyName("IsFavorite")]
    private bool _IsFavorite;
    [JsonIgnore]
    public bool IsFavorite { get => _IsFavorite; set => SetProperty(ref _IsFavorite, value, Save.Yes, Notify.Yes); }


    [JsonInclude]
    [JsonPropertyName("ColorBarColor")]
    private SolidColorBrush _ColorBarColor = Core.Colors.ApplicationBrush_Accent;
    [JsonIgnore]
    public SolidColorBrush ColorBarColor { get => _ColorBarColor; set => SetProperty(ref _ColorBarColor, value, Save.Yes, Notify.Yes); }


    [JsonInclude]
    [JsonPropertyName("TargetTimeSpan")]
    private TimeSpan _TargetTimeSpanTillExecution = MinimumTimer;
    [JsonIgnore]
    public TimeSpan TargetTimeSpanTillExecution {
        get => _TargetTimeSpanTillExecution;
        set {
            if (value < MinimumTimer) {
                Core.MessageRelay.OnMessagePosted(this, "A minimum timer value has been enforced. \r\nConsider changing settings.", Core.MessageRelay.MessageCategory.User);
                value = MinimumTimer;
            }

            SetProperty(ref _TargetTimeSpanTillExecution, value, Save.Yes, Notify.Yes);
        }
    }

    [JsonInclude]
    [JsonPropertyName("TargetDate")]
    private DateTime _TargetDateTillExecution = DateTime.Today.AddDays(1);
    [JsonIgnore]
    public DateTime TargetDateTillExecution {
        get => _TargetDateTillExecution;
        set {
            SetProperty(ref _TargetDateTillExecution, value, Save.Yes, Notify.Yes);
        }
    }

    [JsonInclude]
    [JsonPropertyName("StartTime")]
    private DateTime _StartTime;
    [JsonIgnore]
    public DateTime StartTime { get => _StartTime; set => SetProperty(ref _StartTime, value, Save.Yes, Notify.Yes); }


    [JsonIgnore]
    private string _CountdownTillExecution = MinimumTimer.ToString(@"hh\:mm\:ss");
    [JsonIgnore]
    public string CountdownTillExecution { get => _CountdownTillExecution; set => SetProperty(ref _CountdownTillExecution, value); }


    [JsonIgnore]
    private int _MaxLines = 1;
    [JsonIgnore]
    public int MaxLines { get => _MaxLines; set => SetProperty(ref _MaxLines, value); }


    [JsonIgnore]
    public TimeSpan TimeSpanTillExecution {
        get {
            switch (TimeMode) {
                case TimeModeChoice.Duration:
                    return IsActive ? StartTime.Add(TargetTimeSpanTillExecution).Subtract(DateTime.Now) : TargetTimeSpanTillExecution;
                case TimeModeChoice.Time:
                    var now1 = DateTime.Now;
                    var targetTimeToday = new DateTime(
                        now1.Year,
                        now1.Month,
                        now1.Day,
                        TargetTimeSpanTillExecution.Hours,
                        TargetTimeSpanTillExecution.Minutes,
                        TargetTimeSpanTillExecution.Seconds
                        );

                    /// If the target time for today has already passed, set it for tomorrow
                    if (targetTimeToday < now1) {
                        targetTimeToday = targetTimeToday.AddDays(1);
                    }

                    return (targetTimeToday - now1).Add(TimeSpan.FromDays(TargetTimeSpanTillExecution.Days));
                case TimeModeChoice.Date:
                    var now2 = DateTime.Now;
                    var targetTimeOfExecution = new DateTime(
                        TargetDateTillExecution.Year,
                        TargetDateTillExecution.Month,
                        TargetDateTillExecution.Day,
                        TargetTimeSpanTillExecution.Hours,
                        TargetTimeSpanTillExecution.Minutes,
                        TargetTimeSpanTillExecution.Seconds
                        );

                    /// If the target time for today has already passed.
                    if (targetTimeOfExecution < now2) {
                        var targetTimeToday2 = new DateTime(
                            now2.Year,
                            now2.Month,
                            now2.Day,
                            TargetTimeSpanTillExecution.Hours,
                            TargetTimeSpanTillExecution.Minutes,
                            TargetTimeSpanTillExecution.Seconds
                            );

                        if (targetTimeToday2 < now2) {
                            targetTimeToday2 = targetTimeToday2.AddDays(1);
                        }
                        return (targetTimeToday2 - now2).Add(TimeSpan.FromDays(TargetTimeSpanTillExecution.Days));
                    }

                    var diff = (targetTimeOfExecution - now2);
                    return diff;
                default:
                    throw new ArgumentOutOfRangeException($"{nameof(TimeMode)} enum value is not defined.");
            }
        }
    }


    //... Animated Transition Properties

    [JsonIgnore]
    private Double _Opacity;
    /// <summary>
    /// Not bound through axaml. The code behind binds a transition that monitors this property and is bound to a control.
    /// </summary>
    [JsonIgnore]
    public Double Opacity { get => _Opacity; set => SetProperty(ref _Opacity, value); }


    [JsonIgnore]
    private Thickness _PaddingLeft;
    /// <summary>
    /// Not bound through axaml. The code behind binds a transition that monitors this property and is bound to a control.
    /// </summary>
    /// 
    [JsonIgnore]
    public Thickness PaddingLeft { get => _PaddingLeft; set => SetProperty(ref _PaddingLeft, value); }

    public void TimeStrategyCommand(string parameter) {
        if (string.Equals(parameter, "Duration", StringComparison.OrdinalIgnoreCase)) {
            TimeMode = TimeModeChoice.Duration;
        }
        else if (string.Equals(parameter, "Time", StringComparison.OrdinalIgnoreCase)) {
            TimeMode = TimeModeChoice.Time;
        }
        else if (string.Equals(parameter, "Date", StringComparison.OrdinalIgnoreCase)) {
            TimeMode = TimeModeChoice.Date;
        }
    }
}
