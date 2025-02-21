using CommandTimer.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandTimer.Core.ViewModels;
public static partial class LibraryManager {

    //... Static Constructor
    static LibraryManager() {
        Deserialize();

        InitializeLibraries();

        InitializeOnDelay();
    }


    //... Events
    public static event EventHandler<CommandTimerLibrary>? CurrentLibraryChanging;
    public static event EventHandler<CommandTimerLibrary>? CurrentLibraryChanged;
    public static event EventHandler? LibraryAdded;
    public static event EventHandler? LibraryRemoved;


    //... Collections
    private static HashSet<string> Serialized_LibrariesByName = [];
    public static IEnumerable<string> LibrariesByName => [.. Serialized_LibrariesByName];

    private static HashSet<CommandTimerLibrary> _libraries = [];
    public static IEnumerable<CommandTimerLibrary> Libraries => [.. _libraries];


    //... Fields
    public const string DEFAULT_LIBRARY = "Default";

    private static CommandTimerLibrary _CurrentLibrary = new() { LibraryName = DEFAULT_LIBRARY };
    public static CommandTimerLibrary CurrentLibrary {
        get => _CurrentLibrary;
        private set {
            if (_CurrentLibrary != value) {
                _CurrentLibrary = value;
            }
        }
    }


    //... Public Methods

    public static void LoadLibraryToCurrent(string libraryName) {
        if (string.IsNullOrWhiteSpace(libraryName)) return;

        OnCurrentLibraryChanging();

        CurrentLibrary.Unload();

        var library = GetLibrary(libraryName);
        library.Load();

        CurrentLibrary = library;

        SerializeLibrary(CurrentLibrary);

        OnCurrentLibraryChanged();
    }

    public static CommandTimerLibrary RenameLibrary(CommandTimerLibrary library, string name) {
        if (string.IsNullOrWhiteSpace(name)) return library;
        if (library.LibraryName == name) return library;
        name = MakeUniqueLibraryName(name);

        var newLibrary = FindLibrary(name) ?? CreateNewLibrary(name);
        newLibrary.Load();

        var sortedTimers = library.CommandTimers.SortByName(o => o.Name).ToList();
        sortedTimers.ForEach(newLibrary.AddToLibrary);
        SerializeLibrary(newLibrary);

        RemoveLibrary(library.LibraryName);
        return newLibrary;
    }

    public static void SetCurrentLibrary(CommandTimerLibrary library) {
        OnCurrentLibraryChanging();

        library.Load();

        CurrentLibrary = library;

        OnCurrentLibraryChanged();
    }

    public static void RemoveLibrary(string libraryName, bool includeBackups = false) 
        => RemoveLibrary(GetLibrary(libraryName), includeBackups);

    public static void RemoveLibrary(CommandTimerLibrary library, bool includeBackups = false) {
        _libraries.RemoveWhere((o) => o == library);
        Serialized_LibrariesByName.Remove(library.LibraryName);
        ServiceProvider.Get<ISerializer>().DeleteFile(library.LibraryName);
        if (includeBackups) {
            ServiceProvider.Get<ISerializer>().DeleteBackupFiles(library.LibraryName);
        }
        OnLibraryRemoved();
        Serialize();
    }

    public static CommandTimerLibrary? FindLibrary(string libraryName) => GetFromLoaded(libraryName)
                                                                       ?? GetFromSaveData(libraryName);
    public static CommandTimerLibrary GetLibrary(string libraryName) => GetFromLoaded(libraryName)
                                                                     ?? GetFromSaveData(libraryName)
                                                                     ?? CreateNewLibrary(libraryName);


    //... Private Methods

    private static CommandTimerLibrary? GetFromLoaded(string libraryName) => _libraries.FirstOrDefault((o) => o.LibraryName == libraryName);
    private static CommandTimerLibrary? GetFromSaveData(string libraryName) => ServiceProvider.Get<ISerializer>().Deserialize<CommandTimerLibrary>($"{Core.Settings.Keys.LibraryPrefix}{libraryName}", libraryName);
    private static CommandTimerLibrary CreateNewLibrary(string libraryName) => new() { LibraryName = libraryName.Trim() };
    private static void Deserialize() {
        if (ServiceProvider.Get<ISerializer>().Deserialize<HashSet<string>>($"{Core.Settings.Keys.LibraryManagerPrefix}{nameof(LibrariesByName)}", Core.Settings.DEFAULT_DATA_FILE) is HashSet<string> libraries) {
            Serialized_LibrariesByName = libraries;
        }
    }
    private static void Serialize() => ServiceProvider.Get<ISerializer>().Serialize($"{Core.Settings.Keys.LibraryManagerPrefix}{nameof(LibrariesByName)}", Serialized_LibrariesByName, Core.Settings.DEFAULT_DATA_FILE);

    private static bool SerializeLibrary(CommandTimerLibrary library) {
        bool added = false;
        if (Serialized_LibrariesByName.Add(library.LibraryName)) {
            Serialized_LibrariesByName = new(Serialized_LibrariesByName.SortByName());
            Serialize();
            added = true;
        }
        if (_libraries.Add(library)) {            
            library.Serialize();
            added = true;
        }
        if (added) {
            OnLibraryAdded();
        }
        return added;
    }


    [System.Text.RegularExpressions.GeneratedRegex(@"^(.*)\s*\((\d+)\)\s*$")]
    private static partial System.Text.RegularExpressions.Regex AppendedModifier();

    private static string MakeUniqueLibraryName(string name) {
        int counter = 1;
        string baseName = GetBaseName(name);
        string uniqueName = name;

        while (Serialized_LibrariesByName.Contains(uniqueName)) {
            uniqueName = $"{baseName} ({counter++})";
        }
        return uniqueName;
    }

    private static string GetBaseName(string name) {
        var match = AppendedModifier().Match(name);
        if (match.Success) {
            return match.Groups[1].Value.Trim();
        }
        return name.Trim();
    }

    private static void InitializeLibraries() {
        /// Default Case
        if (Serialized_LibrariesByName.Count is 0) {
            LoadLibraryToCurrent(DEFAULT_LIBRARY);
        }

        LibrariesByName.ForEach(l => SerializeLibrary(GetLibrary(l).Load()));

        LibrariesByName.ForEach(l => GetLibrary(l).Unload());

        OnLibraryAdded();
    }

    private static async void InitializeOnDelay() {
        await Task.Delay(1000);

        TriggerAutoStarts();
    }

    public static void TriggerAutoStarts(bool active = true) {
        _libraries.SelectMany(library => library.CommandTimers)
                  .ForEach(timer => timer.IsActive = timer.IsAutoStart ? active && timer.IsAutoStart : timer.IsActive);
    }

    public static void CleanDatabase() {
        if (Core.Settings.ShouldCleanDatabase.Value is false) return;
        var serializer = ServiceProvider.Get<ISerializer>();
        List<string> removed = [];

        try {
            /// Load
            _libraries.ForEach(library => library.Load());

            /// Backup
            _libraries.ForEach(library => {
                if (serializer.BackupFile(library.LibraryName)) {
                    removed.Add(library.LibraryName);
                }
            });
            if (serializer.BackupFile(Core.Settings.DEFAULT_DATA_FILE)) {
                removed.Add(Core.Settings.DEFAULT_DATA_FILE);
            }

            /// Serialize
            Core.ActionRelay.OnActionPosted(nameof(LibraryManager), Core.Settings.Keys.ActionRelay_Serialization);
            Serialize();

            /// Serialize Libraries in order
            _libraries = new(_libraries.SortByName(o => o.LibraryName));
            foreach (var library in _libraries) {
                library.Serialize();
                var sortedTimers = library.CommandTimers.SortByName(o => o.Name);
                foreach (var timer in sortedTimers) {
                    timer.Serialize();
                }
            }

            /// Unload
            _libraries.Where(l => l != CurrentLibrary)
                      .ForEach(library => library.Unload());
        }
        catch (Exception ex) {
            /// Restore Last
            serializer.RestoreFile(Core.Settings.DEFAULT_DATA_FILE);
            removed.ForEach(serializer.RestoreFile);

            Core.MessageRelay.OnMessagePosted(nameof(LibraryManager), $"Database cleaning while starting failed. Previous files restored. {ex.Message}", Core.MessageRelay.MessageCategory.User);
        }
    }

    private static void OnCurrentLibraryChanging() => CurrentLibraryChanging?.Invoke(nameof(LibraryManager), CurrentLibrary);
    private static void OnCurrentLibraryChanged() => CurrentLibraryChanged?.Invoke(nameof(LibraryManager), CurrentLibrary);
    private static void OnLibraryAdded() => LibraryAdded?.Invoke(nameof(LibraryManager), EventArgs.Empty);
    private static void OnLibraryRemoved() => LibraryRemoved?.Invoke(nameof(LibraryManager), EventArgs.Empty);
}
