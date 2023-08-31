using Windows.Media.Playback;
using Windows.Storage;

namespace Fluent_Video_Player.Models
{
    public class FluentPlaybackItem
    {
        public FluentPlaybackItem(Video myVideo) => MyVideo = myVideo;
        public Video MyVideo { get; set; }
        public MediaPlaybackItem? Item { get; set; }
        public StorageFile? VideoFile => MyVideo?.MyVideoFile;
        public MediaItemDisplayProperties? GetDisplayProperties() => Item?.GetDisplayProperties();
        public FluentPlaybackItem THIS => this;

        #region Methods
        public void DisposeMediaPlaybackItem()
        {
            Item?.Source.Dispose();
            Item = default;
        }
        #endregion Methods
    }
}
