using Windows.Media.Playback;

namespace Fluent_Video_Player.EventArgs;

public class CurrentItemChangedEventArgs : System.EventArgs
{
    public MediaPlaybackItemChangedReason Reason { get; set; }
}
