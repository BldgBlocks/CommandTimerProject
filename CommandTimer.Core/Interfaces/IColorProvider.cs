namespace CommandTimer.Core.Interfaces;

/// <summary>
/// Service contract for accessing application theme colors.
/// All colors are resource-backed observables — subscribers can attach to ValueChanged events.
/// Reading .Value always returns the current resource value. Setting .Value updates the resource.
/// </summary>
public interface IColorProvider {

    ObservableProperty<AppColor> ApplicationBrush_Background { get; }
    ObservableProperty<AppColor> ApplicationBrush_Contrast { get; }
    ObservableProperty<AppColor> ApplicationBrush_Overlay { get; }
    ObservableProperty<AppColor> ApplicationBrush_Accent { get; }
    ObservableProperty<AppColor> ApplicationBrush_Inconspicuous { get; }
    ObservableProperty<AppColor> ApplicationBrush_Stripe { get; }
    ObservableProperty<AppColor> ApplicationBrush_Transparent { get; }
    ObservableProperty<AppColor> ApplicationBrush_DoThingIntended { get; }
    ObservableProperty<AppColor> ApplicationBrush_Bad { get; }
    ObservableProperty<AppColor> ApplicationBrush_Text { get; }
    ObservableProperty<AppColor> ApplicationBrush_Highlight { get; }
}


