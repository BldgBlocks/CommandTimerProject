using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using CommandTimer.Core.ViewModels.MenuItems;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CommandTimer.Core.ViewModels;

public class SettingsFlyoutViewModel : ViewModelBase {

    public SettingsFlyoutViewModel() {
        /// Global Action
        ActionRelay.ActionPosted += (o, a) => {
            if (a.ActionKey == Settings.Keys.ActionRelay_Serialization) {
                Serialize();
            }
        };

        /// Theme Selection Menu
        var themeSelectionMenu = new ThemeSelectionsMenu(Command_ThemeSelection);
        ThemeSelections = new ObservableCollection<MenuItemViewModel>(themeSelectionMenu.Items);
        _ThemeSelection = ThemeSelection.Header;
    }

    public override void Serialize() => ServiceProvider.Get<ISerializer>().Serialize(Settings.Keys.GlobalSettings, this, Settings.DEFAULT_DATA_FILE);

    //...

    [JsonInclude]
    [JsonPropertyName("ShouldAnimate")]
    private Settings.AnimationChoice _ShouldAnimate = Settings.AnimationChoice.All;

    /// <summary>
    /// Global Setting
    /// </summary>
    /// <remarks>Choose if controls should animate.</remarks>
    [JsonIgnore]
    public bool ShouldAnimate {
        get => _ShouldAnimate is Settings.AnimationChoice.All;
        set {
            var _value = value ? Settings.AnimationChoice.All : Settings.AnimationChoice.None;
            if (SetProperty(ref _ShouldAnimate, _value, Save.Yes, Notify.Yes)) {
                Settings.ShouldAnimate.Value = _value;
            }
        }
    }

    //...

    [JsonInclude]
    [JsonPropertyName("ShouldExecuteOnTimer")]
    private bool _ShouldExecuteOnTimer = true;

    /// <summary>
    /// Global Setting
    /// </summary>
    /// <remarks>Choose if commands should execute on timers (manual only).</remarks>
    [JsonIgnore]
    public bool ShouldExecuteOnTimer {
        get => _ShouldExecuteOnTimer;
        set {
            if (SetProperty(ref _ShouldExecuteOnTimer, value, Save.Yes, Notify.Yes)) {
                Settings.ShouldExecuteOnTimer.Value = value;
            }
        }
    }

    //...

    [JsonInclude]
    [JsonPropertyName("ShouldAutoNotificationsExpire")]
    private bool _ShouldAutoNotificationsExpire = true;

    /// <summary>
    /// Global Setting
    /// </summary>
    /// <remarks>Choose if notifications from timer expiration should expire.</remarks>
    [JsonIgnore]
    public bool ShouldAutoNotificationsExpire {
        get => _ShouldAutoNotificationsExpire;
        set {
            if (SetProperty(ref _ShouldAutoNotificationsExpire, value, Save.Yes, Notify.Yes)) {
                Settings.ShouldAutoNotificationsExpire.Value = value;
            }
        }
    }

    //...

    [JsonInclude]
    [JsonPropertyName("ShouldLog")]
    private bool _ShouldLog = true;

    /// <summary>
    /// Global Setting
    /// </summary>
    /// <remarks>Choose if commands should be logged.</remarks>
    [JsonIgnore]
    public bool ShouldLog {
        get => _ShouldLog;
        set {
            if (SetProperty(ref _ShouldLog, value, Save.Yes, Notify.Yes)) {
                Settings.ShouldLog.Value = value;
            }
        }
    }

    //...

    [JsonInclude]
    [JsonPropertyName("ShouldPromptByDefault")]
    private bool _ShouldPromptByDefault = true;

    /// <summary>
    /// Global Setting
    /// </summary>
    /// <remarks>Choose if 'Execute' button should prompt for confirmation.</remarks>
    [JsonIgnore]
    public bool ShouldPromptByDefault {
        get => _ShouldPromptByDefault;
        set {
            if (SetProperty(ref _ShouldPromptByDefault, value, Save.Yes, Notify.Yes)) {
                Settings.ShouldPromptByDefault.Value = value;
            }
        }
    }

    //...

    [JsonInclude]
    [JsonPropertyName("ShouldUsePasswordConfirmation")]
    private bool _ShouldUsePasswordConfirmation = false;

    /// <summary>
    /// Global Setting
    /// </summary>
    /// <remarks>Require password for confirmation windows.</remarks>
    [JsonIgnore]
    public bool ShouldUsePasswordConfirmation {
        get => _ShouldUsePasswordConfirmation;
        set {
            if (SetProperty(ref _ShouldUsePasswordConfirmation, value, Save.Yes, Notify.Yes)) {
                Settings.ShouldUsePasswordConfirmation.Value = value;
            }
        }
    }

    //...

    [JsonInclude]
    [JsonPropertyName("ShouldAutoStart")]
    private bool _ShouldAutoStart = true;

    /// <summary>
    /// Global Setting
    /// </summary>
    /// <remarks>Choose if AutoStart should trigger upon start up.</remarks>
    [JsonIgnore]
    public bool ShouldAutoStart {
        get => _ShouldAutoStart;
        set {
            if (SetProperty(ref _ShouldAutoStart, value, Save.Yes, Notify.Yes)) {
                Settings.ShouldAutoStart.Value = value;
            }
        }
    }

    //...

    [JsonInclude]
    [JsonPropertyName("ShouldExpandColorBar")]
    private bool _ShouldExpandColorBar = false;

    /// <summary>
    /// Global Setting
    /// </summary>
    /// <remarks>Choose if color should expand to timer item accents.</remarks>
    [JsonIgnore]
    public bool ShouldExpandColorBar {
        get => _ShouldExpandColorBar;
        set {
            if (SetProperty(ref _ShouldExpandColorBar, value, Save.Yes, Notify.Yes)) {
                Settings.ShouldExpandColorBar.Value = value;
            }
        }
    }

    //...

    [JsonInclude]
    [JsonPropertyName("ShouldStripeList")]
    private bool _ShouldStripeList = true;

    /// <summary>
    /// Global Setting
    /// </summary>
    /// <remarks>Choose if list items should have alternating background colors.</remarks>
    [JsonIgnore]
    public bool ShouldStripeList {
        get => _ShouldStripeList;
        set {
            if (SetProperty(ref _ShouldStripeList, value, Save.Yes, Notify.Yes)) {
                Settings.ShouldStripeList.Value = value;
            }
        }
    }

    //...

    [JsonInclude]
    [JsonPropertyName("ShouldCleanDatabase")]
    private bool _ShouldCleanDatabase = true;

    /// <summary>
    /// Global Setting
    /// </summary>
    /// <remarks>Choose if database should be cleaned on backup.</remarks>
    [JsonIgnore]
    public bool ShouldCleanDatabase {
        get => _ShouldCleanDatabase;
        set {
            if (SetProperty(ref _ShouldCleanDatabase, value, Save.Yes, Notify.Yes)) {
                Settings.ShouldCleanDatabase.Value = value;
            }
        }
    }

    //...

    [JsonInclude]
    [JsonPropertyName("MaxLines")]
    [Range(1, 5)]
    private int _MaxLines = 1;

    /// <summary>
    /// Global Setting
    /// </summary>
    /// <remarks>Choose how many lines should expand for description.</remarks>
    [JsonIgnore]
    [Range(1, 5)]
    public int MaxLines {
        get => _MaxLines;
        set {
            if (SetProperty(ref _MaxLines, value, Save.Yes, Notify.Yes)) {
                Settings.MaxLines.Value = value;
            }
        }
    }

    //...

    [JsonInclude]
    [JsonPropertyName("BackupVersions")]
    [Range(0, 10)]
    private int _BackupVersionsToKeep = 0;

    /// <summary>
    /// Global Setting
    /// </summary>
    /// <remarks>Choose how many backup versions to keep.</remarks>
    [JsonIgnore]
    [Range(0, 10)]
    public int BackupVersionsToKeep {
        get => _BackupVersionsToKeep;
        set {
            if (SetProperty(ref _BackupVersionsToKeep, value, Save.Yes, Notify.Yes)) {
                Settings.BackupVersionsToKeep.Value = value;
            }
        }
    }


    //...

    [JsonIgnore]
    private string _ThemeSelection = "Dark";
    [JsonInclude]
    [JsonPropertyName("ThemeSelection")]
    private string Serialized_ThemeSelection {
        get => _ThemeSelection;
        set {
            if (SetProperty(ref _ThemeSelection, value, Save.No, Notify.Yes, nameof(ThemeSelection))) {
                Application_ThemeSelectionChanged();
            }
        }
    }
    // Does not bind up to the Settings. Use Application.Current.ActualThemeVariant for notification and value.
    [JsonIgnore]
    public MenuItemViewModel ThemeSelection {
        get {
            if (GetMenuItem(ThemeSelections, _ThemeSelection) is not MenuItemViewModel menuItem) {
                menuItem = ThemeSelections[0];
                Serialized_ThemeSelection = menuItem.Header;
            }

            return menuItem;
        }

        private set {
            if (SetProperty(ref _ThemeSelection, value.Header, Save.Yes, Notify.Yes)) {
                Application_ThemeSelectionChanged();
            }
        }
    }

    [JsonIgnore]
    public ObservableCollection<MenuItemViewModel> ThemeSelections { get; }

    private void Command_ThemeSelection(object? parameter) => ThemeSelection = parameter as MenuItemViewModel ?? ThemeSelection;

    private static MenuItemViewModel? GetMenuItem(IEnumerable<MenuItemViewModel> menuItems, string itemName) => menuItems.FirstOrDefault((item) => item.Header == itemName);

    private void Application_ThemeSelectionChanged() {
        var variant = ThemeSelection.Header switch {
            var h when h == ThemeVariant.Dark.ToString() => ThemeVariant.Dark,
            var h when h == ThemeVariant.Light.ToString() => ThemeVariant.Light,
            var h when h == ThemeVariant.Default.ToString() => ThemeVariant.Default,
            _ => ThemeVariant.Default
        };

        Application_ApplyTheme(variant);
    }

    public static void Application_ApplyTheme(ThemeVariant theme) {
        if (Application.Current is null) return;
        Application.Current.RequestedThemeVariant = theme;
    }

    //...

    [JsonInclude]
    [JsonPropertyName("AccentColorSelection")]
    private AppColor _AccentColorSelection = ServiceProvider.Get<IColorProvider>().ApplicationBrush_Accent.Value;

    /// <summary>
    /// Global Setting
    /// </summary>
    /// <remarks>Choose the accent color of the application.</remarks>
    [JsonIgnore]
    public AppColor AccentColorSelection {
        get => _AccentColorSelection;
        set {
            if (value != _AccentColorSelection) {
                Settings.AccentColorSelection.Value = value;
                SetProperty(ref _AccentColorSelection, value, Save.Yes, Notify.Yes);
            }
        }
    }


}

