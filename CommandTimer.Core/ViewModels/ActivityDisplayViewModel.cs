using CommandTimer.Core.Utilities;
using System.Text.Json.Serialization;

namespace CommandTimer.Core.ViewModels {

    public partial class ActivityDisplayViewModel : ViewModelBase {

        public ActivityDisplayViewModel() {
            /// Global Action
            Core.ActionRelay.ActionPosted += (o, a) => {
                if (a.ActionKey == Core.Settings.Keys.ActionRelay_Serialization) {
                    Serialize();
                }
            };
        }


        public override void Serialize()
            => ServiceProvider.Get<ISerializer>().Serialize(Core.Settings.Keys.ActivityView, this, Core.Settings.DEFAULT_DATA_FILE);

        [JsonIgnore]
        private bool _ActivityText;
        [JsonIgnore]
        public bool ActivityText { 
            get => _ActivityText; 
            set => SetProperty(ref _ActivityText, value, Save.Yes, Notify.Yes);
        }
    }
}
