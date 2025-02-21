using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;
using System;

namespace CommandTimer.Core.Utilities;

public class NumericUpDownBinding(FlyoutBase flyout, Button target, NumericUpDown entry) {

    public event Action? Accept;
    public event Action? Cancel;
    private decimal _last;
    private Window? _parentWindow;

    public void Connect() {
        target.Tapped += Tapped;
        //target.PointerEntered += Entered;
        //target.PointerExited += Exited;
        /// KeyDown will bubble to other controls including the increase/decrease buttons when trying to accept, this intercepts the event.
        entry.AddHandler(NumericUpDown.KeyDownEvent, KeyDown, Avalonia.Interactivity.RoutingStrategies.Tunnel);
        entry.ValueChanged += ValueChanged;
        flyout.Opened += FlyoutOpened;
        flyout.Closed += FlyoutClosed;
    }

    public void Disconnect() {
        //target.PointerEntered -= Entered;
        //target.PointerExited -= Exited;
        entry.RemoveHandler(NumericUpDown.KeyDownEvent, KeyDown);
        entry.ValueChanged -= ValueChanged;
        flyout.Opened -= FlyoutOpened;
        flyout.Closed -= FlyoutClosed;
    }

    private void Tapped(object? sender, TappedEventArgs args) {
        FlyoutBase.ShowAttachedFlyout(target);
    }

    private void KeyDown(object? sender, KeyEventArgs args) {
        if (args.Key is Key.Enter) {
            args.Handled = true;
            OnAccept();
        }
        else if (args.Key is Key.Escape) {
            args.Handled = true;
            OnCancel();
        }
    }

    public void OnAccept() {
        Accept?.Invoke();
        flyout?.Hide();
    }

    public void OnCancel() {
        entry.Value = _last;
        Cancel?.Invoke();
        flyout?.Hide();
    }

    /// <summary>
    /// For some reason, even with string formats specified, setting the Content string from a Value decimal was adding a decimal.
    /// Code-behind taking control...
    /// </summary>
    private void ValueChanged(object? sender, NumericUpDownValueChangedEventArgs args) {
        if (args.NewValue is null) return;
    }

    private void OnWindowPointerPressed(object? sender, PointerPressedEventArgs args) {
        /// The Window will only receive the event if you click outside of the flyout.
        /// The flyout takes all pressed events. Bounds checking doesn't work with flyouts either.
        args.Handled = true;
        OnAccept();
    }

    private void FlyoutClosed(object? sender, EventArgs args) {
        _last = 0;

        if (_parentWindow is null) return;
        _parentWindow.PointerPressed -= OnWindowPointerPressed;
    }

    private void FlyoutOpened(object? sender, EventArgs args) {
        if (decimal.TryParse(target?.Content?.ToString(), out decimal result) is false) {
            result = 0;
        }
        _last = result;
        entry.Value = result;

        _parentWindow ??= target?.GetVisualRoot() as Window;

        if (_parentWindow is null) return;
        _parentWindow.PointerPressed += OnWindowPointerPressed;
    }

    public void Entered(object? sender, PointerEventArgs args) => target.Cursor = new Cursor(StandardCursorType.Hand);
    public void Exited(object? sender, PointerEventArgs args) => target.Cursor = new Cursor(StandardCursorType.Arrow);

}
