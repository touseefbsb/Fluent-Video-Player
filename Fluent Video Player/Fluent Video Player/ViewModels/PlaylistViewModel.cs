using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Fluent_Video_Player.Extensions;
using Fluent_Video_Player.Helpers;
using Fluent_Video_Player.Models;
using Fluent_Video_Player.Services;
using Microsoft.AppCenter.Analytics;
using Microsoft.Toolkit.Uwp.UI;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using WinUI = Microsoft.UI.Xaml.Controls;
namespace Fluent_Video_Player.ViewModels
{
    public partial class PlaylistViewModel : ObservableObject
    {
        [ObservableProperty] private string playlistTitle;
        [ObservableProperty] private bool playlistLoading;
        public InAppNotification FileNotFoundInAppNotification { get; set; }
        public ObservableCollection<IVideoFolder> SourcePrivate { get; }
        public AdvancedCollectionView Source { get; set; }
        public PlaylistViewModel()
        {
            SourcePrivate = new ObservableCollection<IVideoFolder>();
            Source = new AdvancedCollectionView(SourcePrivate, true);
        }
        public async Task FillPlaylist()
        {
            PlaylistLoading = true;
            if (SourcePrivate.Count > 0)
            {
                SourcePrivate.Clear();
            }
            var ToRemovePaths = new List<string>();
            foreach (var path in await Database.DbHelper.GetVideosForPlaylist(PlaylistTitle))
            {
                try
                {
                    var file = await StorageFile.GetFileFromPathAsync(path);
                    var vv = new Video(file);
                    SourcePrivate.Add(vv);

                    await vv.SetDurationAndThumbnailAsync(file);

                    vv.DeleteButtonEnabled = true;
                }
                catch (UnauthorizedAccessException)
                {
                    await SettingsViewModel.OnFileAccessAsync();
                }
                catch (System.IO.FileNotFoundException)
                {
                    ToRemovePaths.Add(path);
                }
            }
            PlaylistLoading = false;
            foreach (var path in ToRemovePaths)
            {
                await Database.DbHelper.RemoveFromAllPlaylists(path);
            }
            if (ToRemovePaths.Count > 0)
            {
                await FileNotFoundShow();
            }
            Analytics.TrackEvent("PlaylistViewModel.FillPlayllists()");
        }

        private async Task FileNotFoundShow()
        {
            FileNotFoundInAppNotification.Show();
            await Task.Delay(6000);
            FileNotFoundInAppNotification.Dismiss();
        }

        public ShellViewModel ShellVM { get; internal set; }

        [RelayCommand] private async Task LoadedAsync() => await FillPlaylist();

        [RelayCommand]
        private async Task DeletePlaylistAsync()
        {
            var dialog = new ContentDialog
            {
                Title = "ConfirmationTitle".GetLocalized(),
                IsPrimaryButtonEnabled = true,
                IsSecondaryButtonEnabled = true,
                PrimaryButtonText = "ContentDialogPrimaryText".GetLocalized(),
                SecondaryButtonText = "ContentDialogSecondaryText".GetLocalized(),
                Content = "ConfirmDeletePlaylistText".GetLocalized()
            };
            var result = await dialog.ShowAsync();
            switch (result)
            {
                case ContentDialogResult.Primary:
                    await Database.DbHelper.RemovePlaylist(PlaylistTitle);
                    NavigationService.Navigate<Views.HomePage>();
                    if (NavigationService.Frame.BackStack.Count > 0)//dont go back to the removed playlist page
                        NavigationService.Frame.BackStack.RemoveAt(NavigationService.Frame.BackStack.Count - 1);

                    var item = ShellVM?.NavigationView?.MenuItems.OfType<WinUI.NavigationViewItem>()
                              .First(a => (string)a.Content == PlaylistTitle && (string)a.Tag == Constants.PlaylistTag);
                    if (!(item is null))
                        ShellVM.NavigationView.MenuItems.Remove(item);
                    break;
                case ContentDialogResult.Secondary:
                    break;
                case ContentDialogResult.None:
                    break;
            }
        }

    }
}
