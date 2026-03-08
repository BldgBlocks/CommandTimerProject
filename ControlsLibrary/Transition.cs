using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Data;
using System;

namespace ControlsLibrary;

/// <summary>
/// Builder pattern extensions for Avalonia transitions.
/// Simplifies creating, binding, and managing transitions between view controls and DataContext properties.
/// </summary>
public static class Transition {

    /// <summary>
    /// Create a transition of the specified type with optional duration and easing.
    /// </summary>
    public static T CreateAnimation<T>(int milliseconds = 300, Easing? easing = default) where T : TransitionBase, new() {
        return new T() {
            Duration = TimeSpan.FromMilliseconds(milliseconds),
            Easing = easing ?? new QuadraticEaseInOut()
        };
    }

    /// <summary>
    /// Set the target property for the transition.
    /// </summary>
    public static T WithProperty<T>(this T transition, AvaloniaProperty property) where T : TransitionBase {
        transition.Property = property;
        return transition;
    }

    /// <summary>
    /// Set the duration of the transition.
    /// </summary>
    public static T WithTime<T>(this T transition, int milliseconds) where T : TransitionBase {
        transition.Duration = TimeSpan.FromMilliseconds(milliseconds);
        return transition;
    }

    /// <summary>
    /// Set the easing function for the transition.
    /// </summary>
    public static T WithEasing<T>(this T transition, Easing easing) where T : TransitionBase {
        transition.Easing = easing;
        return transition;
    }

    /// <summary>
    /// Adds the transition to the control and creates a binding between the control's property and the DataContext property.
    /// </summary>
    /// <param name="transition">The transition to bind.</param>
    /// <param name="control">The control with the property to bind to a DataContext ViewModel.</param>
    /// <param name="nameOfDataContextProperty">The name of the ViewModel property to bind to.</param>
    /// <param name="token">The binding token for later disposal.</param>
    /// <exception cref="ArgumentNullException">Requires a valid DataContext and a property set on the transition.</exception>
    public static T WithBind<T>(this T transition, Control control, string nameOfDataContextProperty, out BindingExpressionBase token) where T : TransitionBase {
        if (control.DataContext is null) {
            throw new ArgumentNullException(nameof(control), "The provided control does not have a DataContext set.");
        }
        if (transition.Property is null) {
            throw new ArgumentNullException(nameof(transition), "A property has not been provided to the builder to perform the binding.");
        }

        if (control.Transitions is null) {
            control.Transitions = [transition];
        }
        else {
            control.Transitions.Add(transition);
        }

        token = control.Bind(transition.Property, new Binding() {
            Source = control.DataContext,
            Path = nameOfDataContextProperty
        });

        return transition;
    }

    /// <summary>
    /// Remove this transition from the control's list of transitions.
    /// </summary>
    public static void Remove<T>(this T transition, Control control) where T : TransitionBase {
        if (control.Transitions is not null && control.Transitions.Contains(transition)) {
            control.Transitions.Remove(transition);
        }
    }

    /// <summary>
    /// Dispose of the binding through the token.
    /// </summary>
    public static void Unbind<T>(this T transition, BindingExpressionBase token) where T : TransitionBase {
        token.Dispose();
    }
}

