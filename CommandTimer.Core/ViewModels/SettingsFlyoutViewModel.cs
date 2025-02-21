using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using CommandTimer.Core.Utilities;
using CommandTimer.Core.ViewModels.MenuItems;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;

namespace CommandTimer.Core.ViewModels;

public class SettingsFlyoutViewModel : ViewModelBase {

    public SettingsFlyoutViewModel() {
        /// Global Action
        Core.ActionRelay.ActionPosted += (o, a) => {
            if (a.ActionKey == Core.Settings.Keys.ActionRelay_Serialization) {
                Serialize();
            }
        };

        /// Theme Selection Menu
        var themeSelectionMenu = new ThemeSelectionsMenu(Command_ThemeSelection);
        ThemeSelections = new ObservableCollection<MenuItemViewModel>(themeSelectionMenu.Items);
        _ThemeSelection = ThemeSelection.Header;

        /// Default Values
        _ShouldAnimate = Core.Settings.ShouldAnimate.Value;
        _ShouldExecuteOnTimer = Core.Settings.ShouldExecuteOnTimer.Value;
        _ShouldAutoNotificationsExpire = Core.Settings.ShouldAutoNotificationsExpire.Value;
        _ShouldLog = Core.Settings.ShouldLog.Value;
        _ShouldPromptByDefault = Core.Settings.ShouldPromptByDefault.Value;
        _ShouldAutoStart = Core.Settings.ShouldAutoStart.Value;
        _ShouldExpandColorBar = Core.Settings.ShouldExpandColorBar.Value;
        _ShouldCleanDatabase = Core.Settings.ShouldCleanDatabase.Value;
        _MaxLines = Core.Settings.MaxLines.Value;
        _BackupVersionsToKeep = Core.Settings.BackupVersionsToKeep.Value;
        _AccentColorSelection = Core.Settings.AccentColorSelection.Value;
    }

    public override void Serialize() => ServiceProvider.Get<ISerializer>().Serialize(Core.Settings.Keys.GlobalSettings, this, Core.Settings.DEFAULT_DATA_FILE);

    //...
    [JsonIgnore]
    private Core.Settings.AnimationChoice _ShouldAnimate = Core.Settings.AnimationChoice.All;

    /// Do the same thing as the base property, but not trigger serialize.
    /// Stops reserialize on serialize, while giving you a place to react to deserialized change.
    [JsonInclude]
    [JsonPropertyName("ShouldAnimate")]
    public bool Serialized_ShouldAnimate {
        get => _ShouldAnimate is Core.Settings.AnimationChoice.All;
        set {
            var _value = value ? Core.Settings.AnimationChoice.All : Core.Settings.AnimationChoice.None;
            /// WithoutNotify is just an explicit helper method to compare values. No notify because this property is not for binding.
            if (SetProperty(ref _ShouldAnimate, _value, Save.No, Notify.Yes, nameof(ShouldAnimate))) {
                /// When deserialized, this will still trigger the changed event from the StaticObservableProperty
                Core.Settings.ShouldAnimate.Value = _value;
            }
        }
    }

    /// <summary>
    /// Global Setting
    /// </summary>
    /// <remarks>Choose if controls should animate.</remarks>
    // This may change to more options.
    [JsonIgnore]
    public bool ShouldAnimate {
        get => _ShouldAnimate is Core.Settings.AnimationChoice.All;
        set {
            var _value = value ? Core.Settings.AnimationChoice.All : Core.Settings.AnimationChoice.None;
            if (SetProperty(ref _ShouldAnimate, _value, Save.Yes, Notify.Yes)) {
                Core.Settings.ShouldAnimate.Value = _value;
            }
        }
    }

    //...

    [JsonIgnore]
    private bool _ShouldExecuteOnTimer = true;

    [JsonInclude]
    [JsonPropertyName("ShouldExecuteOnTimer")]
    public bool Serialized_ShouldExecuteOnTimer {
        get => _ShouldExecuteOnTimer;
        set {
            if (SetProperty(ref _ShouldExecuteOnTimer, value, Save.No, Notify.Yes, nameof(ShouldExecuteOnTimer))) {
                Core.Settings.ShouldExecuteOnTimer.Value = value;
            }
        }
    }


    /// <summary>
    /// Global Setting
    /// </summary>
    /// <remarks>Choose if commands should execute on timers (manual only).</remarks>
    [JsonIgnore]
    public bool ShouldExecuteOnTimer {
        get => _ShouldExecuteOnTimer;
        set {
            if (SetProperty(ref _ShouldExecuteOnTimer, value, Save.Yes, Notify.Yes)) {
                Core.Settings.ShouldExecuteOnTimer.Value = value;
            }
        }
    }

    //...

    [JsonIgnore]
    private bool _ShouldAutoNotificationsExpire = true;

    [JsonInclude]
    [JsonPropertyName("ShouldAutoNotificationsExpire ")]
    public bool Serialized_ShouldNotificationsExpire {
        get => _ShouldAutoNotificationsExpire;
        set {
            if (SetProperty(ref _ShouldAutoNotificationsExpire, value, Save.No, Notify.Yes, nameof(ShouldAutoNotificationsExpire))) {
                Core.Settings.ShouldAutoNotificationsExpire.Value = value;
            }
        }
    }


    /// <summary>
    /// Global Setting
    /// </summary>
    /// <remarks>Choose if notifications from timer expiration should expire. (So you can see what ran while away) (manual only).</remarks>
    [JsonIgnore]
    public bool ShouldAutoNotificationsExpire {
        get => _ShouldAutoNotificationsExpire;
        set {
            if (SetProperty(ref _ShouldAutoNotificationsExpire, value, Save.Yes, Notify.Yes)) {
                Core.Settings.ShouldAutoNotificationsExpire.Value = value;
            }
        }
    }

    //...

    [JsonIgnore]
    private bool _ShouldLog = true;
    /// <summary>
    /// Global Setting
    /// </summary>
    /// <remarks>Choose if commands should be logged.</remarks>

    [JsonInclude]
    [JsonPropertyName("ShouldLog")]
    public bool Serialized_ShouldLog {
        get => _ShouldLog;
        set {
            if (SetProperty(ref _ShouldLog, value, Save.No, Notify.Yes, nameof(ShouldLog))) {
                Core.Settings.ShouldLog.Value = value;
            }
        }
    }

    [JsonIgnore]
    public bool ShouldLog {
        get => _ShouldLog;
        set {
            if (SetProperty(ref _ShouldLog, value, Save.Yes, Notify.Yes)) {
                Core.Settings.ShouldLog.Value = value;
            }
        }
    }

    //...

    private bool _ShouldPromptByDefault = true;

    [JsonInclude]
    [JsonPropertyName("ShouldPromptByDefault")]
    public bool Serialized_ShouldPromptByDefault {
        get => _ShouldPromptByDefault;
        set {
            if (SetProperty(ref _ShouldPromptByDefault, value, Save.No, Notify.Yes, nameof(ShouldPromptByDefault))) {
                Core.Settings.ShouldPromptByDefault.Value = value;
            }
        }
    }

    /// <summary>
    /// Global Setting
    /// </summary>
    /// <remarks>Choose if 'Execute' button should prompt for confirmation. (Prevent accidental execution)</remarks>
    [JsonIgnore]
    public bool ShouldPromptByDefault {
        get => _ShouldPromptByDefault;
        set {
            if (SetProperty(ref _ShouldPromptByDefault, value, Save.Yes, Notify.Yes)) {
                Core.Settings.ShouldPromptByDefault.Value = value;
            }
        }
    }

    //...

    private bool _ShouldUsePasswordConfirmation= false;

    [JsonInclude]
    [JsonPropertyName("ShouldUsePasswordConfirmation")]
    public bool Serialized_ShouldUsePasswordConfirmation{
        get => _ShouldUsePasswordConfirmation;
        set {
            if (SetProperty(ref _ShouldUsePasswordConfirmation, value, Save.No, Notify.Yes, nameof(ShouldUsePasswordConfirmation))) {
                Core.Settings.ShouldUsePasswordConfirmation.Value = value;
            }
        }
    }

    /// <summary>
    /// Global Setting
    /// </summary>
    /// <remarks>Choose if 'Execute' button should prompt for confirmation. (Prevent accidental execution)</remarks>
    [JsonIgnore]
    public bool ShouldUsePasswordConfirmation{
        get => _ShouldUsePasswordConfirmation;
        set {
            if (SetProperty(ref _ShouldUsePasswordConfirmation, value, Save.Yes, Notify.Yes)) {
                Core.Settings.ShouldUsePasswordConfirmation.Value = value;
            }
        }
    }

    //...

    private bool _ShouldAutoStart = true;

    [JsonInclude]
    [JsonPropertyName("ShouldAutoStart")]
    public bool Serialized_ShouldAutoStart {
        get => _ShouldAutoStart;
        set {
            if (SetProperty(ref _ShouldAutoStart, value, Save.No, Notify.Yes, nameof(ShouldAutoStart))) {
                Core.Settings.ShouldAutoStart.Value = value;
            }
        }
    }

    /// <summary>
    /// Global Setting
    /// </summary>
    /// <remarks>Choose if AutoStart should trigger upon start up.</remarks>
    [JsonIgnore]
    public bool ShouldAutoStart {
        get => _ShouldAutoStart;
        set {
            if (SetProperty(ref _ShouldAutoStart, value, Save.Yes, Notify.Yes)) {
                Core.Settings.ShouldAutoStart.Value = value;
            }
        }
    }

    //...

    [JsonIgnore]
    private bool _ShouldExpandColorBar = false;

    [JsonInclude]
    [JsonPropertyName("ShouldExpandColorBar")]
    public bool Serialized_ShouldExpandColorBar {
        get => _ShouldExpandColorBar;
        set {
            if (SetProperty(ref _ShouldExpandColorBar, value, Save.No, Notify.Yes, nameof(ShouldExpandColorBar))) {
                Core.Settings.ShouldExpandColorBar.Value = value;
            }
        }
    }


    /// <summary>
    /// Global Setting
    /// </summary>
    /// <remarks>Choose if color should expand to timer item accents.</remarks>
    [JsonIgnore]
    public bool ShouldExpandColorBar {
        get => _ShouldExpandColorBar;
        set {
            if (SetProperty(ref _ShouldExpandColorBar, value, Save.Yes, Notify.Yes)) {
                Core.Settings.ShouldExpandColorBar.Value = value;
            }
        }
    }

    //...

    private bool _ShouldStripeList = true;

    [JsonInclude]
    [JsonPropertyName("ShouldStripeList")]
    public bool Serialized_ShouldStripeList {
        get => _ShouldStripeList;
        set {
            if (SetProperty(ref _ShouldStripeList, value, Save.No, Notify.Yes, nameof(ShouldStripeList))) {
                Core.Settings.ShouldStripeList.Value = value;
            }
        }
    }

    /// <summary>
    /// Global Setting
    /// </summary>
    /// <remarks>Choose if AutoStart should trigger upon start up.</remarks>
    [JsonIgnore]
    public bool ShouldStripeList {
        get => _ShouldStripeList;
        set {
            if (SetProperty(ref _ShouldStripeList, value, Save.Yes, Notify.Yes)) {
                Core.Settings.ShouldStripeList.Value = value;
            }
        }
    }



    //...

    private bool _ShouldCleanDatabase = true;

    [JsonInclude]
    [JsonPropertyName("ShouldCleanDatabase")]
    public bool Serialized_ShouldCleanDatabase {
        get => _ShouldCleanDatabase;
        set {
            if (SetProperty(ref _ShouldCleanDatabase, value, Save.No, Notify.Yes, nameof(ShouldCleanDatabase))) {
                Core.Settings.ShouldCleanDatabase.Value = value;
            }
        }
    }

    /// <summary>
    /// Global Setting
    /// </summary>
    /// <remarks>Choose if AutoStart should trigger upon start up.</remarks>
    [JsonIgnore]
    public bool ShouldCleanDatabase {
        get => _ShouldCleanDatabase;
        set {
            if (SetProperty(ref _ShouldCleanDatabase, value, Save.Yes, Notify.Yes)) {
                Core.Settings.ShouldCleanDatabase.Value = value;
            }
        }
    }


    //...

    private int _MaxLines = 1;

    [Range(1, 5)]
    [JsonInclude]
    [JsonPropertyName("MaxLines")]
    public int Serialized_MaxLines {
        get => _MaxLines;
        set {
            if (SetProperty(ref _MaxLines, value, Save.No, Notify.Yes, nameof(MaxLines))) {
                Core.Settings.MaxLines.Value = value;
            }
        }
    }

    /// <summary>
    /// Global Setting
    /// </summary>
    /// <remarks>Choose how many lines should expand for description.</remarks>
    [Range(1, 5)]
    [JsonIgnore]
    public int MaxLines {
        get => _MaxLines;
        set {
            if (SetProperty(ref _MaxLines, value, Save.Yes, Notify.Yes)) {
                Core.Settings.MaxLines.Value = value;
            }
        }
    }


    //...

    private int _BackupVersionsToKeep = 0;

    [Range(0, 10)]
    [JsonInclude]
    [JsonPropertyName("BackupVersions")]
    public int Serialized_BackupVersionsToKeep {
        get => _BackupVersionsToKeep;
        set {
            if (SetProperty(ref _BackupVersionsToKeep, value, Save.No, Notify.Yes, nameof(BackupVersionsToKeep))) {
                Core.Settings.BackupVersionsToKeep.Value = value;
            }
        }
    }

    /// <summary>
    /// Global Setting
    /// </summary>
    /// <remarks>Choose how many lines should expand for description.</remarks>
    [Range(1, 5)]
    [JsonIgnore]
    public int BackupVersionsToKeep {
        get => _BackupVersionsToKeep;
        set {
            if (SetProperty(ref _BackupVersionsToKeep, value, Save.Yes, Notify.Yes)) {
                Core.Settings.BackupVersionsToKeep.Value = value;
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
    // Does not bind up to the Core.Settings. Use Application.Current.ActualThemeVariant for notification and value.
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

    private SolidColorBrush _AccentColorSelection = new(Core.Colors.ApplicationColor_Accent);

    [JsonInclude]
    [JsonPropertyName("AccentColorSelection")]
    public SolidColorBrush Serialized_AccentColorSelection {
        get => _AccentColorSelection;
        set {
            if (SetProperty(ref _AccentColorSelection, value, Save.No, Notify.Yes, nameof(AccentColorSelection))) {
                Core.Settings.AccentColorSelection.Value = new SolidColorBrush(value.Color);
                Core.Colors.SetBrush(nameof(Core.Colors.ApplicationBrush_Accent), value);
            }
        }
    }

    /// <summary>
    /// Global Setting
    /// </summary>
    /// <remarks>Choose the accent color of the application.</remarks>
    [JsonIgnore]
    public SolidColorBrush AccentColorSelection {
        get => _AccentColorSelection;
        set {
            if (value.Color != _AccentColorSelection.Color) {
                Core.Colors.ApplicationBrush_Accent = value;
                Core.Settings.AccentColorSelection.Value = value;
                // Alright broski... WTF. FluentTheme color follows the accent color via ValueChanged event from Core.Settings property.
                // Fluent is managed in the App.axaml and ''.cs files. The event is hit, but fluent is always one update behind???
                // If I set it twice then it updates correctly. But it only works if I set it here twice, not in the event.
                Core.Colors.ApplicationBrush_Accent = value;  // Contains all the colors for use.
                Core.Settings.AccentColorSelection.Value = value; // Contains the default and observable settings of the app.
                //TODO: Make all the Colors observable, remove the accent color from the Settings class.
                // because these two are in addition to the serialized value in the settings flyout view model.
                SetProperty(ref _AccentColorSelection, value, Save.Yes, Notify.Yes);
            }
        }
    }


}
