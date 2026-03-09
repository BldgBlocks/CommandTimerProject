namespace CommandTimer.Desktop.Views;

public partial class MainView : UserControl {

    public MainView() {
        InitializeComponent();
        AssignViews();
    }

    private void AssignViews() {
        MainViewMain.Content = new ListView();
        MainViewBottom.Content = new ActivityDisplay();
    }

}


