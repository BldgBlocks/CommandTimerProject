using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Threading.Tasks;

namespace CommandTimer.Desktop.Views;
public partial class CustomTextBox : UserControl {

    private string _lastEntry = string.Empty;
    private string _initializedEntry = string.Empty;
    private IBrush? _initializedBrush;
    public TextBox? TextBox;
    public Border? BackgroundBorder;
    private bool _accepted;
    private Window? _parentWindow => MainWindow.Instance;
    private bool HasFocus => TextBox is not null && TextBox.IsFocused;
    private bool PointerOver => TextBox is not null && TextBox.IsPointerOver;
    public object? Parameter { get; set; }


    /// <summary>
    /// Forward the TextChanged from the internal control.
    /// </summary>
    public event EventHandler<TextChangedEventArgs>? TextChanged;
    public event Action? OnAccept;
    public event Action? OnCancel;

    public CustomTextBox() {
        InitializeComponent();
    }

    //...

    ///
    /// This custom control exposes the internal controls of a TextBox. However, this means that any properties that you need to use from the outside
    /// have to be exposed here. So add a StyledProperty for binding to the Avalonia system. Create a public property that can be bound to from axaml.
    /// Then in the axaml for this control add the property in the correct place and bind it to the template as you see others done.
    /// 

    /// <summary>
    /// Register the public property for changes.
    /// </summary>
    public static new readonly StyledProperty<IBrush> ForegroundProperty =
        AvaloniaProperty.Register<CustomTextBox, IBrush>(nameof(Foreground), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);
    public new IBrush Foreground {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    /// <summary>
    /// Register the public property for changes.
    /// </summary>
    public static new readonly StyledProperty<IBrush> BackgroundProperty =
        AvaloniaProperty.Register<CustomTextBox, IBrush>(nameof(Background), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);
    public new IBrush Background {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    /// <summary>
    /// Register the public property for changes.
    /// </summary>
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<CustomTextBox, string?>(nameof(Text), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);
    public string Text {
        get => GetValue(TextProperty) ?? string.Empty;
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Register the public property for changes.
    /// </summary>
    public static new readonly StyledProperty<Thickness> BorderThicknessProperty =
        AvaloniaProperty.Register<CustomTextBox, Thickness>(nameof(BorderThickness), defaultValue: new Thickness(0), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);
    public new Thickness BorderThickness {
        get => GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }

    /// </summary>
    public static new readonly StyledProperty<IBrush> BorderBrushProperty =
        AvaloniaProperty.Register<CustomTextBox, IBrush>(nameof(BorderBrush), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);
    public new IBrush BorderBrush {
        get => GetValue(BorderBrushProperty);
        set => SetValue(BorderBrushProperty, value);
    }

    /// </summary>
    public static readonly StyledProperty<IBrush> PointerOverColorProperty =
        AvaloniaProperty.Register<CustomTextBox, IBrush>(nameof(PointerOverColor), Brushes.Gray, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);
    public IBrush PointerOverColor {
        get => GetValue(PointerOverColorProperty);
        set => SetValue(PointerOverColorProperty, value);
    }

    /// <summary>
    /// Register the public property for changes.
    /// </summary>
    public static readonly StyledProperty<int> MaxLinesProperty =
        AvaloniaProperty.Register<CustomTextBox, int>(nameof(MaxLines), defaultValue: 1, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);
    public int MaxLines {
        get => GetValue(MaxLinesProperty);
        set => SetValue(MaxLinesProperty, value);
    }

    /// <summary>
    /// Register the public property for changes.
    /// </summary>
    public static new readonly StyledProperty<FontStyle> FontStyleProperty =
        AvaloniaProperty.Register<CustomTextBox, FontStyle>(nameof(FontStyle), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);
    public new FontStyle FontStyle {
        get => GetValue(FontStyleProperty);
        set => SetValue(FontStyleProperty, value);
    }

    /// <summary>
    /// Register the public property for changes.
    /// </summary>
    public static new readonly StyledProperty<double> FontSizeProperty =
        AvaloniaProperty.Register<CustomTextBox, double>(nameof(FontSize), defaultValue: 12, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);
    public new double FontSize {
        get => GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    /// <summary>
    /// Register the public property for changes.
    /// </summary>
    public static new readonly StyledProperty<FontWeight> FontWeightProperty =
        AvaloniaProperty.Register<CustomTextBox, FontWeight>(nameof(FontWeight), defaultValue: FontWeight.Medium, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);
    public new FontWeight FontWeight {
        get => GetValue(FontWeightProperty);
        set => SetValue(FontWeightProperty, value);
    }

    /// <summary>
    /// Register the public property for changes.
    /// </summary>
    public static readonly StyledProperty<TextWrapping> TextWrappingProperty =
        AvaloniaProperty.Register<CustomTextBox, TextWrapping>(nameof(TextWrapping), defaultValue: TextWrapping.NoWrap, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);
    public TextWrapping TextWrapping {
        get => GetValue(TextWrappingProperty);
        set => SetValue(TextWrappingProperty, value);
    }

    /// <summary>
    /// Register the public property for changes.
    /// </summary>
    public static readonly StyledProperty<TextAlignment> TextAlignmentProperty =
        AvaloniaProperty.Register<CustomTextBox, TextAlignment>(nameof(TextAlignment), defaultValue: TextAlignment.Left, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);
    public TextAlignment TextAlignment {
        get => GetValue(TextAlignmentProperty);
        set => SetValue(TextAlignmentProperty, value);
    }

    /// <summary>
    /// Register the public property for changes.
    /// </summary>
    public static readonly StyledProperty<int> MaxLengthProperty =
        AvaloniaProperty.Register<CustomTextBox, int>(nameof(MaxLength), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);
    public int MaxLength {
        get => GetValue(MaxLengthProperty);
        set => SetValue(MaxLengthProperty, value);
    }

    /// <summary>
    /// Register the public property for changes.
    /// </summary>
    public static new readonly StyledProperty<int> MinWidthProperty =
        AvaloniaProperty.Register<CustomTextBox, int>(nameof(MinWidth), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);
    public new int MinWidth {
        get => GetValue(MinWidthProperty);
        set => SetValue(MinWidthProperty, value);
    }

    /// <summary>
    /// Register the public property for changes.
    /// </summary>
    public static readonly StyledProperty<bool> AcceptWithDismissProperty =
        AvaloniaProperty.Register<CustomTextBox, bool>(nameof(AcceptWithDismiss), true, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);
    public bool AcceptWithDismiss {
        get => GetValue(AcceptWithDismissProperty);
        set => SetValue(AcceptWithDismissProperty, value);
    }

    /// <summary>
    /// Register the public property for changes.
    /// </summary>
    public static readonly StyledProperty<bool> AcceptsReturnProperty =
        AvaloniaProperty.Register<CustomTextBox, bool>(nameof(AcceptsReturn), true, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);
    public bool AcceptsReturn {
        get => GetValue(AcceptsReturnProperty);
        set => SetValue(AcceptsReturnProperty, value);
    }

    //...

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        _accepted = false;
        _lastEntry = TextBox?.Text ?? string.Empty;

        TextBox?.AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Bubble, true);
    }

    protected override void OnUnloaded(RoutedEventArgs e) {
        base.OnUnloaded(e);

        /// Global Setting
        TextBox?.RemoveHandler(KeyDownEvent, OnKeyDown);
    }

    //...

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);

        TextBox = e.NameScope.Find<TextBox>("PART_TextBox");
        BackgroundBorder = e.NameScope.Find<Border>("PART_BorderElement");
        _lastEntry = TextBox?.Text ?? string.Empty;
        _initializedEntry = TextBox?.Text ?? string.Empty;
    }

    public void OnKeyDown(object? sender, KeyEventArgs e) {
        if (TextBox is null) return;

        switch (e.Key) {
            case Key.Enter:
                if (sender != TextBox) return;
                AcceptEntry();
                break;
            case Key.Escape:
                CancelEntry();
                break;
            case Key.Right:
                if (TextBox.SelectedText.Length == 0) return;
                TextBox.Focus();
                TextBox.SelectionStart = -1;
                TextBox.SelectionEnd = -1;
                TextBox.CaretIndex = Math.Max(TextBox.SelectionStart, TextBox.SelectionEnd); ;
                break;
            case Key.Left:
                if (TextBox.SelectedText.Length == 0) return;
                TextBox.Focus();
                TextBox.SelectionStart = -1;
                TextBox.SelectionEnd = -1;
                TextBox.CaretIndex = Math.Min(TextBox.SelectionStart, TextBox.SelectionEnd);
                break;
            default:
                return;
        }
        e.Handled = true;
    }

    private void AcceptEntry() {
        if (TextBox == null) return;

        Text = TextBox.Text?.Trim() ?? string.Empty;
        _lastEntry = Text;

        if (string.IsNullOrWhiteSpace(Text)) {
            Text = _initializedEntry;
            _lastEntry = _initializedEntry;
        }

        TextBox.CaretIndex = 0;
        TextBox.ScrollToLine(0);
        _parentWindow?.FocusManager?.ClearFocus();

        if (Text != _initializedEntry) {
            _accepted = true;
            OnAccept?.Invoke();
        }
        else {
            CancelEntry();
        }

    }

    private void CancelEntry() {
        Text = _lastEntry;
        if (TextBox is not null) {
            TextBox.Text = _lastEntry; 
        }
        OnCancel?.Invoke();
    }

    /// Skip the initial value. Avoid callbacks on the default value at start.
    int _textChangedInitialSkip = 1;
    private void OnTextChanged(object? sender, TextChangedEventArgs args) {
        if (_textChangedInitialSkip is not 0) {
            _textChangedInitialSkip--;
            return;
        }

        TextChanged?.Invoke(this, args);
    }

    private void OnWindowPointerPressed(object? sender, PointerPressedEventArgs e) {
        if (HasFocus is false) return;
        if (IsPointerInsideControl(e)) return;

        if (AcceptWithDismiss) AcceptEntry(); else CancelEntry();

        _parentWindow?.FocusManager?.ClearFocus();
    }

    private bool IsPointerInsideControl(PointerPressedEventArgs e) {
        var point = e.GetPosition(this);
        return point.X >= 0 && point.X <= Bounds.Width &&
               point.Y >= 0 && point.Y <= Bounds.Height;
    }

    private void OnPointerEntered(object? sender, PointerEventArgs args) {
        if (HasFocus is false) {
            _initializedBrush = Background;
            Background = PointerOverColor;
        }
    }

    private void OnPointerExited(object? sender, PointerEventArgs args) {
        if (HasFocus is false) {
            Background = _initializedBrush ?? Brushes.Transparent;
        }
    }

    /// <summary>
    /// Handles the focus for the control, including PointerOver color, events, text selection and focus.
    /// </summary>
    /// <remarks>
    /// - Monitors window for clicks off the control.
    /// </remarks>
    private async void OnGotFocus(object? sender, GotFocusEventArgs args) {
        _parentWindow?.AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        _parentWindow?.AddHandler(PointerPressedEvent, OnWindowPointerPressed, RoutingStrategies.Tunnel);

        var color = (PointerOverColor as SolidColorBrush)?.Color
            ?? (PointerOverColor as IImmutableSolidColorBrush)?.Color
            ?? Colors.White;
        Background = new SolidColorBrush(new Color(10, color.R, color.G, color.B));

        try {
            /// Wait till click is finished, then select all for the user.
            await Dispatcher.UIThread.Invoke(async () => {
                await Task.Delay(50);
                TextBox?.SelectAll();
                /// For some reason focus is sometimes stolen to keyboard navigation 
                /// when pressing left arrow calling focus seems to handle this.
                TextBox?.Focus();
            }, DispatcherPriority.Background);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) {
            throw new Exception($"Method: {nameof(OnGotFocus)} has thrown the following.", ex);
        }
    }

    /// <summary>
    /// Handles the loss of focus for the control, including PointerOver color and cleanup of events and state.
    /// </summary>
    /// <remarks>
    /// - Cancels entry if not already accepted.
    /// </remarks>
    private void OnLostFocus(object? sender, RoutedEventArgs args) {
        _parentWindow?.RemoveHandler(KeyDownEvent, OnKeyDown);
        _parentWindow?.RemoveHandler(PointerPressedEvent, OnWindowPointerPressed);

        Background = PointerOver ? PointerOverColor : _initializedBrush ?? Brushes.Transparent;

        /// If you click outside the control, it's detected OnWindowPointerPressed. But if you click another button directly
        /// then it appears there is a new value in this text box, but the backing still has the old, so if not accepted already, cancel.
        if (!_accepted) CancelEntry();
    }
}