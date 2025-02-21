﻿using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Data;
using System;

namespace ControlsLibrary;

public static class DoubleTransitionHelper {

    public static DoubleTransition CreateAnimation(int milliseconds = 300, Easing? easing = default) {
        var builder = new DoubleTransition() {
            Duration = new TimeSpan(0, 0, 0, 0, milliseconds),
            Easing = easing ?? new QuadraticEaseInOut()
        };
        return builder;
    }

    public static DoubleTransition WithProperty(this DoubleTransition transition, StyledProperty<double> viewControlProperty) {
        transition.Property = viewControlProperty;
        return transition;
    }

    public static DoubleTransition WithTime(this DoubleTransition transition, int milliseconds) {
        transition.Duration = new TimeSpan(0, 0, 0, 0, milliseconds);
        return transition;
    }

    public static DoubleTransition WithEasing(this DoubleTransition transition, Easing easing) {
        transition.Easing = easing;
        return transition;
    }

    /// <summary>
    /// Builder pattern to simplify creating animations. Handles setting the transition and performing the binding between xaml control and DataContext.
    /// </summary>
    /// <param name="transition"></param>
    /// <param name="control">The control with the property to bind to a DataContext ViewModel.</param>
    /// <exception cref="ArgumentNullException">Requires a valid DataContext in the control and valid builder properties set.</exception>
    public static DoubleTransition WithBind(this DoubleTransition transition, Control control, string nameOfDataContextProperty, out BindingExpressionBase token) {
        if (control.DataContext is null) {
            throw new ArgumentNullException("The provided control does not have a DataContext set.");
        }
        if (transition.Property is null) {
            throw new ArgumentNullException("A property has not been provided to the builder to perform the binding.");
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
    public static void Remove(this DoubleTransition transition, Control control) {
        if (control.DataContext is null) {
            throw new ArgumentNullException("The provided control does not have a DataContext set.");
        }

        if (control.Transitions is not null && control.Transitions.Contains(transition)) {
            control.Transitions.Remove(transition);
        }
    }

    /// <summary>
    /// Dispose of the binding through the token.
    /// </summary>
    public static void Unbind(this DoubleTransition transition, BindingExpressionBase token) {
        token.Dispose();
    }
}
