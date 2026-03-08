using Avalonia.Controls;
using Avalonia.Interactivity;

namespace CommandTimer.Desktop.Views;

public partial class TextEntryPopup : UserControl {

    public TextEntryPopup() {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        Entry.BackgroundBorder!.BorderBrush = ServiceProvider.Get<IColorProvider>().ApplicationBrush_Accent.Value;
        Entry.BackgroundBorder!.BorderThickness = new Avalonia.Thickness(1);

        base.OnLoaded(e);
    }

}


