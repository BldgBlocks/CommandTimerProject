using CommandTimer.Core.Utilities;
using System;
using System.Threading.Tasks;


namespace CommandTimer.Core.ViewModels;

/// <summary>
/// Execution logic for a command timer.
/// Subscribes to WillCall for countdown updates and execution checks.
/// Reads and writes through the ViewModel.
/// </summary>
public class CommandTimerEngine {

    public CommandTimerEngine(CommandTimerViewModel viewModel) {
        _viewModel = viewModel;
        Subscribe();
    }


    //... Fields

    public static readonly TimeSpan ExecutionThreshold = TimeSpan.FromMilliseconds(500);
    public static readonly TimeSpan ExecutionUpdateInterval = TimeSpan.FromMilliseconds(100);

    private const string WillCallExecutionKey = "CommandTimerItemExecution";

    private bool _executionGate;

    private readonly CommandTimerViewModel _viewModel;


    //... Subscription

    public void Subscribe() {
        /// Runs all the time for UI
        WillCall.Subscribe(Core.Settings.Keys.WillCall_Key_On100ms, Core.Settings.Keys.WillCall_Interval_On100ms, EventHandler_UpdateCountdown);
        /// Runs only when active.
        WillCall.Subscribe(WillCallExecutionKey, ExecutionUpdateInterval.Milliseconds, EventHandler_CheckExecution);
    }

    public void Unsubscribe() {
        WillCall.Unsubscribe(Core.Settings.Keys.WillCall_Key_On100ms, Core.Settings.Keys.WillCall_Interval_On100ms, EventHandler_UpdateCountdown);
        WillCall.Unsubscribe(WillCallExecutionKey, ExecutionUpdateInterval.Milliseconds, EventHandler_CheckExecution);
    }


    //... Event Handlers

    private void EventHandler_UpdateCountdown(object? sender, EventArgs args) {
        _viewModel.Update_Countdown();
    }

    private void EventHandler_CheckExecution(object? sender, EventArgs args) {
        CheckTimeTillExecution();
    }


    //... Execution Logic

    private void CheckTimeTillExecution() {
        if (_viewModel.IsActive is false) return;
        if (_viewModel.TimeSpanTillExecution < ExecutionThreshold) {
            if (_viewModel.IsLoop && Enum.Equals(_viewModel.TimeMode, CommandTimerViewModel.TimeModeChoice.Time | CommandTimerViewModel.TimeModeChoice.Duration)) {
                _viewModel.StartTime = _viewModel.StartTime.Add(_viewModel.TargetTimeSpanTillExecution);

                if (_viewModel.StartTime < DateTime.Now) _viewModel.StartTime = DateTime.Now;

                _viewModel.IsActive = true;
            }
            else {
                /// Leave start time as the last start time.
                _viewModel.IsActive = false;
            }

            if (Core.Settings.ShouldExecuteOnTimer.Value) {
                Core.MessageRelay.OnMessagePosted(this, $"Executing [{_viewModel.Name}]", Core.MessageRelay.MessageCategory.User, Core.Settings.ShouldAutoNotificationsExpire.Value ? 0 : 100);
                ExecuteCommand();
            }
        }
    }

    public async void ExecuteCommand() {
        if (_executionGate) return;
        _executionGate = true;

        if (string.IsNullOrWhiteSpace(_viewModel.Command) || _viewModel.Command == CommandTimerViewModel.PLACEHOLDER_COMMAND) {
            Core.MessageRelay.OnMessagePosted(this, $"Executing [{_viewModel.Name}]: The command is empty...", Core.MessageRelay.MessageCategory.User);

            _executionGate = false;
            return;
        }

        SystemInteraction.Execute.Command(_viewModel.Command, _viewModel.Name, _viewModel.IsShowTerminal);

        /// Minimum time removes accidental double clicks and such.
        int minDelay = 1;
        /// If active timer should loop, then ensure time is not so fast that the user can not respond.
        if (_viewModel.IsLoop && _viewModel.IsActive && _viewModel.TargetTimeSpanTillExecution <= TimeSpan.FromSeconds(5)) {
            Core.MessageRelay.OnMessagePosted(this, $"Executing [{_viewModel.Name}]: A minimum execution timer is being enforced.", Core.MessageRelay.MessageCategory.User);
            minDelay = 5;
        }

        await Task.Delay(TimeSpan.FromSeconds(minDelay));

        _executionGate = false;
    }
}
