using System.Text.Json.Serialization;

namespace CommandTimer.Core.ViewModels;

public partial class CommandTimerLibrary {

    // TODO: I would like to clean up this class. The adding, tracking, removing... Encapsulate the timer name with the associated names methods.

    public CommandTimerLibrary() : this(new CommandTimerLibraryData()) { }

    public CommandTimerLibrary(CommandTimerLibraryData data) {
        Data = data;
    }

    internal CommandTimerLibraryData Data { get; }

    /// <summary>
    /// Serialized value unique to this library.
    /// </summary>
    public string LibraryName {
        get => Data.LibraryName;
        init => Data.LibraryName = value;
    }

    /// <summary>
    /// The library manages a list of names. Upon load, those names access saved data to create instances of CommandTimers held in a collection.
    /// </summary>
    public IEnumerable<string> Names => Data.TimerNames;

    public bool IsLoaded { get; private set; }
    public bool IsLoading { get; private set; }

    public IEnumerable<CommandTimerViewModel> CommandTimers => _timerInstances;
    private readonly List<CommandTimerViewModel> _timerInstances = [];

    private bool _shouldNotifyChange = true;

    //...

    public event EventHandler<CommandTimerViewModel>? TimerAdded;
    public event EventHandler<CommandTimerViewModel>? TimerRemoved;

    //...

    public CommandTimerLibrary Load() {
        if (IsLoaded) return this;

        IsLoading = true;
        _shouldNotifyChange = false;

        var copyOfNames = Data.TimerNames.ToList();
        foreach (var name in copyOfNames) {
            if (_timerInstances.Any((o) => o.Name == name)) continue;
            if (Deserialize(name) is CommandTimerViewModel fromSavedData) {
                _timerInstances.Add(fromSavedData);
            }
            else {
                TryRemoveName(name);
                AddToLibrary(CommandTimerViewModel.Create(name));
                /// Will trigger serialization at ViewModel level.
                MessageRelay.OnMessagePosted(this, $"No data was found for '{name}'. A default template has been added.", MessageRelay.MessageCategory.User);
            }
        }

        _shouldNotifyChange = true;
        IsLoaded = true;
        IsLoading = false;
        Serialize();
        return this;
    }

    public void Unload() {
        if (IsLoading) return;
        for (int i = _timerInstances.Count - 1; i >= 0; i--) {
            var instance = _timerInstances[i];
            if (instance.IsAutoStart || instance.IsActive) continue;
            _timerInstances.RemoveAt(i);
            instance.Unsubscribe();
        }
        IsLoaded = false;
    }

    public void Serialize()
        => ServiceProvider.Get<ISerializer>().Serialize($"{Settings.Keys.LibraryPrefix}{LibraryName}", Data, LibraryName);

    private CommandTimerViewModel? Deserialize(string name) {
        var data = ServiceProvider.Get<ISerializer>().Deserialize<CommandTimerData>($"{Settings.Keys.CommandTimerPrefix}{name}", LibraryName);
        return data is not null ? new CommandTimerViewModel(data) : null;
    }

    //... Names

    private bool TryAddName(string name)
        => Data.TimerNames.Add(name);

    private bool TryRemoveName(string name)
        => Data.TimerNames.Remove(name);

    public bool ContainsName(string name)
        => Data.TimerNames.Contains(name);

    public CommandTimerViewModel? GetSingleTimerInstance(string name) {
        var wasNotLoaded = !IsLoaded;
        Load();
        var timer = CommandTimers.FirstOrDefault(timer => timer.Name == name);
        if (wasNotLoaded) Unload();
        return timer;
    }

    public string MakeUniqueTimerName(string name) {
        int counter = 1;
        string baseName = GetBaseName(name);
        string uniqueName = name;

        while (Data.TimerNames.Contains(uniqueName)) {
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

    [System.Text.RegularExpressions.GeneratedRegex(@"^(.*)\s*\((\d+)\)\s*$")]
    private static partial System.Text.RegularExpressions.Regex AppendedModifier();


    //... Timers

    private void OnTimerAdded(CommandTimerViewModel timer) {
        if (_shouldNotifyChange) {
            TimerAdded?.Invoke(this, timer);
        }
    }

    private void OnTimerRemoved(CommandTimerViewModel timer) {
        try {
            if (_shouldNotifyChange) {
                TimerRemoved?.Invoke(this, timer);
            }
        }
        finally {
            timer.Unsubscribe();
        }
    }

    private void TrackInstance(CommandTimerViewModel timer) {
        _timerInstances.Add(timer);
        TryAddName(timer.Name);
    }

    private void UntrackInstance(CommandTimerViewModel timer) {
        _timerInstances.Remove(timer);
        TryRemoveName(timer.Name);
    }

    public void AddToLibrary(CommandTimerViewModel newTimer) {
        var wasNotLoaded = !IsLoaded;
        Load();

        newTimer.LibraryName = LibraryName;
        newTimer.Name = MakeUniqueTimerName(newTimer.Name);

        TrackInstance(newTimer);

        newTimer.Serialize();
        Serialize();

        if (wasNotLoaded) {
            Unload();
        }

        OnTimerAdded(newTimer);
    }

    public void RemoveFromLibrary(CommandTimerViewModel timer) {
        var wasNotLoaded = !IsLoaded;
        Load();

        timer.Unserialize();
        UntrackInstance(timer);
        Serialize();

        if (wasNotLoaded) Unload();

        OnTimerRemoved(timer);
    }

    public void ChangeTimerNameTo(CommandTimerViewModel timer, string newName) {
        newName = string.IsNullOrWhiteSpace(newName)
            ? CommandTimerViewModel.PLACEHOLDER_NAME
            : newName.Trim();

        timer.Unserialize();
        UntrackInstance(timer);

        timer.Name = MakeUniqueTimerName(newName);

        TrackInstance(timer);
        Serialize();
        timer.Serialize();
    }
}

