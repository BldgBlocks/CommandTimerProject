using CommandTimer.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace CommandTimer.Core.ViewModels;

public partial class CommandTimerLibrary {

    // TODO: I would like to clean up this class. The adding, tracking, removing... Encapsulate the timer name with the associated names methods.

    /// <summary>
    /// Serialized value unique to this library.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("LibraryName")]
    public required string LibraryName { get; init; }

    /// <summary>
    /// The library manages a list of names. Upon load, those names access saved data to create instances of CommandTimers held in a collection.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("TimersByName")]
    private HashSet<string> TimerNames = [];

    [JsonIgnore]
    public bool IsLoaded { get; private set; }
    [JsonIgnore]
    public bool IsLoading { get; private set; }

    [JsonIgnore]
    public IEnumerable<CommandTimerViewModel> CommandTimers => _timerInstances;
    [JsonIgnore]
    private readonly List<CommandTimerViewModel> _timerInstances = [];

    [JsonIgnore]
    public IEnumerable<string> Names => TimerNames;
    [JsonIgnore]
    private bool _shouldNotifyChange = true;

    //...

    public event EventHandler<CommandTimerViewModel>? TimerAdded;
    public event EventHandler<CommandTimerViewModel>? TimerRemoved;

    //...

    public CommandTimerLibrary Load() {
        IsLoaded = false;
        IsLoading = true;
        //TimerNames = new HashSet<string>(TimerNames.SortByName());
        _shouldNotifyChange = false;

        var copyOfNames = TimerNames.ToList();
        foreach (var name in copyOfNames) {
            if (_timerInstances.Any((o) => o.Name == name)) continue;  
            if (Deserialize(name) is CommandTimerViewModel fromSavedData) {
                _timerInstances.Add(fromSavedData);
            }
            else {
                TryRemoveName(name);
                AddToLibrary(CommandTimerViewModel.Create(name));
                /// Will trigger serialization at ViewModel level.
                Core.MessageRelay.OnMessagePosted(this, $"No data was found for '{name}'. A default template has been added.", Core.MessageRelay.MessageCategory.User);
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
        => ServiceProvider.Get<ISerializer>().Serialize($"{Core.Settings.Keys.LibraryPrefix}{LibraryName}", this, LibraryName);

    private CommandTimerViewModel? Deserialize(string name)
        => ServiceProvider.Get<ISerializer>().Deserialize<CommandTimerViewModel>($"{Core.Settings.Keys.CommandTimerPrefix}{name}", LibraryName);

    //... Names

    private bool TryAddName(string name) 
        => TimerNames.Add(name);

    private bool TryRemoveName(string name)
        => TimerNames.Remove(name);

    public bool ContainsName(string name) 
        => TimerNames.Contains(name);

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

        while (TimerNames.Contains(uniqueName)) {
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
        newTimer.Unserialize();

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
        timer.Unserialize();
        UntrackInstance(timer);

        timer.Name = newName;

        TrackInstance(timer);
        Serialize();
        timer.Serialize();
    }
}
