using CommandTimer.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandTimer.Core.ViewModels;

public partial class LibraryManager : ILibraryManager {

    //... Constructor

    public LibraryManager() {
        DeserializeState();

        InitializeLibraries();

        InitializeOnDelay();
    }


    //... Events

    public event EventHandler<CommandTimerLibrary>? CurrentLibraryChanging;
    public event EventHandler<CommandTimerLibrary>? CurrentLibraryChanged;
    public event EventHandler? LibraryAdded;
    public event EventHandler? LibraryRemoved;


    //... Collections

    private HashSet<string> Serialized_LibraryNames = [];
    public IEnumerable<string> LibraryNames => [.. Serialized_LibraryNames];

    private string Serialized_CurrentLibraryName = Core.Settings.Keys.DefaultLibrary;

    private HashSet<CommandTimerLibrary> _libraries = [];
    public IEnumerable<CommandTimerLibrary> Libraries => [.. _libraries];


    //... Fields

    private CommandTimerLibrary _CurrentLibrary = new() { LibraryName = Core.Settings.Keys.DefaultLibrary };
    public CommandTimerLibrary CurrentLibrary {
        get => _CurrentLibrary;
        private set {
            if (_CurrentLibrary != value) {
                _CurrentLibrary = value;
            }
        }
    }


    //... Public Methods — ILibraryManager

    public void SetCurrent(string libraryName) {
        if (string.IsNullOrWhiteSpace(libraryName)) return;

        OnCurrentLibraryChanging();

        CurrentLibrary.Unload();

        CurrentLibrary = GetLibrary(libraryName).Load();

        PersistLibrary(CurrentLibrary);

        Serialized_CurrentLibraryName = CurrentLibrary.LibraryName;
        SerializeState();

        OnCurrentLibraryChanged();
    }

    public CommandTimerLibrary RenameLibrary(CommandTimerLibrary library, string name) {
        if (string.IsNullOrWhiteSpace(name)) return library;
        if (library.LibraryName == name) return library;
        name = MakeUniqueLibraryName(name);

        var newLibrary = FindLibrary(name) ?? CreateNewLibrary(name);
        newLibrary.Load();

        var sortedTimers = library.CommandTimers.SortByName(o => o.Name).ToList();
        sortedTimers.ForEach(newLibrary.AddToLibrary);
        PersistLibrary(newLibrary);

        RemoveLibrary(library.LibraryName);
        return newLibrary;
    }

    public void RemoveLibrary(string libraryName, bool includeBackups = false)
        => RemoveLibrary(GetLibrary(libraryName), includeBackups);

    public CommandTimerLibrary? FindLibrary(string libraryName) => GetFromLoaded(libraryName)
                                                                 ?? GetFromSaveData(libraryName);

    public CommandTimerLibrary GetLibrary(string libraryName) => GetFromLoaded(libraryName)
                                                               ?? GetFromSaveData(libraryName)
                                                               ?? CreateNewLibrary(libraryName);

    // TODO: Move TriggerAutoStarts to a more appropriate location (e.g., a timer coordinator or engine manager)
    public void TriggerAutoStarts(bool active = true) {
        _libraries.SelectMany(library => library.CommandTimers)
                  .ForEach(timer => timer.IsActive = timer.IsAutoStart ? active && timer.IsAutoStart : timer.IsActive);
    }

    /// <summary>
    /// Create versioned backups of all library files and the global data file.
    /// </summary>
    public void BackupLibraries() {
        var serializer = ServiceProvider.Get<ISerializer>();
        _libraries.ForEach(library => serializer.BackupFile(library.LibraryName));
        serializer.BackupFile(Core.Settings.DEFAULT_DATA_FILE);
    }

    /// <summary>
    /// Re-serialize all libraries and timers in sorted order.
    /// Self-healing: writes fresh data from memory, clearing any file corruption or format drift.
    /// Libraries must be loaded before calling. Backup should be created before calling.
    /// </summary>
    public void CleanLibraries() {
        try {
            /// Ensure all libraries are loaded
            _libraries.ForEach(library => library.Load());

            /// Re-serialize global data (settings, password, etc.)
            Core.ActionRelay.OnActionPosted(nameof(LibraryManager), Core.Settings.Keys.ActionRelay_Serialization);
            SerializeState();

            /// Re-serialize all libraries and timers in sorted order
            _libraries = [.. _libraries.SortByName(o => o.LibraryName)];
            foreach (var library in _libraries) {
                library.Serialize();
                var sortedTimers = library.CommandTimers.SortByName(o => o.Name);
                foreach (var timer in sortedTimers) {
                    timer.Serialize();
                }
            }
        }
        catch (Exception ex) {
            /// Restore from the backup created before this method was called
            var serializer = ServiceProvider.Get<ISerializer>();
            serializer.RestoreFile(Core.Settings.DEFAULT_DATA_FILE);
            _libraries.ForEach(library => serializer.RestoreFile(library.LibraryName));

            Core.MessageRelay.OnMessagePosted(nameof(LibraryManager), $"Library cleaning failed. Previous files restored. {ex.Message}", Core.MessageRelay.MessageCategory.User);
        }
    }


    //... Private Methods

    private void RemoveLibrary(CommandTimerLibrary library, bool includeBackups = false) {
        _libraries.RemoveWhere((o) => o == library);
        Serialized_LibraryNames.Remove(library.LibraryName);
        ServiceProvider.Get<ISerializer>().DeleteFile(library.LibraryName);
        if (includeBackups) {
            ServiceProvider.Get<ISerializer>().DeleteBackupFiles(library.LibraryName);
        }
        OnLibraryRemoved();
        SerializeState();
    }

    private CommandTimerLibrary? GetFromLoaded(string libraryName) => _libraries.FirstOrDefault((o) => o.LibraryName == libraryName);
    private CommandTimerLibrary? GetFromSaveData(string libraryName) => ServiceProvider.Get<ISerializer>().Deserialize<CommandTimerLibrary>($"{Core.Settings.Keys.LibraryPrefix}{libraryName}", libraryName);
    private CommandTimerLibrary CreateNewLibrary(string libraryName) => new() { LibraryName = libraryName.Trim() };

    private void DeserializeState() {
        var serializer = ServiceProvider.Get<ISerializer>();
        var prefix = Core.Settings.Keys.LibraryManagerPrefix;
        var dataFile = Core.Settings.DEFAULT_DATA_FILE;

        /// Try current key, fall back to legacy key for existing save files
        var libraries = serializer.Deserialize<HashSet<string>>($"{prefix}{nameof(LibraryNames)}", dataFile)
                     ?? serializer.Deserialize<HashSet<string>>($"{prefix}LibrariesByName", dataFile);

        if (libraries is not null) {
            Serialized_LibraryNames = libraries;
        }

        /// Load last-selected library name
        if (serializer.Deserialize<string>($"{prefix}{nameof(Serialized_CurrentLibraryName)}", dataFile) is string savedName) {
            Serialized_CurrentLibraryName = savedName;
        }
    }

    private void SerializeState() {
        var serializer = ServiceProvider.Get<ISerializer>();
        var prefix = Core.Settings.Keys.LibraryManagerPrefix;
        var dataFile = Core.Settings.DEFAULT_DATA_FILE;

        serializer.Serialize($"{prefix}{nameof(LibraryNames)}", Serialized_LibraryNames, dataFile);
        serializer.Serialize($"{prefix}{nameof(Serialized_CurrentLibraryName)}", Serialized_CurrentLibraryName, dataFile);
    }

    private bool PersistLibrary(CommandTimerLibrary library) {
        bool added = false;
        if (Serialized_LibraryNames.Add(library.LibraryName)) {
            Serialized_LibraryNames = [.. Serialized_LibraryNames.SortByName()];
            SerializeState();
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

    private string MakeUniqueLibraryName(string name) {
        int counter = 1;
        string baseName = GetBaseName(name);
        string uniqueName = name;

        while (Serialized_LibraryNames.Contains(uniqueName)) {
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

    private void InitializeLibraries() {
        /// Default Case
        if (Serialized_LibraryNames.Count is 0) {
            PersistLibrary(GetLibrary(Core.Settings.Keys.DefaultLibrary).Load());
        }

        /// Load all libraries into memory
        LibraryNames.ForEach(l => PersistLibrary(GetLibrary(l).Load()));

        /// Restore last-selected library as current
        var libraryToLoad = FindLibrary(Serialized_CurrentLibraryName)?.LibraryName ?? LibraryNames.FirstOrDefault() ?? Core.Settings.Keys.DefaultLibrary;
        CurrentLibrary = GetLibrary(libraryToLoad);
        CurrentLibrary.Load();

        /// Self-healing re-serialize from memory
        if (Core.Settings.ShouldCleanDatabase.Value) {
            BackupLibraries();
            CleanLibraries();
        }

        /// Unload all non-current libraries
        _libraries.Where(l => l != CurrentLibrary)
                  .ForEach(library => library.Unload());

        OnLibraryAdded();
    }

    private async void InitializeOnDelay() {
        await Task.Delay(1000);

        TriggerAutoStarts();
    }

    private void OnCurrentLibraryChanging() => CurrentLibraryChanging?.Invoke(nameof(LibraryManager), CurrentLibrary);
    private void OnCurrentLibraryChanged() => CurrentLibraryChanged?.Invoke(nameof(LibraryManager), CurrentLibrary);
    private void OnLibraryAdded() => LibraryAdded?.Invoke(nameof(LibraryManager), EventArgs.Empty);
    private void OnLibraryRemoved() => LibraryRemoved?.Invoke(nameof(LibraryManager), EventArgs.Empty);
}
