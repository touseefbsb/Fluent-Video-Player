using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Fluent_Video_Player.Extensions;
using Fluent_Video_Player.Helpers;
using Fluent_Video_Player.Models;
using Windows.Storage;

namespace Fluent_Video_Player.ViewModels
{
    public partial class HomeViewModel : HomeHistoryViewModel
    {
        [ObservableProperty] private bool libraryLoading;
        public ObservableCollection<Video> LibrarySource { get; } = new();
        private async Task FillUpLibrary()
        {
            LibraryLoading = true;
            if (LibrarySource.Count > 0)
            {
                LibrarySource.Clear();
            }

            uint index = 0;
            var VideoQuery = FileHelper.GetVideoFilesQuery(KnownFolders.VideosLibrary, Constants._thumbnailReqestedSize);
            IReadOnlyList<StorageFile> files = await VideoQuery.GetFilesAsync(index, Constants._stepSize);
            index += Constants._stepSize;
            while (files.Count != 0)
            {
                var fileTask = VideoQuery.GetFilesAsync(index, Constants._stepSize).AsTask();
                foreach (StorageFile file in files)
                {
                    var vv = new Video(file);
                    LibrarySource.Add(vv);

                    await vv.SetDurationAndThumbnailAsync(file);
                }
                if (LibrarySource.Count >= Constants.TrainViewMaxItems)
                {
                    break;
                }

                files = await fileTask;
                index += Constants._stepSize;
            }
            LibraryLoading = false;
        }
        [RelayCommand] private async Task LoadedAsync() => await Task.WhenAll(FillUpLibrary(), FillHistoryAsync(Constants.TrainViewMaxItems));
    }
}
