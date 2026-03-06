using Avalonia;
using Avalonia.Media;
using CommandTimer.Core.Utilities;
using CommandTimer.Core.ViewModels.MenuItems;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Threading.Tasks;


namespace CommandTimer.Core.ViewModels;

public partial class CommandTimerViewModel : ViewModelBase {

    public CommandTimerViewModel() {
        Data = new CommandTimerData();
        Subscribe();
    }

    public CommandTimerViewModel(CommandTimerData data) {
        Data = data;
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
    public static readonly TimeSpan MinimumTimer = CommandTimerData.MinimumTimer;
    public static readonly TimeSpan OneDay = TimeSpan.FromDays(1);

    public const string PLACEHOLDER_COMMAND = CommandTimerData.PLACEHOLDER_COMMAND;
    public const string PLACEHOLDER_NAME = CommandTimerData.PLACEHOLDER_NAME;
    public const string PLACEHOLDER_DESCRIPTION = CommandTimerData.PLACEHOLDER_DESCRIPTION;
    public string CountDownFormat => TimeSpanTillExecution.Duration() > OneDay ? @"dd\:hh\:mm\:ss" : @"hh\:mm\:ss";

    private const string WillCallExecutionKey = "CommandTimerItemExecution";

    private bool _executionGate;

    public enum TimeModeChoice { Duration, Time, Date }


    //... Data

    internal CommandTimerData Data { get; }


    //...

    public CommandTimerLibrary Library
        => LibraryManager.FindLibrary(LibraryName) ?? LibraryManager.CurrentLibrary;

    public override void Serialize()
        => ServiceProvider.Get<ISerializer>().Serialize($"{Core.Settings.Keys.CommandTimerPrefix}{Name}", Data, LibraryName);

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


    //... Bindings — UI-only properties (not serialized)

    private SolidColorBrush _Background = Core.Colors.ApplicationBrush_Transparent;
    public SolidColorBrush Background { get => _Background; set => SetProperty(ref _Background, value, Save.No, Notify.Yes); }


    private SolidColorBrush _Accent = Core.Colors.ApplicationBrush_Accent;
    public SolidColorBrush Accent { get => _Accent; set => SetProperty(ref _Accent, value, Save.No, Notify.Yes); }

    public ObservableCollection<MenuItemViewModel> CopyMoveSelections { get; } = [];


    //... Bindings — Delegated to Data (ViewModel setters add Save/Notify behavior)

    public string Name {
        get => Data.Name;
        set { var old = Data.Name; Data.Name = value; SetPropertyNotify(old, value); }
    }

    public string LibraryName {
        get => Data.LibraryName;
        set { var old = Data.LibraryName; Data.LibraryName = value; SetPropertyNotify(old, value); }
    }

    public string Description {
        get => Data.Description;
        set { var old = Data.Description; Data.Description = value; SetPropertySaveNotify(old, value); }
    }

    public string Command {
        get => Data.Command;
        set { var old = Data.Command; Data.Command = value; SetPropertySaveNotify(old, value); }
    }

    public bool IsLog {
        get => Data.IsLog;
        set { var old = Data.IsLog; Data.IsLog = value; SetPropertySaveNotify(old, value); }
    }

    public bool IsShowTerminal {
        get => Data.IsShowTerminal;
        set { var old = Data.IsShowTerminal; Data.IsShowTerminal = value; SetPropertySaveNotify(old, value); }
    }

    public TimeModeChoice TimeMode {
        get => (TimeModeChoice)Data.TimeMode;
        set { var old = (TimeModeChoice)Data.TimeMode; Data.TimeMode = (CommandTimerData.TimeModeChoice)value; SetPropertySaveNotify(old, value); }
    }

    public bool IsLoop {
        get => Data.IsLoop;
        set { var old = Data.IsLoop; Data.IsLoop = value; SetPropertySaveNotify(old, value); }
    }

    public bool IsAutoStart {
        get => Data.IsAutoStart;
        set { var old = Data.IsAutoStart; Data.IsAutoStart = value; SetPropertySaveNotify(old, value); }
    }

    public bool IsPromptForExecute {
        get => Data.IsPromptForExecute;
        set { var old = Data.IsPromptForExecute; Data.IsPromptForExecute = value; SetPropertySaveNotify(old, value); }
    }

    public bool IsFavorite {
        get => Data.IsFavorite;
        set { var old = Data.IsFavorite; Data.IsFavorite = value; SetPropertySaveNotify(old, value); }
    }

    public SolidColorBrush ColorBarColor {
        get => Data.ColorBarColor;
        set { var old = Data.ColorBarColor; Data.ColorBarColor = value; SetPropertySaveNotify(old, value); }
    }

    public TimeSpan TargetTimeSpanTillExecution {
        get => Data.TargetTimeSpanTillExecution;
        set {
            if (value < MinimumTimer) {
                Core.MessageRelay.OnMessagePosted(this, "A minimum timer value has been enforced. \r\nConsider changing settings.", Core.MessageRelay.MessageCategory.User);
                value = MinimumTimer;
            }
            var old = Data.TargetTimeSpanTillExecution;
            Data.TargetTimeSpanTillExecution = value;
            SetPropertySaveNotify(old, value);
        }
    }

    public DateTime TargetDateTillExecution {
        get => Data.TargetDateTillExecution;
        set { var old = Data.TargetDateTillExecution; Data.TargetDateTillExecution = value; SetPropertySaveNotify(old, value); }
    }

    public DateTime StartTime {
        get => Data.StartTime;
        set { var old = Data.StartTime; Data.StartTime = value; SetPropertySaveNotify(old, value); }
    }


    private bool _IsActive;
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


    private string _CountdownTillExecution = CommandTimerData.MinimumTimer.ToString(@"hh\:mm\:ss");
    public string CountdownTillExecution { get => _CountdownTillExecution; set => SetProperty(ref _CountdownTillExecution, value); }


    private int _MaxLines = 1;
    public int MaxLines { get => _MaxLines; set => SetProperty(ref _MaxLines, value); }


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

    private Double _Opacity;
    /// <summary>
    /// Not bound through axaml. The code behind binds a transition that monitors this property and is bound to a control.
    /// </summary>
    public Double Opacity { get => _Opacity; set => SetProperty(ref _Opacity, value); }


    private Thickness _PaddingLeft;
    /// <summary>
    /// Not bound through axaml. The code behind binds a transition that monitors this property and is bound to a control.
    /// </summary>
    /// 
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


    //... Delegation Helpers

    /// <summary>
    /// Notify-only: value already written to Data. Fires PropertyChanged if changed.
    /// </summary>
    private void SetPropertyNotify<T>(T oldValue, T newValue, [System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null) {
        if (!EqualityComparer<T>.Default.Equals(oldValue, newValue)) {
            OnPropertyChanged(propertyName);
        }
    }

    /// <summary>
    /// Save + Notify: value already written to Data. Fires PropertyChanged and serializes if changed.
    /// </summary>
    private void SetPropertySaveNotify<T>(T oldValue, T newValue, [System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null) {
        if (!EqualityComparer<T>.Default.Equals(oldValue, newValue)) {
            OnPropertyChanged(propertyName);
            if (ShouldSerialize) {
                Serialize();
            }
        }
    }
}
