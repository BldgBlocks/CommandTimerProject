using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CommandTimer.Core;
using System;

namespace CommandTimer.Desktop.Utilities;

public class CustomTipTimer : IShowToolTip {

    private const int DISTANCE_FROM_TRIGGER = 10;
    private readonly DispatcherTimer _tipTimer;
    private PointerEventArgs? _movedEventArgs;
    private IShowToolTipProperties _lastProperties = new ToolTipProperties();
    private Control? _lastControl;
    private string _lastToolTip = string.Empty;
    private Point _currentPosition;
    private Point _lastTriggerPosition = new(0, 0);
    private bool _isEnabled = true;

    private readonly Window _window;
    private readonly Popup _show;
    private readonly Action<string, IShowToolTipProperties> _onShow;

    /// <summary>
    /// Manages delaying, showing, hiding, and properties configuration of tooltips without flickering.
    /// </summary>
    /// <param name="defaults">A set of fallback properties if not provided by caller.</param>
    /// <param name="onShowing">Called before opening popup for configuration of controls based on properties.</param>
    public CustomTipTimer(Window window, Popup toShow, Action<string, IShowToolTipProperties> onShow, int delay = 1000) {
        _window = window;
        _show = toShow;
        _onShow = onShow;
        _window.PointerMoved += EventHandler_PointerMoved;
        _window.AddHandler(InputElement.PointerPressedEvent, EventHandler_PointerClicked, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, true);

        _tipTimer = new();
        _tipTimer.Interval = TimeSpan.FromMilliseconds(delay);
        _tipTimer.Tick += EventHandler_TimerTick;
        _tipTimer.Tick += (s, e) => _tipTimer.Stop();
    }

    private void EventHandler_PointerClicked(object? sender, PointerPressedEventArgs e)
        => _tipTimer.Stop();

    private void EventHandler_PointerMoved(object? sender, PointerEventArgs args) {
        _movedEventArgs = args;
        if (_lastControl is null) return;
        _currentPosition = _movedEventArgs?.GetPosition(_lastControl) ?? new Point(0, 0);
        if (_show.IsOpen && IsPointerOverControl(_lastControl) is false) {
            Hide();
        }
    }

    private void EventHandler_TimerTick(object? sender, EventArgs args) {
        if (_lastControl is null) return;

        if (IsPointerOverControl(_lastControl)) {
            Show();
        }
    }

    private bool IsPointerOverControl(Control control) {
        return control.IsPointerOver || Math.Abs(_lastTriggerPosition.X - _currentPosition.X) <= DISTANCE_FROM_TRIGGER &&
                                        Math.Abs(_lastTriggerPosition.Y - _currentPosition.Y) <= DISTANCE_FROM_TRIGGER;
    }


    //...

    public void OnPointerOver(Control control, string tooltip, IShowToolTipProperties properties) {
        if (_show.IsOpen && control == _lastControl) return;
        if (_show.IsOpen) Hide();
        if (_isEnabled is false) return;

        _lastControl = control;
        _lastToolTip = tooltip;
        _lastTriggerPosition = _movedEventArgs?.GetPosition(control) ?? new Point(0, 0);
        _lastProperties = properties;

        if (_tipTimer.IsEnabled) _tipTimer.Stop();
        _tipTimer.Start();
    }

    public void Show() {
        _show.OverlayInputPassThroughElement = _lastControl;
        _show.PlacementTarget = _lastControl;
        _onShow(_lastToolTip, _lastProperties);
        _show.Open();
    }

    public void Hide() {
        _tipTimer.Stop();
        _show.Close();
    }

    public bool IsOpen() => _isEnabled;
    public void Enable() => _isEnabled = true;

    public void Disable() {
        Hide();
        _isEnabled = false;
    }
}
