using CommandTimer.Core.ViewModels;
using System.Collections.Generic;

namespace CommandTimer.Desktop.Utilities;

public interface IDefaultTimerCollection {
    public IEnumerable<CommandTimerViewModel> Timers { get; }
}
