using Avalonia.Controls;
using Avalonia.Interactivity;
using CommandTimer.Core.Utilities;
using CommandTimer.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CommandTimer.Desktop.Views;

public partial class ActivityDisplay : UserControl {

    public ActivityDisplay() {
        InitializeComponent();

        DataContext = new ActivityDisplayViewModel();
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);

        WillCall.Subscribe(Core.Settings.Keys.WillCall_Key_OnOneSecond, Core.Settings.Keys.WillCall_Interval_OnOneSecond, EventHandler_UpdateUI);
    }

    protected override void OnUnloaded(RoutedEventArgs e) {
        base.OnUnloaded(e);

        WillCall.Unsubscribe(Core.Settings.Keys.WillCall_Key_OnOneSecond, Core.Settings.Keys.WillCall_Interval_OnOneSecond, EventHandler_UpdateUI);
    }

    private void EventHandler_UpdateUI(object? sender, EventArgs e) {
        if (DataContext is not ActivityDisplayViewModel viewModel) return;

        int commandsCount = 0;
        var libraries = LibraryManager.Libraries;
        libraries.ForEach(l => commandsCount += l.Names.Count());

        List<CommandTimerViewModel> activeTimers = [];
        foreach (var library in libraries) {
            activeTimers.AddRange(library.CommandTimers.Where(t => t.IsActive));
        }
        var nextTimer = activeTimers.MinBy(t => t.CountdownTillExecution);

        LibrariesValue.Text = $"{libraries.Count()}";

        CurrentCommandsValue.Text = $"({LibraryManager.CurrentLibrary.CommandTimers.Count()} current)";
        TotalCommandsValue.Text = $"({commandsCount} total)";

        ActiveCommandsValue.Text = $"({activeTimers.Count} active)";

        CommandNameValue.Text = $"'{nextTimer?.Name ?? "_"}'";

        CompletesInValue.Text = nextTimer != null ? nextTimer.CountdownTillExecution : "_";

        string completesAt = nextTimer != null ? (DateTime.Now + nextTimer.TimeSpanTillExecution).ToString() : "_";
        CompletesAtValue.Text = $"{completesAt}";

        CurrentTimeValue.Text = $"({DateTime.Now})";
    }
}