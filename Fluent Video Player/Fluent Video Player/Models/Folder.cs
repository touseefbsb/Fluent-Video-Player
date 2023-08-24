using CommunityToolkit.Mvvm.ComponentModel;
using Fluent_Video_Player.Enums;
using Microsoft.UI.Xaml.Media;
using Windows.Storage;

namespace Fluent_Video_Player.Models
{
    /// <summary>
    /// the folder object which will be used as folder throught the app
    /// </summary>
    public partial class Folder : ObservableRecipient, IVideoFolder
    {
        #region ObservableFields
        [ObservableProperty] private ImageSource _thumbnail;
        #endregion ObservableFields

        #region Props
        public string Title => MyStorageFolder.DisplayName;
        public StorageFolder MyStorageFolder { get; set; }
        public Folder THIS => this;
        #endregion Props
    }
}
