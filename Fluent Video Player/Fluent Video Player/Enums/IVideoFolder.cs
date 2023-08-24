using Microsoft.UI.Xaml.Media;

namespace Fluent_Video_Player.Enums
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
