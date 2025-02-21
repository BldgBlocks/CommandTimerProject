using CommandTimer.Desktop.Views;
using CommandTimer.Core.Utilities;
using System.Threading.Tasks;

namespace CommandTimer.Desktop.Utilities;
public class AskTheUserForMe : IAskTheUser {


    public Task<bool?> Ask(string title, string message, bool includeNo = false, bool includeCancel = false) {
        if (MainWindow.Instance is not MainWindow window) return Task.FromResult<bool?>(null);

        return ConfirmationDialog.Create()
                                 .WithNoButton(includeNo)
                                 .WithCancelButton(includeCancel)
                                 .WithTitle(title)
                                 .WithMessage(message)
                                 .Show(window.MainWindowLayout);
    }
}
