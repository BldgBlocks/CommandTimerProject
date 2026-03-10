using System.Text.Json.Serialization;

namespace CommandTimer.Core.ViewModels;

/// <summary>
/// Pure serializable data for the settings flyout.
/// No behavior, no subscriptions, no UI bindings.
/// </summary>
public class SettingsFlyoutData {

    [JsonPropertyName("ShouldAnimate")]
    public Settings.AnimationChoice ShouldAnimate { get; set; } = Settings.AnimationChoice.All;

    [JsonPropertyName("ShouldExecuteOnTimer")]
    public bool ShouldExecuteOnTimer { get; set; } = true;

    [JsonPropertyName("ShouldAutoNotificationsExpire")]
    public bool ShouldAutoNotificationsExpire { get; set; } = true;

    [JsonPropertyName("ShouldLog")]
    public bool ShouldLog { get; set; } = true;

    [JsonPropertyName("ShouldPromptByDefault")]
    public bool ShouldPromptByDefault { get; set; } = true;

    [JsonPropertyName("ShouldUsePasswordConfirmation")]
    public bool ShouldUsePasswordConfirmation { get; set; }

    [JsonPropertyName("ShouldAutoStart")]
    public bool ShouldAutoStart { get; set; } = true;

    [JsonPropertyName("ShouldExpandColorBar")]
    public bool ShouldExpandColorBar { get; set; } = true;

    [JsonPropertyName("ShouldStripeList")]
    public bool ShouldStripeList { get; set; } = true;

    [JsonPropertyName("ShouldCleanDatabase")]
    public bool ShouldCleanDatabase { get; set; } = true;

    [JsonPropertyName("MaxLines")]
    public int MaxLines { get; set; } = 1;

    [JsonPropertyName("BackupVersions")]
    public int BackupVersionsToKeep { get; set; } = 2;

    [JsonPropertyName("ThemeSelection")]
    public string ThemeSelection { get; set; } = "Dark";

    [JsonPropertyName("AccentColorSelection")]
    public AppColor AccentColorSelection { get; set; } = ServiceProvider.Get<IColorProvider>().ApplicationBrush_Accent.Value;
}
