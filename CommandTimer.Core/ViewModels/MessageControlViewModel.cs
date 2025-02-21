using Avalonia;
using Avalonia.Media;
using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CommandTimer.Core.ViewModels;

public partial class MessageControlViewModel : ViewModelBase {

    [JsonIgnore]
    public int lifetimeRemaining;

    private event Func<Task> Remove = async delegate { await Task.Yield(); };
    
    //...


    public MessageControlViewModel() {
        lifetimeRemaining = Lifetime;
    }

    public void Subscribe(Func<Task> remove) => Remove += remove;
    public void Unsubscribe(Func<Task> remove) => Remove -= remove;
    public Task OnRemove() => Remove();

    //...


    [JsonIgnore]
    private string _Message = "This is a test.";
    [JsonIgnore]
    public string Message {
        get => _Message; 
        set => SetProperty(ref _Message, value, Save.No, Notify.Yes);
    }


    [JsonIgnore]
    private int _Priority = 0;
    [JsonIgnore]
    public int Priority { get => _Priority; set => SetProperty(ref _Priority, value, Save.No, Notify.Yes); }


    [JsonIgnore]
    private int _Lifetime = 5;
    [JsonIgnore]
    public int Lifetime { get => _Lifetime; set => SetProperty(ref _Lifetime, value, Save.No, Notify.Yes); }


    [JsonIgnore]
    private bool _Transient = true;
    [JsonIgnore]
    public bool Transient { get => _Transient; set => SetProperty(ref _Transient, value, Save.No, Notify.Yes); }


    //...


    [JsonInclude]
    private IBrush _Background = Core.Colors.ApplicationBrush_Overlay;
    [JsonIgnore]
    public IBrush Background { get => _Background; set => SetProperty(ref _Background, value, Save.No, Notify.Yes); }

    [JsonInclude]
    private IBrush _Foreground = Core.Colors.ApplicationBrush_Text;
    [JsonIgnore]
    public IBrush Foreground { get => _Foreground; set => SetProperty(ref _Foreground, value, Save.No, Notify.Yes); }

}
