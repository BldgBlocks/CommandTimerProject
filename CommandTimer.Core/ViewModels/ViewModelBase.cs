// Ignore Spelling: Serializable

using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace CommandTimer.Core.ViewModels;

public class ViewModelBase : ObservableObject {


    // TODO: Should this really be static or per class?
    protected static bool ShouldSerialize { get; set; } = true;
    protected enum Notify { Yes, No }
    protected enum Save { Yes, No }

    protected bool SetProperty<T>([NotNullIfNotNull(nameof(newValue))] ref T field, T newValue, Save shouldSerialize = Save.No, Notify shouldNotify = Notify.Yes, [CallerMemberName] string? propertyName = null) {
        if (EqualityComparer<T>.Default.Equals(field, newValue)) {
            return false;
        }

        if (shouldNotify is Notify.Yes) {
            SetProperty(ref field, newValue, propertyName);
        }
        else {
            field = newValue;
        }

        if (shouldSerialize is Save.Yes && ShouldSerialize) {
            Serialize();
        }

        return true;
    }

    //... Callbacks

    /// <summary>
    /// No default implementation. No need to call base.
    /// </summary>
    public virtual void Serialize() {
        // No default implementation.
    }

    /// <summary>
    /// No default implementation. No need to call base.
    /// </summary>
    public virtual void Unserialize() {
        // No default implementation.
    }

}
