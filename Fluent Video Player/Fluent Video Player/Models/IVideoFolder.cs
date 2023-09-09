using Windows.UI.Xaml.Media;

namespace Fluent_Video_Player.Models
{
    /// <summary>
    /// Parent of video and folder objects
    /// </summary>
    public interface IVideoFolder
    {
        string Title { get; }
        ImageSource Thumbnail { get; set; }
    }
}
