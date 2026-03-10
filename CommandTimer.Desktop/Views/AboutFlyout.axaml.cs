using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;

namespace CommandTimer.Desktop.Views;

public partial class AboutFlyout : UserControl {

    private readonly string _aboutMessage01 = $"{Environment.NewLine}Fast access and organization of common commands or tasks.{Environment.NewLine}{Environment.NewLine}";
    private readonly string _aboutMessage02 = $"Anything you run in the terminal should be entered exactly the same here.{Environment.NewLine}{Environment.NewLine}";
    private readonly string _aboutMessage03 = $"This is a Visual Studio C# project made with the Avalonia Framework.{Environment.NewLine}{Environment.NewLine}";
    private readonly string _donationMessage = $"{Environment.NewLine}Please show your support:{Environment.NewLine}";
    private readonly List<(Control Control, EventHandler<PointerEventArgs> Handler)> _toolTipPointerEnteredHandlers = [];
    private EventHandler<PointerPressedEventArgs>? _toolTipPointerPressedHandler;

    public AboutFlyout() {
        InitializeComponent();

        /// About Flyout
        FlyoutAboutText.Text = $"{_aboutMessage01}{_aboutMessage02}{_aboutMessage03}";
        FlyoutDonationText.Text = $"{_donationMessage}";
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);

        if (Design.IsDesignMode is false) {
            SetupCustomToolTips();
        }
    }

    protected override void OnUnloaded(RoutedEventArgs e) {
        ClearCustomToolTipHandlers();

        base.OnUnloaded(e);
    }

    private void Tapped_DonationButton_Monero(object? sender, TappedEventArgs args)
        => App.CopyToClipboard(Settings.Keys.DonationKey_Monero);

    private void Tapped_DonationButton_Btc(object? sender, TappedEventArgs args)
        => App.CopyToClipboard(Settings.Keys.DonationKey_Btc);

    private void Tapped_PrivacyStatementButton(object? sender, TappedEventArgs args)
        => PrivacyStatement.Get().Show((TopLevel.GetTopLevel(this) as MainWindow)?.MainWindowLayout!);

    private void SetupCustomToolTips() {
        ClearCustomToolTipHandlers();

        var properties = ToolTipDefaults.GetDefaults();
        properties.Reference.Placement = PlacementMode.Bottom;
        var tooltip = ServiceProvider.Get<IShowToolTip>();

        _toolTipPointerPressedHandler = (o, a) => tooltip.Hide();
        RootPanel.AddHandler(PointerPressedEvent, _toolTipPointerPressedHandler, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, true);

        RegisterToolTip(PrivacyStatementButton, "Privacy Statement");
        RegisterToolTip(DonationButton_Monero, "Click to copy to clipboard");
        RegisterToolTip(DonationButton_MoneroQR, "Click to copy to clipboard");
        RegisterToolTip(DonationButton_Btc, "Click to copy to clipboard");
        RegisterToolTip(DonationButton_BtcQR, "Click to copy to clipboard");

        void RegisterToolTip(Control control, string message) {
            EventHandler<PointerEventArgs> handler = (s, args) => tooltip.OnPointerOver(control, message, properties);
            control.PointerEntered += handler;
            _toolTipPointerEnteredHandlers.Add((control, handler));
        }
    }

    private void ClearCustomToolTipHandlers() {
        if (_toolTipPointerPressedHandler is not null) {
            RootPanel.RemoveHandler(PointerPressedEvent, _toolTipPointerPressedHandler);
            _toolTipPointerPressedHandler = null;
        }

        foreach (var (control, handler) in _toolTipPointerEnteredHandlers) {
            control.PointerEntered -= handler;
        }

        _toolTipPointerEnteredHandlers.Clear();
    }

}

