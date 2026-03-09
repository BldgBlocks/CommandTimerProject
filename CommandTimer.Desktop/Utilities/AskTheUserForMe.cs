using CommandTimer.Desktop.Views;
using System;
using System.Threading.Tasks;

namespace CommandTimer.Desktop.Utilities;

public class AskTheUserForMe : IAskTheUser {


    public Task<bool?> Ask(string title, string message, bool includeNo = false, bool includeCancel = false) {
        if (MainWindow.Instance is not MainWindow window) return Task.FromResult<bool?>(null);

        var panel = window.Content as Panel ?? throw new InvalidOperationException("MainWindow's content is not a Panel.");
        return ConfirmationDialog.Create()
                                 .WithNoButton(includeNo)
                                 .WithCancelButton(includeCancel)
                                 .WithTitle(title)
                                 .WithMessage(message)
                                 .Show(panel);
    }
}


