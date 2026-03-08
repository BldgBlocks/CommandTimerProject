namespace CommandTimer.Core.Interfaces;

/// <summary>
/// Service contract for accessing application theme colors.
/// All colors are resource-backed observables — subscribers can attach to ValueChanged events.
/// Reading .Value always returns the current resource value. Setting .Value updates the resource.
/// </summary>
public interface IColorProvider {

    ResourceBackedObservableProperty ApplicationBrush_Background { get; }
    ResourceBackedObservableProperty ApplicationBrush_Contrast { get; }
    ResourceBackedObservableProperty ApplicationBrush_Overlay { get; }
    ResourceBackedObservableProperty ApplicationBrush_Accent { get; }
    ResourceBackedObservableProperty ApplicationBrush_Inconspicuous { get; }
    ResourceBackedObservableProperty ApplicationBrush_Stripe { get; }
    ResourceBackedObservableProperty ApplicationBrush_Transparent { get; }
    ResourceBackedObservableProperty ApplicationBrush_DoThingIntended { get; }
    ResourceBackedObservableProperty ApplicationBrush_Bad { get; }
    ResourceBackedObservableProperty ApplicationBrush_Text { get; }
    ResourceBackedObservableProperty ApplicationBrush_Highlight { get; }
}


