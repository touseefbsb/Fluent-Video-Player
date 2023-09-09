using System;
using Fluent_Video_Player.Helpers;
using Fluent_Video_Player.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Fluent_Video_Player.DataTemplates
{
    public sealed partial class VideoFolderTemplate : UserControl
    {
        public VideoFolderTemplate() => InitializeComponent();

        public ObservableCollection<IVideoFolder> LibraryFoldersPrivate { get; set; }

        public Folder MyFolder
        {
            get { return (Folder)GetValue(MyFolderProperty); }
            set { SetValue(MyFolderProperty, value); }
        }
        public static readonly DependencyProperty MyFolderProperty =
           DependencyProperty.Register("MyFolder", typeof(Folder), typeof(VideoFolderTemplate), new PropertyMetadata(null));

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CheckView();
            SettingsStorageExtensions.OnCollectionViewSelected += SettingsStorageExtensions_OnCollectionViewSelected;
        }
        private void SettingsStorageExtensions_OnCollectionViewSelected(object sender, System.EventArgs e) => CheckView();
        private void UserControl_Unloaded(object sender, RoutedEventArgs e) => SettingsStorageExtensions.OnCollectionViewSelected -= SettingsStorageExtensions_OnCollectionViewSelected;
        private void CheckView()
        {
            switch (ApplicationData.Current.LocalSettings.ReadCurrentCollectionView())
            {
                case CurrentCollectionView.GridView:
                    TurnToGridView();
                    break;
                case CurrentCollectionView.ListView:
                    TurnToListView();
                    break;
            }
        }
        private void TurnToGridView()
        {
            GridVieww.Visibility = Visibility.Visible;
            ListVieww.Visibility = Visibility.Collapsed;
        }
        private void TurnToListView()
        {
            GridVieww.Visibility = Visibility.Collapsed;
            ListVieww.Visibility = Visibility.Visible;
        }

        private ICommand _folderRemoveCommand;
        public ICommand FolderRemoveCommand => _folderRemoveCommand ?? (_folderRemoveCommand = new RelayCommand(OnFolderRemove));

        private void OnFolderRemove() => LibraryFolderRemoved?.Invoke(this, new LibraryFolderRemovedEventArgs { MyFolder = MyFolder });
        public event EventHandler<LibraryFolderRemovedEventArgs> LibraryFolderRemoved;
    }

    public class LibraryFolderRemovedEventArgs
    {
        public Folder MyFolder { get; set; }
    }
}
