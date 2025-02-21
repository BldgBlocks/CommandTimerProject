using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace CommandTimer.Core.ViewModels;

public partial class MessageViewerViewModel : ViewModelBase {

    public MessageViewerViewModel() {

    }

    [JsonIgnore]
    public ObservableCollection<MessageControlViewModel> Messages { get; } = [];

}
