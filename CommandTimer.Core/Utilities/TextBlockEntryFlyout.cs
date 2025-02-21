using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using System;

namespace CommandTimer.Core.Utilities;

public class TextBlockEntryFlyout(FlyoutBase flyout, TextBlock target, TextBox entry) {
    public Action? OnAccept;
    public Action? OnCancel;
    private string _last = string.Empty;

    public void Connect() {
        target.Tapped += Tapped;
        target.PointerEntered += Entered;
        target.PointerExited += Exited;
        entry.KeyDown += KeyDown;
        flyout.Opened += FlyoutOpened;
        flyout.Closed += FlyoutClosed;
    }

    public void Disconnect() {
        target.PointerEntered -= Entered;
        target.PointerExited -= Exited;
        entry.KeyDown -= KeyDown;
        flyout.Opened -= FlyoutOpened;
        flyout.Closed -= FlyoutClosed;
    }

    private void Tapped(object? sender, TappedEventArgs args) {
        FlyoutBase.ShowAttachedFlyout(target);
        entry.SelectAll();
    }

    private void KeyDown(object? sender, KeyEventArgs args) {
        if (args.Key is Key.Enter) {
            flyout?.Hide();
            OnAccept?.Invoke();
            args.Handled = true;
        }
        else if (args.Key is Key.Escape) {
            entry.Text = _last;
            flyout?.Hide();
            OnCancel?.Invoke();
            args.Handled = true;
        }
    }


    private void FlyoutClosed(object? sender, EventArgs e) => _last = string.Empty;
    private void FlyoutOpened(object? sender, EventArgs e) => _last = target.Text ?? string.Empty;
    public void Entered(object? sender, PointerEventArgs args) => target.Cursor = new Cursor(StandardCursorType.Hand);
    public void Exited(object? sender, PointerEventArgs args) => target.Cursor = new Cursor(StandardCursorType.Arrow);

}
