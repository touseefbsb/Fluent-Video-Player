namespace Fluent_Video_Player.EventArgs;

public class CurrentPlaybackListItemChangedEventArgs : System.EventArgs
{
    public bool PreviousButtonEnabled { get; set; }
    public bool NextButtonEnabled { get; set; }
}
