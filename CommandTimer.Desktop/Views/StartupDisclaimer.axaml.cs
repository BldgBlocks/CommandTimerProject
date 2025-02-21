using Avalonia.Controls;
using CommandTimer.Core;
using CommandTimer.Core.Utilities;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace CommandTimer.Desktop.Views;

public partial class StartupDisclaimer : UserControl {

    /// <summary>
    /// This class ideally is only shown the first time the application starts, 
    /// leading to no access to this class to trigger a static constructor.
    /// However, serialization must be subscribed to so that its values can be written.
    /// The [ModuleInitializer] attribute is used to guarantee initialization at startup.
    /// </summary>
    [ModuleInitializer]
    public static void Initialize() {
        Core.ActionRelay.ActionPosted += ActionRelay_Serialization;
    }

    private static StartupDisclaimer? _instance;
    private static bool _termsAccepted;
    private ConfirmationPresenter _confirmationPresenter;

    //...

    public StartupDisclaimer() {
        InitializeComponent();
        _confirmationPresenter = ConfirmationPresenter.Create(this, PART_ButtonProceed, PART_ButtonQuit);
    }

    private static void Serialize() {
        ServiceProvider.Get<ISerializer>().Serialize<bool>("Application:HaveShownDisclaimer", _instance == null || _termsAccepted, Core.Settings.DEFAULT_DATA_FILE);
    }

    public static bool HaveShownTerms() {
        return ServiceProvider.Get<ISerializer>().Deserialize<bool>("Application:HaveShownDisclaimer", Core.Settings.DEFAULT_DATA_FILE);
    }

    //... Public Methods

    public static StartupDisclaimer Create() => _instance = new();

    public async Task<bool?> Show(Panel panel) {
        var result = await _confirmationPresenter.Show(panel);
        if (result.Value) Proceed();
        else Quit();
        return result.Value;
    }

    public StartupDisclaimer Hide() {
        _confirmationPresenter.Hide();
        return this;
    }

    //... Event Handlers

    private static void ActionRelay_Serialization(object? sender, Core.ActionRelay.ActionEventArgs args)
        => Serialize();

    //... Actions

    private void Proceed() {
        _termsAccepted = true;
        Serialize();
        Hide();
    }

    private static void Quit() {
        _termsAccepted = false;
        App.Shutdown();
    }
}