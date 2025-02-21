using Avalonia.Media;

namespace CommandTimer.Core;

public static class Settings {

    //...
    public static readonly string DEFAULT_PATH = """C:\"""; // This will change based on distribution
    public static string APPLICATION_PATH = """F:\GitRepos\CommandTimer\build\net8.0\CommandTimer.Internal.Initialization.exe""";  // This will change.

    public static string DEFAULT_DATA_FILE = "global_data";

    public static class Keys {

        public static readonly string ListView = "ListView:Settings";

        public static readonly string ActivityView = "Activity:Settings";

        public static readonly string GlobalSettings = "Global:Settings";

        public static readonly string LibraryPrefix = "Library:";

        public static readonly string LibraryManagerPrefix = "LibraryManager:";

        public static readonly string ApplicationDisclaimerAccepted = "Application:HaveShownDisclaimer";

        public static readonly string CommandTimerPrefix = "CommandTimer:";

        public static readonly string ActionRelay_Serialization = "Serialize";

        public static readonly string WillCall_Key_OnOneSecond = "WillCall_Key_OnOneSecond";

        public static readonly int WillCall_Interval_OnOneSecond = 1000;

        public static readonly string WillCall_Key_On100ms = "WillCall_Key_On100ms";

        public static readonly int WillCall_Interval_On100ms = 100;

        public static readonly string DonationKey_Monero = "87vRCaVUdbs63TeiWdsVwBKBiQo8RiGMUPy2K65ya7QLBWVF8nSmQVmdvx9AiMB8eU1LnPcUk2K4Q1gRmULRAFMsFxS7rgE";

        public static readonly string DonationKey_Btc = "bc1qqhqrlm5hkd994ehmwju8ju2swmu38wczcrhvrf";

    }

    public enum AnimationChoice { None, All, }

    //...
    /// <summary>
    /// Global Setting
    /// </summary>
    /// <remarks>Choose if controls should animate.</remarks>
    public static readonly ObservableProperty<AnimationChoice> ShouldAnimate = new(AnimationChoice.All);
    /// <summary>
    /// Global Setting
    /// </summary>
    /// <remarks>Choose if commands should execute on timers (manual only).</remarks>
    public static readonly ObservableProperty<bool> ShouldExecuteOnTimer = new(true);
    /// <summary>
    /// Global Setting
    /// </summary>
    /// <remarks>Choose if notifications from timer expiration should expire. (So you can see what ran while away) (manual only).</remarks>
    public static readonly ObservableProperty<bool> ShouldAutoNotificationsExpire = new(true);
    /// <summary>
    /// Global Setting
    /// </summary>
    /// <remarks>Choose if commands should be logged.</remarks>
    public static readonly ObservableProperty<bool> ShouldLog = new(true);
    /// <summary>
    /// Global Setting
    /// </summary>
    /// <remarks>Choose if 'Execute' button should prompt for confirmation. (Prevent accidental execution)</remarks>
    public static readonly ObservableProperty<bool> ShouldPromptByDefault = new(true);
    /// <summary>
    /// Global Setting
    /// </summary>
    /// <remarks>Choose if 'Execute' button should prompt for password confirmation. (Prevent unauthorized execution)</remarks>
    public static readonly ObservableProperty<bool> ShouldUsePasswordConfirmation = new(false);
    /// <summary>
    /// Global Setting
    /// </summary>
    /// <remarks>Choose if color should expand to timer item accents.</remarks>
    public static readonly ObservableProperty<bool> ShouldExpandColorBar = new(true);
    /// <summary>
    /// Global Setting
    /// </summary>
    /// <remarks>Choose if color should expand to timer item accents.</remarks>
    public static readonly ObservableProperty<bool> ShouldStripeList = new(true);
    /// <summary>
    /// Global Setting
    /// </summary>
    /// <remarks>Choose if AutoStart should trigger upon load.</remarks>
    public static readonly ObservableProperty<bool> ShouldAutoStart = new(true);
    /// <summary>
    /// Global Setting
    /// </summary>
    /// <remarks>Choose if the database should be cleaned at each startup.</remarks>
    public static readonly ObservableProperty<bool> ShouldCleanDatabase = new(true);
    /// <summary>
    /// Global Setting
    /// </summary>
    /// <remarks>Choose how many lines should expand for description.</remarks>
    public static readonly ObservableProperty<int> MaxLines = new(1);
    /// <summary>
    /// Global Setting
    /// </summary>
    /// <remarks>Data backups to keep after database cleaning.</remarks>
    public static readonly ObservableProperty<int> BackupVersionsToKeep = new(2);
    /// <summary>
    /// Global Setting
    /// </summary>
    /// <remarks>Choose the accent color of the application.</remarks>
    public static readonly ObservableProperty<SolidColorBrush> AccentColorSelection = new(new SolidColorBrush(Core.Colors.ApplicationColor_Accent));
}


