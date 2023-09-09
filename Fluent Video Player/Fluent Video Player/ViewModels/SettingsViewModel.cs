using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Fluent_Video_Player.Helpers;
using Fluent_Video_Player.Models;
using Fluent_Video_Player.Services;
using MahApps.Metro.IconPacks;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.Toolkit.Uwp.UI;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Fluent_Video_Player.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        [ObservableProperty] private bool libraryLoading;
        [ObservableProperty] private bool shortcutLoading;
        [ObservableProperty] private string versionDescription;
        [ObservableProperty] private ElementTheme elementTheme = ThemeSelectorService.Theme;
        public string VideoFoldersTitle => "VideoFoldersTitle".GetLocalized();
        public string KeyboardShortCutsTitle => "KeyboardShortCutsTitle".GetLocalized();
        public ObservableCollection<IVideoFolder> LibraryFoldersPrivate { get; }
        public AdvancedCollectionView LibraryFolders { get; set; }
        public ObservableCollection<KeyboardShortCut> KeyboardShortCutsPrivate { get; }
        public AdvancedCollectionView KeyboardShortCuts { get; set; }
        public ShellViewModel ShellVm { get; set; }
        public StorageLibrary VideosLibrary { get; private set; }
        public Visibility FeedbackLinkVisibility => Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.IsSupported() ? Visibility.Visible : Visibility.Collapsed;

        public SettingsViewModel()
        {
            LibraryFoldersPrivate = new ObservableCollection<IVideoFolder>();
            LibraryFolders = new AdvancedCollectionView(LibraryFoldersPrivate, true);

            KeyboardShortCutsPrivate = new ObservableCollection<KeyboardShortCut>();
            KeyboardShortCuts = new AdvancedCollectionView(KeyboardShortCutsPrivate, true);
        }

        [RelayCommand] private async Task LaunchFeedbackHubAsync() => await Windows.System.Launcher.LaunchUriAsync(new Uri("windows-feedback:?contextid=143"));
        [RelayCommand] private async Task DefaultAppAsync() => await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:defaultapps"));

        [RelayCommand]
        private async Task FileAccessAsync() => await OnFileAccessAsync();
        public static async Task OnFileAccessAsync()
        {
            var dialog = new ContentDialog
            {
                Title = "ConfirmationTitle".GetLocalized(),
                IsPrimaryButtonEnabled = true,
                IsSecondaryButtonEnabled = true,
                PrimaryButtonText = "ContentDialogPrimaryText".GetLocalized(),
                SecondaryButtonText = "ContentDialogSecondaryText".GetLocalized(),
                Content = "FileAccessWarningText".GetLocalized()
            };
            var result = await dialog.ShowAsync();
            switch (result)
            {
                case ContentDialogResult.Primary:
                    await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-broadfilesystemaccess"));
                    break;
                case ContentDialogResult.Secondary:
                case ContentDialogResult.None:
                    break;
            }
        }

        [RelayCommand] private async Task SwitchThemeAsync(ElementTheme theme)
        {
            ElementTheme = theme;
            await ThemeSelectorService.SetThemeAsync(theme);
        }
        [RelayCommand] private async Task RatingYesAsync() => await FileHelper.Rate();
        [RelayCommand] private async Task AddFoldersAsync() => await VideosLibrary.RequestAddFolderAsync();
        public async Task Initialize()
        {
            VersionDescription = GetVersionDescription();
            VideosLibrary = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Videos);
            VideosLibrary.DefinitionChanged += VideosLibrary_DefinitionChanged;
            FillFolders();
            FillShortCuts();
        }

        private async void VideosLibrary_DefinitionChanged(StorageLibrary sender, object args) => await DispatcherHelper.ExecuteOnUIThreadAsync(FillFolders);

        public void Dispose() => VideosLibrary.DefinitionChanged -= VideosLibrary_DefinitionChanged;

        private void FillFolders()
        {
            LibraryLoading = true;
            if (LibraryFoldersPrivate.Count > 0)
            {
                LibraryFoldersPrivate.Clear();
            }

            foreach (var folder in VideosLibrary.Folders)
            {
                LibraryFoldersPrivate.Add(new Folder { MyStorageFolder = folder });
            }
            LibraryLoading = false;
        }
        private void FillShortCuts()
        {
            ShortcutLoading = true;
            if (KeyboardShortCutsPrivate.Count > 0)
            {
                KeyboardShortCutsPrivate.Clear();
            }

            KeyboardShortCutsPrivate.Add(new KeyboardShortCut { Description = "PlayPause".GetLocalized(), ShortCut = "SpaceBar".GetLocalized(), Kind = PackIconMaterialDesignKind.SpaceBar });
            KeyboardShortCutsPrivate.Add(new KeyboardShortCut { Description = "MuteUnMute".GetLocalized(), ShortCut = "M".GetLocalized(), Kind = PackIconMaterialDesignKind.VolumeMute });
            KeyboardShortCutsPrivate.Add(new KeyboardShortCut { Description = "FullScreen".GetLocalized(), ShortCut = "F/Esc".GetLocalized(), Kind = PackIconMaterialDesignKind.Fullscreen });
            KeyboardShortCutsPrivate.Add(new KeyboardShortCut { Description = "VolumeUp".GetLocalized(), ShortCut = "Up".GetLocalized(), Kind = PackIconMaterialDesignKind.VolumeUp });
            KeyboardShortCutsPrivate.Add(new KeyboardShortCut { Description = "VolumeDown".GetLocalized(), ShortCut = "Down".GetLocalized(), Kind = PackIconMaterialDesignKind.VolumeDown });
            KeyboardShortCutsPrivate.Add(new KeyboardShortCut { Description = "Seek5Forward".GetLocalized(), ShortCut = "Right".GetLocalized(), Kind = PackIconMaterialDesignKind.Forward5 });
            KeyboardShortCutsPrivate.Add(new KeyboardShortCut { Description = "Seek5Backward".GetLocalized(), ShortCut = "Left".GetLocalized(), Kind = PackIconMaterialDesignKind.Replay5 });
            KeyboardShortCutsPrivate.Add(new KeyboardShortCut { Description = "Seek30Forward".GetLocalized(), ShortCut = "Right30".GetLocalized(), Kind = PackIconMaterialDesignKind.Forward30 });
            KeyboardShortCutsPrivate.Add(new KeyboardShortCut { Description = "Seek30Backward".GetLocalized(), ShortCut = "Left30".GetLocalized(), Kind = PackIconMaterialDesignKind.Replay30 });
            KeyboardShortCutsPrivate.Add(new KeyboardShortCut { Description = "Seek60Forward".GetLocalized(), ShortCut = "Right60".GetLocalized(), Kind = PackIconMaterialDesignKind.RotateRight });
            KeyboardShortCutsPrivate.Add(new KeyboardShortCut { Description = "Seek60Backward".GetLocalized(), ShortCut = "Left60".GetLocalized(), Kind = PackIconMaterialDesignKind.RotateLeft });
            KeyboardShortCutsPrivate.Add(new KeyboardShortCut { Description = "NextTrack".GetLocalized(), ShortCut = "D".GetLocalized(), Kind = PackIconMaterialDesignKind.SkipNext });
            KeyboardShortCutsPrivate.Add(new KeyboardShortCut { Description = "PreviousTrack".GetLocalized(), ShortCut = "A".GetLocalized(), Kind = PackIconMaterialDesignKind.SkipPrevious });
            KeyboardShortCutsPrivate.Add(new KeyboardShortCut { Description = "ToggleLooping".GetLocalized(), ShortCut = "L".GetLocalized(), Kind = PackIconMaterialDesignKind.Loop });
            KeyboardShortCutsPrivate.Add(new KeyboardShortCut { Description = "TogglePipMode".GetLocalized(), ShortCut = "P".GetLocalized(), Kind = PackIconMaterialDesignKind.PictureInPicture });
            KeyboardShortCutsPrivate.Add(new KeyboardShortCut { Description = "ToggleMiniBar".GetLocalized(), ShortCut = "B".GetLocalized(), Kind = PackIconMaterialDesignKind.BorderBottom });
            ShortcutLoading = false;
        }
        private string GetVersionDescription()
        {
            var appName = "AppDisplayName".GetLocalized();
            var package = Package.Current;
            var packageId = package.Id;
            var version = packageId.Version;

            return $"{appName} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }

        [RelayCommand]
        private void Shared()
        {
            DataTransferManager.GetForCurrentView().DataRequested += SharingDataRequested;
            DataTransferManager.ShowShareUI();
        }
        private void SharingDataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            args.Request.Data.Properties.Title = Constants.AppName;
            args.Request.Data.Properties.Description = Constants.AppStoreLink;
            args.Request.Data.SetText(Constants.AppStoreLink + @" (https://www.microsoft.com/store/apps/" + Constants.AppStoreId + ")");
            args.Request.Data.SetWebLink(new Uri("https://www.microsoft.com/store/apps/" + Constants.AppStoreId));
            DataTransferManager.GetForCurrentView().DataRequested -= SharingDataRequested;
        }
    }
}
