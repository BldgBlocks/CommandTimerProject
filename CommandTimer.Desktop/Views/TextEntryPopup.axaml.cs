using Avalonia.Interactivity;
using CommandTimer.Core.Utilities;

namespace CommandTimer.Desktop.Views;

public partial class TextEntryPopup : UserControl {

    public TextEntryPopup() {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        Entry.BackgroundBorder!.BorderBrush = ServiceProvider.Get<IColorProvider>().ApplicationBrush_Accent.Value.AsBrush();
        Entry.BackgroundBorder!.BorderThickness = new Avalonia.Thickness(1);

        base.OnLoaded(e);
    }

}


