using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace CommandTimer.Desktop.Views;

public partial class StartupDisclaimer : UserControl {

    /// <summary>
    /// This class ideally is only shown the first time the application starts,
    /// leading to no access to this class to trigger a static constructor on subsequent launches.
    /// However, its serialized value (HaveShownDisclaimer) lives in global_data alongside everything else.
    /// When serialization is triggered, every serializable thing must write its current state — 
    /// if this class was never accessed this session, no static constructor fires, no subscription exists,
    /// and the value gets silently dropped from the next write.
    /// 
    /// [ModuleInitializer] is method-scoped (not class-scoped like static constructors) and runs 
    /// unconditionally when the assembly loads, regardless of whether the type is ever accessed.
    /// This guarantees the ActionRelay serialization subscription exists every session,
    /// protecting data persistence even when the disclaimer UI is irrelevant.
    /// </summary>
    [ModuleInitializer]
    public static void Initialize() {
        ActionRelay.ActionPosted += ActionRelay_Serialization;
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
        ServiceProvider.Get<ISerializer>().Serialize<bool>("Application:HaveShownDisclaimer", _instance == null || _termsAccepted, Settings.DEFAULT_DATA_FILE);
    }

    public static bool HaveShownTerms() {
        return ServiceProvider.Get<ISerializer>().Deserialize<bool>("Application:HaveShownDisclaimer", Settings.DEFAULT_DATA_FILE);
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

    private static void ActionRelay_Serialization(object? sender, ActionRelay.ActionEventArgs args)
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

