using CommandTimer.Core.ViewModels;
using System;
using System.Collections.Generic;

namespace CommandTimer.Core.Utilities;

public interface ILibraryManager {

    //... Events
    event EventHandler<CommandTimerLibrary>? CurrentLibraryChanging;
    event EventHandler<CommandTimerLibrary>? CurrentLibraryChanged;
    event EventHandler? LibraryAdded;
    event EventHandler? LibraryRemoved;

    //... State
    CommandTimerLibrary CurrentLibrary { get; }
    IEnumerable<CommandTimerLibrary> Libraries { get; }
    IEnumerable<string> LibraryNames { get; }

    //... Library Lifecycle
    void SetCurrent(string libraryName);
    CommandTimerLibrary RenameLibrary(CommandTimerLibrary library, string name);
    void RemoveLibrary(string libraryName, bool includeBackups = false);

    //... Lookup
    CommandTimerLibrary? FindLibrary(string libraryName);
    CommandTimerLibrary GetLibrary(string libraryName);

    //... Timer Operations
    // TODO: Move TriggerAutoStarts to a more appropriate location (e.g., a timer coordinator or engine manager)
    void TriggerAutoStarts(bool active = true);

    //... Maintenance
    void BackupLibraries();
    void CleanLibraries();
}
