using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Fluent_Video_Player.Models;
using Microsoft.AppCenter.Analytics;
using Microsoft.Toolkit.Uwp.UI;
using Windows.Storage;
using Windows.Storage.Search;
using Fluent_Video_Player.Helpers;
using static Fluent_Video_Player.Helpers.Constants;
using Microsoft.Toolkit.Uwp.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using Fluent_Video_Player.Extensions;
using CommunityToolkit.Mvvm.Input;

namespace Fluent_Video_Player.ViewModels
{
    public partial class LibraryViewModel : ObservableObject
    {
        [ObservableProperty] private StorageFolder mainFolder;
        [ObservableProperty] private bool libraryLoading;
        public StorageLibrary VideosLibrary { get; private set; }
        public List<FolderFilterItem> FolderFilterItems { get; private set; }
        public ObservableCollection<IVideoFolder> SourcePrivate { get; }
        public AdvancedCollectionView Source { get; set; }

        public LibraryViewModel()
        {
            SourcePrivate = new ObservableCollection<IVideoFolder>();
            Source = new AdvancedCollectionView(SourcePrivate, true);
            FolderFilterItems = new List<FolderFilterItem>
            {
                new FolderFilterItem(MahApps.Metro.IconPacks.PackIconMaterialDesignKind.VideoLibrary, "ShowAll".GetLocalized()),
                new FolderFilterItem(MahApps.Metro.IconPacks.PackIconMaterialDesignKind.Folder,  "ShowFolder".GetLocalized()),
                new FolderFilterItem(MahApps.Metro.IconPacks.PackIconMaterialDesignKind.OndemandVideo, "ShowVideo".GetLocalized())
            };
        }

        [RelayCommand] private async Task AddFoldersAsync() => await VideosLibrary.RequestAddFolderAsync();

        public void Initialize(StorageFolder mainFolder) => MainFolder = mainFolder;

        [RelayCommand]
        private async Task LoadedAsync()
        {
            VideosLibrary = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Videos);
            VideosLibrary.DefinitionChanged += VideosLibrary_DefinitionChanged;
            if (MainFolder is null)
            {
                await Fill(KnownFolders.VideosLibrary);
            }
            else
            {
                await Fill(MainFolder);
            }
        }
        public async Task Fill(StorageFolder mainfolder)
        {
            LibraryLoading = true;
            if (SourcePrivate.Count > 0)
            {
                SourcePrivate.Clear();
            }

            MainFolder = mainfolder;
            if (MainFolder.DisplayType == "Library")
            {
                foreach (StorageFolder folder in VideosLibrary.Folders)
                {
                    var vv = new Folder { MyStorageFolder = folder };
                    SourcePrivate.Add(vv);
                }
            }
            else
            {
                IndexedState folderIndexedState = await MainFolder.GetIndexedStateAsync();
                if (folderIndexedState == IndexedState.NotIndexed || folderIndexedState == IndexedState.Unknown)
                {
                    //Only possible in indexed directories.                
                }
                else
                {
                    //this method only works for indexed stuff ( known folders only )
                    //For More Details visit https://blogs.msdn.microsoft.com/adamdwilson/2017/12/20/fast-file-enumeration-with-partially-initialized-storagefiles/

                    await FillUpFolders();
                    await FillUpFiles();
                }
            }
            LibraryLoading = false;
            Analytics.TrackEvent("LibraryViewModel.Fill()");
        }

        private async Task FillUpFiles()
        {
            uint index = 0;
            var VideoQuery = FileHelper.GetVideoFilesQuery(MainFolder, _thumbnailReqestedSize);
            var files = await VideoQuery.GetFilesAsync(index, _stepSize);
            index += _stepSize;
            while (files.Count != 0)
            {
                var fileTask = VideoQuery.GetFilesAsync(index, _stepSize).AsTask();
                foreach (StorageFile file in files)
                {
                    var vv = new Video(file);
                    SourcePrivate.Add(vv);

                    await vv.SetDurationAndThumbnailAsync(file);
                }
                files = await fileTask;
                index += _stepSize;
            }
        }
        private async Task FillUpFolders()
        {
            uint index = 0;
            var VideoQuery = FileHelper.GetVideoFoldersQuery(MainFolder);
            IReadOnlyList<StorageFolder> folders = await VideoQuery.GetFoldersAsync(index, _stepSize);
            index += _stepSize;
            while (folders.Count != 0)
            {
                var folderTask = VideoQuery.GetFoldersAsync(index, _stepSize).AsTask();
                foreach (StorageFolder folder in folders)
                {
                    var vv = new Folder { MyStorageFolder = folder };
                    SourcePrivate.Add(vv);
                }
                folders = await folderTask;
                index += _stepSize;
            }
        }
        private async void VideosLibrary_DefinitionChanged(StorageLibrary sender, object args)
        {
            await DispatcherHelper.ExecuteOnUIThreadAsync(async () =>
            {
                if (MainFolder.DisplayType == "Library")
                {
                    await Fill(KnownFolders.VideosLibrary);
                }
            });
        }

        public void Dispose() => VideosLibrary.DefinitionChanged -= VideosLibrary_DefinitionChanged;
    }
}
