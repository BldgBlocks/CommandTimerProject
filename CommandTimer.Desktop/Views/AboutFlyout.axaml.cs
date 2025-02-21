using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using CommandTimer.Core;
using CommandTimer.Core.Utilities;
using CommandTimer.Desktop.Utilities;
using System;

namespace CommandTimer.Desktop.Views;

public partial class AboutFlyout : UserControl {

    private readonly string _aboutMessage01 = $"{Environment.NewLine}Fast access and organization of common commands or tasks.{Environment.NewLine}{Environment.NewLine}";
    private readonly string _aboutMessage02 = $"Anything you run in the terminal should be entered exactly the same here.{Environment.NewLine}{Environment.NewLine}";
    private readonly string _aboutMessage03 = $"This is a Visual Studio C# project made with the Avalonia Framework.{Environment.NewLine}{Environment.NewLine}";
    private readonly string _donationMessage = $"{Environment.NewLine}Please show your support:{Environment.NewLine}";

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

    private void Tapped_DonationButton_Monero(object? sender, TappedEventArgs args)
        => App.CopyToClipboard(Core.Settings.Keys.DonationKey_Monero);

    private void Tapped_DonationButton_Btc(object? sender, TappedEventArgs args)
        => App.CopyToClipboard(Core.Settings.Keys.DonationKey_Btc);

    private void Tapped_PrivacyStatementButton(object? sender, TappedEventArgs args)
        => PrivacyStatement.Get().Show((MainWindow.Instance as MainWindow)?.MainWindowLayout!);

    private void SetupCustomToolTips() {
        var properties = ToolTipDefaults.GetDefaults();
        properties.Reference.Placement = PlacementMode.Bottom;
        var tooltip = ServiceProvider.Get<IShowToolTip>();

        RootPanel.AddHandler(PointerPressedEvent, (o, a) => tooltip.Hide(), RoutingStrategies.Tunnel | RoutingStrategies.Bubble, true);

        PrivacyStatementButton.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "Privacy Statement", properties);

        DonationButton_Monero.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "Click to copy to clipboard", properties);

        DonationButton_MoneroQR.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "Click to copy to clipboard", properties);

        DonationButton_Btc.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "Click to copy to clipboard", properties);

        DonationButton_BtcQR.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "Click to copy to clipboard", properties);
    }

}