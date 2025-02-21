using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CommandTimer.Core.Utilities;
using CommandTimer.Core.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using static CommandTimer.Core.Settings;

namespace CommandTimer.Desktop.Views;

public partial class MessageViewer : UserControl {

    public MessageViewer() {
        InitializeComponent();
        DataContext = new MessageViewerViewModel();
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);

        Core.MessageRelay.MessagePosted += MessageRelay_MessagePosted;

        WillCall.Subscribe(Keys.WillCall_Key_OnOneSecond, Keys.WillCall_Interval_OnOneSecond, MessageTick_Elapsed);
    }

    protected override void OnUnloaded(RoutedEventArgs e) {
        base.OnUnloaded(e);

        Core.MessageRelay.MessagePosted -= MessageRelay_MessagePosted;

        WillCall.Unsubscribe(Keys.WillCall_Key_OnOneSecond, Keys.WillCall_Interval_OnOneSecond, MessageTick_Elapsed);
    }

    private void MessageTick_Elapsed(object? sender, EventArgs e) {
        if (DataContext is not MessageViewerViewModel viewModel) return;

        ObservableCollection<MessageControlViewModel> messages = new(viewModel.Messages);

        foreach (var message in messages) {
            if (!message.Transient) {
                continue;
            }

            if (message.lifetimeRemaining == 0) {
                RemoveMessage(message);
            }
            message.lifetimeRemaining--;
        }
    }

    private void MessageRelay_MessagePosted(object? sender, Core.MessageRelay.MessageEventArgs e) {
        Dispatcher.UIThread?.Invoke(() => {
            if (DataContext is not MessageViewerViewModel viewModel) return;

            var transient = !(e.Priority >= 100 ||
                e.Category is Core.MessageRelay.MessageCategory.Log or
                Core.MessageRelay.MessageCategory.Exception);

            /// Bug Fix: Was only displaying each whole line of text, leaving the rest cut off. NewLine fixes.
            var newItem = new MessageControlViewModel() { Message = e.Message + Environment.NewLine, Priority = e.Priority, Transient = transient };

            int insertIndex = viewModel.Messages
                .OrderByDescending(item => item.Priority)
                .TakeWhile(item => item.Priority >= newItem.Priority)
                .Count();

            viewModel.Messages.Insert(insertIndex, newItem);
        }, DispatcherPriority.Background);
    }

    private void Tapped_ItemTapped(object? sender, TappedEventArgs args) {
        if (sender is MessageControl control && control.DataContext is MessageControlViewModel itemViewModel) {
            RemoveMessage(itemViewModel);
        }
    }

    private async void RemoveMessage(MessageControlViewModel itemViewModel) {
        if (DataContext is not MessageViewerViewModel viewModel) return;
        try {
            await itemViewModel.OnRemove();
            viewModel.Messages.Remove(itemViewModel);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) {
            throw new Exception($"Method: {nameof(RemoveMessage)} has thrown the following.", ex);
        }
    }
}