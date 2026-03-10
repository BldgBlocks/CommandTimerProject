using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using CommandTimer.Core.ViewModels.MenuItems;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace CommandTimer.Core.ViewModels;

public class SettingsFlyoutViewModel : ViewModelBase {

    public SettingsFlyoutViewModel() : this(new SettingsFlyoutData()) { }

    public SettingsFlyoutViewModel(SettingsFlyoutData data) {
        Data = data;

        /// Global Action
        ActionRelay.ActionPosted += (o, a) => {
            if (a.ActionKey == Settings.Keys.ActionRelay_Serialization) {
                Serialize();
            }
        };

        /// Theme Selection Menu
        var themeSelectionMenu = new ThemeSelectionsMenu(Command_ThemeSelection);
        ThemeSelections = new ObservableCollection<MenuItemViewModel>(themeSelectionMenu.Items);
        InitializeThemeSelection();

        ApplyRuntimeStateFromData();
    }

    internal SettingsFlyoutData Data { get; }

    public override void Serialize() => ServiceProvider.Get<ISerializer>().Serialize(Settings.Keys.GlobalSettings, Data, Settings.DEFAULT_DATA_FILE);

    /// <summary>
    /// Restore the static runtime Settings state from the data object after construction.
    /// The data is the persistence authority, while Settings remains the runtime authority read throughout the app.
    /// </summary>
    private void ApplyRuntimeStateFromData() {
        Settings.ShouldAnimate.Value = Data.ShouldAnimate;
        Settings.ShouldExecuteOnTimer.Value = Data.ShouldExecuteOnTimer;
        Settings.ShouldAutoNotificationsExpire.Value = Data.ShouldAutoNotificationsExpire;
        Settings.ShouldLog.Value = Data.ShouldLog;
        Settings.ShouldPromptByDefault.Value = Data.ShouldPromptByDefault;
        Settings.ShouldUsePasswordConfirmation.Value = Data.ShouldUsePasswordConfirmation;
        Settings.ShouldAutoStart.Value = Data.ShouldAutoStart;
        Settings.ShouldExpandColorBar.Value = Data.ShouldExpandColorBar;
        Settings.ShouldStripeList.Value = Data.ShouldStripeList;
        Settings.ShouldCleanDatabase.Value = Data.ShouldCleanDatabase;
        Settings.MaxLines.Value = Data.MaxLines;
        Settings.BackupVersionsToKeep.Value = Data.BackupVersionsToKeep;
        Settings.AccentColorSelection.Value = Data.AccentColorSelection;

        Application_ThemeSelectionChanged();
    }

    //...

    /// <summary>
    /// Global Setting
    /// </summary>
    /// <remarks>Choose if controls should animate.</remarks>
    public bool ShouldAnimate {
        get => Data.ShouldAnimate is Settings.AnimationChoice.All;
        set {
            var newValue = value ? Settings.AnimationChoice.All : Settings.AnimationChoice.None;
            var oldValue = Data.ShouldAnimate;
            Data.ShouldAnimate = newValue;
            if (SetPropertySaveNotify(oldValue, newValue)) {
                Settings.ShouldAnimate.Value = newValue;
            }
        }
    }

    //...

    /// <summary>
    /// Global Setting
    /// </summary>
    /// <remarks>Choose if commands should execute on timers (manual only).</remarks>
    public bool ShouldExecuteOnTimer {
        get => Data.ShouldExecuteOnTimer;
        set {
            var oldValue = Data.ShouldExecuteOnTimer;
            Data.ShouldExecuteOnTimer = value;
            if (SetPropertySaveNotify(oldValue, value)) {
                Settings.ShouldExecuteOnTimer.Value = value;
            }
        }
    }

    //...

    /// <summary>
    /// Global Setting
    /// </summary>
    /// <remarks>Choose if notifications from timer expiration should expire.</remarks>
    public bool ShouldAutoNotificationsExpire {
        get => Data.ShouldAutoNotificationsExpire;
        set {
            var oldValue = Data.ShouldAutoNotificationsExpire;
            Data.ShouldAutoNotificationsExpire = value;
            if (SetPropertySaveNotify(oldValue, value)) {
                Settings.ShouldAutoNotificationsExpire.Value = value;
            }
        }
    }

    //...

    /// <summary>
    /// Global Setting
    /// </summary>
    /// <remarks>Choose if commands should be logged.</remarks>
    public bool ShouldLog {
        get => Data.ShouldLog;
        set {
            var oldValue = Data.ShouldLog;
            Data.ShouldLog = value;
            if (SetPropertySaveNotify(oldValue, value)) {
                Settings.ShouldLog.Value = value;
            }
        }
    }

    //...

    /// <summary>
    /// Global Setting
    /// </summary>
    /// <remarks>Choose if 'Execute' button should prompt for confirmation.</remarks>
    public bool ShouldPromptByDefault {
        get => Data.ShouldPromptByDefault;
        set {
            var oldValue = Data.ShouldPromptByDefault;
            Data.ShouldPromptByDefault = value;
            if (SetPropertySaveNotify(oldValue, value)) {
                Settings.ShouldPromptByDefault.Value = value;
            }
        }
    }

    //...

    /// <summary>
    /// Global Setting
    /// </summary>
    /// <remarks>Require password for confirmation windows.</remarks>
    public bool ShouldUsePasswordConfirmation {
        get => Data.ShouldUsePasswordConfirmation;
        set {
            var oldValue = Data.ShouldUsePasswordConfirmation;
            Data.ShouldUsePasswordConfirmation = value;
            if (SetPropertySaveNotify(oldValue, value)) {
                Settings.ShouldUsePasswordConfirmation.Value = value;
            }
        }
    }

    //...

    /// <summary>
    /// Global Setting
    /// </summary>
    /// <remarks>Choose if AutoStart should trigger upon start up.</remarks>
    public bool ShouldAutoStart {
        get => Data.ShouldAutoStart;
        set {
            var oldValue = Data.ShouldAutoStart;
            Data.ShouldAutoStart = value;
            if (SetPropertySaveNotify(oldValue, value)) {
                Settings.ShouldAutoStart.Value = value;
            }
        }
    }

    //...

    /// <summary>
    /// Global Setting
    /// </summary>
    /// <remarks>Choose if color should expand to timer item accents.</remarks>
    public bool ShouldExpandColorBar {
        get => Data.ShouldExpandColorBar;
        set {
            var oldValue = Data.ShouldExpandColorBar;
            Data.ShouldExpandColorBar = value;
            if (SetPropertySaveNotify(oldValue, value)) {
                Settings.ShouldExpandColorBar.Value = value;
            }
        }
    }

    //...

    /// <summary>
    /// Global Setting
    /// </summary>
    /// <remarks>Choose if list items should have alternating background colors.</remarks>
    public bool ShouldStripeList {
        get => Data.ShouldStripeList;
        set {
            var oldValue = Data.ShouldStripeList;
            Data.ShouldStripeList = value;
            if (SetPropertySaveNotify(oldValue, value)) {
                Settings.ShouldStripeList.Value = value;
            }
        }
    }

    //...

    /// <summary>
    /// Global Setting
    /// </summary>
    /// <remarks>Choose if database should be cleaned on backup.</remarks>
    public bool ShouldCleanDatabase {
        get => Data.ShouldCleanDatabase;
        set {
            var oldValue = Data.ShouldCleanDatabase;
            Data.ShouldCleanDatabase = value;
            if (SetPropertySaveNotify(oldValue, value)) {
                Settings.ShouldCleanDatabase.Value = value;
            }
        }
    }

    //...

    /// <summary>
    /// Global Setting
    /// </summary>
    /// <remarks>Choose how many lines should expand for description.</remarks>
    [Range(1, 5)]
    public int MaxLines {
        get => Data.MaxLines;
        set {
            var oldValue = Data.MaxLines;
            Data.MaxLines = value;
            if (SetPropertySaveNotify(oldValue, value)) {
                Settings.MaxLines.Value = value;
            }
        }
    }

    //...

    /// <summary>
    /// Global Setting
    /// </summary>
    /// <remarks>Choose how many backup versions to keep.</remarks>
    [Range(0, 10)]
    public int BackupVersionsToKeep {
        get => Data.BackupVersionsToKeep;
        set {
            var oldValue = Data.BackupVersionsToKeep;
            Data.BackupVersionsToKeep = value;
            if (SetPropertySaveNotify(oldValue, value)) {
                Settings.BackupVersionsToKeep.Value = value;
            }
        }
    }


    //...

    // Does not bind up to the Settings. Use Application.Current.ActualThemeVariant for notification and value.
    public MenuItemViewModel ThemeSelection {
        get {
            if (GetMenuItem(ThemeSelections, Data.ThemeSelection) is not MenuItemViewModel menuItem) {
                menuItem = ThemeSelections[0];
                Data.ThemeSelection = menuItem.Header;
            }

            return menuItem;
        }

        private set {
            var oldValue = Data.ThemeSelection;
            Data.ThemeSelection = value.Header;
            if (SetPropertySaveNotify(oldValue, value.Header)) {
                Application_ThemeSelectionChanged();
            }
        }
    }

    public ObservableCollection<MenuItemViewModel> ThemeSelections { get; }

    private void Command_ThemeSelection(object? parameter) => ThemeSelection = parameter as MenuItemViewModel ?? ThemeSelection;

    private static MenuItemViewModel? GetMenuItem(IEnumerable<MenuItemViewModel> menuItems, string itemName) => menuItems.FirstOrDefault((item) => item.Header == itemName);

    private void InitializeThemeSelection() {
        if (GetMenuItem(ThemeSelections, Data.ThemeSelection) is null) {
            Data.ThemeSelection = ThemeSelections[0].Header;
        }
    }

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

    /// <summary>
    /// Global Setting
    /// </summary>
    /// <remarks>Choose the accent color of the application.</remarks>
    public AppColor AccentColorSelection {
        get => Data.AccentColorSelection;
        set {
            var oldValue = Data.AccentColorSelection;
            Data.AccentColorSelection = value;
            if (SetPropertySaveNotify(oldValue, value)) {
                Settings.AccentColorSelection.Value = value;
            }
        }
    }

}

