// Ignore Spelling: Deserialized

using CommandTimer.Core.ViewModels;
using ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CommandTimer.Core.Utilities;

public static partial class CommandTimerItemSortExtensions {

    /// <summary>
    /// Method chaining version of ObservableCollection.Clear()
    /// </summary>
    public static ObservableCollection<T> Reset<T>(this ObservableCollection<T> collection) {
        collection.Clear();
        return collection;
    }

    public static void AddRange<T>(this ObservableCollection<T> collection, ObservableCollection<T> range) 
        => range.ForEach(collection.Add);
    public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> range)
        => range.ForEach(collection.Add);

    public static void RemoveRange<T>(this ObservableCollection<T> list, int start, int count) {
        if (count == 0) return;
        if (start < 0 || start >= list.Count)
            throw new ArgumentOutOfRangeException(nameof(start), "Index is out of range.");
        if (count < 0 || start + count > list.Count)
            throw new ArgumentException("Count is out of range.");

        /// Start from the last element to be removed and move backwards
        for (int i = start + count - 1; i >= start; i--) {
            list.RemoveAt(i);
        }
    }

    /// <summary>
    /// Modifies list in place
    /// </summary>
    public static void SortByName(this List<CommandTimerViewModel> list) {
        var comparer = new NaturalStringComparer();
        list.Sort((x, y) => {
            /// First, compare by IsFavorite, with true coming first
            int favoriteCompare = y.IsFavorite.CompareTo(x.IsFavorite);
            if (favoriteCompare != 0) return favoriteCompare;

            /// If IsFavorite is the same, then compare by name naturally
            return comparer.Compare(x.Name, y.Name);
        });
    }

    /// <summary>
    /// Modifies list in place by sorting first by favorite status, then by time,
    /// and finally by name if times are equal.
    /// </summary>
    public static void SortByTime(this List<CommandTimerViewModel> list) {
        var comparer = new NaturalStringComparer();
        list.Sort((x, y) => {
            /// First, compare by IsFavorite, with true coming first
            int favoriteCompare = y.IsFavorite.CompareTo(x.IsFavorite);
            if (favoriteCompare != 0) return favoriteCompare;

            /// Compare time spans
            int timeCompare = x.TimeSpanTillExecution.CompareTo(y.TimeSpanTillExecution);
            if (timeCompare != 0) return timeCompare;

            /// If time spans are equal, then compare by name naturally
            return comparer.Compare(x.Name, y.Name);
        });
    }

    /// <summary>
    /// Modifies list in place
    /// </summary>
    public static void FilterByCondition(this List<CommandTimerViewModel> list, Func<CommandTimerViewModel, bool> condition) {
        list.RemoveAll(item => !condition.Invoke(item));
    }

    /// <summary>
    /// Modifies list in place
    /// </summary>
    public static void FuzzySearch(this List<CommandTimerViewModel> list, string searchText) {
        string[] fields = new string[3];

        list.RemoveAll(item => {
            fields[0] = item.Name;
            fields[1] = item.Description;
            fields[2] = item.Command;

            return !fields.Any(field => field?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true);
        });
    }


    public static void ForEach<T>(this IEnumerable<T> output, Action<T> action) {
        foreach (var item in output) {
            action(item);
        }
    }

    public static void ForEach<T>(this ICollection<T> output, Action<T> action) {
        foreach (var item in output) {
            action(item);
        }
    }

    public static IEnumerable<string> SortByName(this IEnumerable<string> list) {
        return list.OrderBy(static x => x, new NaturalStringComparer());
    }

    public static IEnumerable<T> SortByName<T>(this IEnumerable<T> list, Func<T, string> getString) where T : class {
        return list.OrderBy(x => getString(x), new NaturalStringComparer());
    }
}
