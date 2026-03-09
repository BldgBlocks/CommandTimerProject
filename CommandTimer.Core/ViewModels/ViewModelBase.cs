// Ignore Spelling: Serializable

using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace CommandTimer.Core.ViewModels;

public class ViewModelBase : ObservableObject, IJsonOnSerializing, IJsonOnSerialized, IJsonOnDeserializing, IJsonOnDeserialized {


    protected bool ShouldSerialize { get; set; } = true;
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
    /// Called by the JSON serialization pipeline before writing. Overrides should call base to preserve lifecycle broadcasts.
    /// </summary>
    protected virtual void PreSerialize() => ActionRelay.OnActionPosted(this, Settings.Keys.ActionRelay_PreSerialize);

    /// <summary>
    /// Called by the JSON serialization pipeline after writing. Overrides should call base to preserve lifecycle broadcasts.
    /// </summary>
    protected virtual void PostSerialize() => ActionRelay.OnActionPosted(this, Settings.Keys.ActionRelay_PostSerialize);

    /// <summary>
    /// Called by the JSON serialization pipeline before reading. Overrides should call base to preserve serialization suppression and lifecycle broadcasts.
    /// </summary>
    protected virtual void PreDeserialize() {
        ShouldSerialize = false;
        ActionRelay.OnActionPosted(this, Settings.Keys.ActionRelay_PreDeserialize);
    }

    /// <summary>
    /// Called by the JSON serialization pipeline after reading. Overrides should call base to restore serialization and preserve lifecycle broadcasts.
    /// </summary>
    protected virtual void PostDeserialize() {
        ActionRelay.OnActionPosted(this, Settings.Keys.ActionRelay_PostDeserialize);
        ShouldSerialize = true;
    }

    void IJsonOnSerializing.OnSerializing() => PreSerialize();

    void IJsonOnSerialized.OnSerialized() => PostSerialize();

    void IJsonOnDeserializing.OnDeserializing() => PreDeserialize();

    void IJsonOnDeserialized.OnDeserialized() => PostDeserialize();

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

