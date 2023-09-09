using CommunityToolkit.Mvvm.ComponentModel;
using Windows.Storage;
using Windows.UI.Xaml.Media;

namespace Fluent_Video_Player.Models
{
    /// <summary>
    /// the folder object which will be used as folder throught the app
    /// </summary>
    public partial class Folder : ObservableObject, IVideoFolder
    {
        public string Title => MyStorageFolder?.DisplayName;
        public StorageFolder MyStorageFolder { get; set; }
        public Folder THIS => this;

        [ObservableProperty] private ImageSource thumbnail;
    }
}
