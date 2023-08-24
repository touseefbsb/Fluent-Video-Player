using Fluent_Video_Player.Models;

namespace Fluent_Video_Player.EventArgs;

public class VideoRemovedFromPlaylistEventArgs : System.EventArgs
{
    public Video MyVideo { get; set; }
}
