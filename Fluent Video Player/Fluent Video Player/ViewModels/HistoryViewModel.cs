using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.UI;
using CommunityToolkit.WinUI.UI.Controls;
using Fluent_Video_Player.Contracts.Services;
using Fluent_Video_Player.Enums;
using Fluent_Video_Player.EventArgs;
using Fluent_Video_Player.Helpers;
using Fluent_Video_Player.Models;
using Microsoft.AppCenter.Analytics;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.AccessCache;
using Windows.Storage.FileProperties;

namespace Fluent_Video_Player.ViewModels;

public partial class HistoryViewModel : ObservableRecipient
{
    #region Services
    private readonly IStorageFileService _storageFileService;
    #endregion Services

    #region ObservableFields
    [ObservableProperty] private bool _historyLoading;
    [ObservableProperty] private string _historyTitle = "History".GetLocalized();
    #endregion ObservableFields

    #region Props
    public ObservableCollection<IVideoFolder> SourcePrivate { get; }
    public AdvancedCollectionView Source { get; set; }
    public InAppNotification FileNotFoundInAppNotification { get; set; }
    #endregion Props

    #region Ctor
    public HistoryViewModel(IStorageFileService storageFileService)
    {
        _storageFileService = storageFileService;
        SourcePrivate = new ObservableCollection<IVideoFolder>();
        Source = new AdvancedCollectionView(SourcePrivate, true);
    }
    #endregion Ctor

    #region Commands
    [RelayCommand] private async Task LoadedAsync() => await FillHistoryAsync();

    [RelayCommand]
    private async Task ClearHistoryAsync()
    {
        var dialog = new ContentDialog
        {
            Title = "ConfirmationTitle".GetLocalized(),
            IsPrimaryButtonEnabled = true,
            IsSecondaryButtonEnabled = true,
            PrimaryButtonText = "ContentDialogPrimaryText".GetLocalized(),
            SecondaryButtonText = "ContentDialogSecondaryText".GetLocalized(),
            Content = "ConfirmClearHistoryText".GetLocalized(),
        };
        var result = await dialog.ShowAsync();
        switch (result)
        {
            case ContentDialogResult.Primary:
                StorageApplicationPermissions.FutureAccessList.Clear();
                SourcePrivate.Clear();
                break;
            case ContentDialogResult.Secondary:
            case ContentDialogResult.None:
                break;
        }
    }
    #endregion Commands

    #region Methods
    public async Task FillHistoryAsync()
    {
        HistoryLoading = true;
        if (SourcePrivate.Count > 0)
        {
            SourcePrivate.Clear();
        }
        var ToRemoveTokens = new List<string>();
        foreach (var token in StorageApplicationPermissions.FutureAccessList.Entries.Select(x => x.Token))
        {
            try
            {
                var file = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(token);
                var video = new Video(file);
                SourcePrivate.Add(video);

                video.HistoryToken = token;
                video.Duration = await _storageFileService.GetDurationAsync(file);

                //set asyncronosly thumbnail
                BitmapImage? bitm = null;
                using (var imgSource = await video.MyVideoFile.GetScaledImageAsThumbnailAsync(ThumbnailMode.VideosView, Constants._thumbnailReqestedSize, ThumbnailOptions.UseCurrentScale))
                {
                    if (imgSource is not null)
                    {
                        bitm = new BitmapImage();
                        await bitm.SetSourceAsync(imgSource);
                    }
                }
                if (bitm is not null)
                {
                    video.Thumbnail = bitm;
                }
            }
            catch (FileNotFoundException)
            {
                ToRemoveTokens.Add(token);
            }
        }
        HistoryLoading = false;
        foreach (var item in ToRemoveTokens)
        {
            StorageApplicationPermissions.FutureAccessList.Remove(item);
        }
        if (ToRemoveTokens.Count > 0)
        {
            FileNotFoundInAppNotification.Show();
            await Task.Delay(6000);
            FileNotFoundInAppNotification.Dismiss();
        }
        Analytics.TrackEvent(nameof(FillHistoryAsync));
    }

    internal void ApplyFilter(string searchBoxText)
    {
        Source.Filter = x => true;
        Source.Filter = string.IsNullOrWhiteSpace(searchBoxText)
            ? (_ => true)
            : (x =>
            ((Video)x).Title.Contains(searchBoxText, StringComparison.OrdinalIgnoreCase));
    }

    internal void HistoryItemDeleted(HistoryItemDeletedEventArgs e)
    {
        StorageApplicationPermissions.FutureAccessList.Remove(e.MyVideo.HistoryToken);
        SourcePrivate.Remove(e.MyVideo);
    }
    #endregion Methods
}
