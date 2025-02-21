using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using CommandTimer.Core.ViewModels;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.ExceptionServices;

namespace CommandTimer.Desktop.Views;

public record ListViewMenuItems_BulkActions(ListViewModel ViewModel, Panel UserPromptAreaWillCover) : IMenuItemsCollection {

    private const string removeCurrentItems = "Remove Items";
    private const string resetColors = "Reset Colors";
    private const string stopAllTimers = "Stop Timers";

    private readonly IReadOnlyList<MenuItem> _Items = ImmutableArray.Create(
        new MenuItem() {
            Header = removeCurrentItems,
            Command = new RelayCommand(async () => {
                try {
                    bool? result = await ConfirmationDialog.Create()
                                                           .WithMessage($"Remove visible timers in this library?{Environment.NewLine}Are you sure?")
                                                           .WithPasswordEntry(Core.Settings.ShouldUsePasswordConfirmation.Value)
                                                           .WithBorder(Brushes.Red, new Thickness(2))
                                                           .Show(UserPromptAreaWillCover);
                    if (result is true) {
                        var timers = ViewModel.RelevantCommandTimers.ToList();
                        timers.ForEach(f => f.Library.RemoveFromLibrary(f));
                    }
                }
                catch (OperationCanceledException) { }
                catch (Exception ex) {
                    var enhancedException = new Exception($"MenuItem: {removeCurrentItems} has thrown the following.", ex);
                    ExceptionDispatchInfo.Capture(enhancedException).Throw();
                }
            })
        },
         new MenuItem() {
             Header = resetColors,
             Command = new RelayCommand(async () => {
                 try {
                     bool? result = await ConfirmationDialog.Create()
                                                            .WithMessage($"Reset item colors?{Environment.NewLine}Are you sure?")
                                                            .WithPasswordEntry(Core.Settings.ShouldUsePasswordConfirmation.Value)
                                                            .Show(UserPromptAreaWillCover);
                     if (result is true) {
                         var timers = ViewModel.RelevantCommandTimers.ToList();
                         timers.ForEach(f => f.ColorBarColor = Core.Colors.ApplicationBrush_Accent);
                     }
                 }
                 catch (OperationCanceledException) { }
                 catch (Exception ex) {
                     var enhancedException = new Exception($"MenuItem: {resetColors} has thrown the following.", ex);
                     ExceptionDispatchInfo.Capture(enhancedException).Throw();
                 }
             })
         },
        new MenuItem() {
            Header = stopAllTimers,
            Command = new RelayCommand(async () => {
                try {
                    bool? result = await ConfirmationDialog.Create()
                                                           .WithMessage($"Stop all timers?{Environment.NewLine}Are you sure?")
                                                           .WithPasswordEntry(Core.Settings.ShouldUsePasswordConfirmation.Value)
                                                           .Show(UserPromptAreaWillCover);
                    if (result is true) {
                        var timers = ViewModel.RelevantCommandTimers.ToList();
                        timers.ForEach(f => f.IsActive = false);
                    }
                }
                catch (OperationCanceledException) { }
                catch (Exception ex) {
                    var enhancedException = new Exception($"MenuItem: {stopAllTimers} has thrown the following.", ex);
                    ExceptionDispatchInfo.Capture(enhancedException).Throw();
                }
            })
        }
    );
    public IReadOnlyList<MenuItem> Items => _Items;
}
