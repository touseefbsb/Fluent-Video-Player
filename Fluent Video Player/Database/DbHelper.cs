using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Fluent_Video_Player.Database
{
    public static class DbHelper
    {
        public static void Initialize()
        {
            using (var MyDb = new DatabaseContext())
            {
                MyDb.Database.Migrate();
            }
        }

        public static async Task<int> GetMyViews(string folderRelativeId)
        {
            using (var MyDb = new DatabaseContext())
            {
                var fileDataItem = await MyDb?.FileDatas?.FirstOrDefaultAsync(a => a.Path == folderRelativeId);
                return fileDataItem?.Views ?? 0;
            }
        }

        public static async Task UpdateViews(string folderRelativeId)
        {
            using (var MyDb = new DatabaseContext())
            {
                if (MyDb.FileDatas.Count() == 0)
                {
                    await MyDb.FileDatas.AddAsync(new FileData { Path = folderRelativeId, Views = 1 });
                    await MyDb.SaveChangesAsync();
                    return;
                }
                var viewItem = await MyDb?.FileDatas?.FirstOrDefaultAsync(a => a.Path == folderRelativeId);

                if (viewItem == null)
                {
                    await MyDb?.FileDatas?.AddAsync(new FileData { Path = folderRelativeId, Views = 1 });
                    await MyDb.SaveChangesAsync();
                    return;
                }
                MyDb.FileDatas.Update(viewItem);
                viewItem.Views++;
                await MyDb.SaveChangesAsync();
            }
        }

        public static TimeSpan LastRetrievedPosition = TimeSpan.Zero;

        public static async Task<TimeSpan> GetPosition(string folderRelativeId)
        {
            using (var MyDb = new DatabaseContext())
            {
                if (MyDb.FileDatas.Count() == 0)
                {
                    LastRetrievedPosition = TimeSpan.Zero;
                    return LastRetrievedPosition;
                }

                var PositionItem = await MyDb.FileDatas.FirstOrDefaultAsync(a => a.Path == folderRelativeId);

                if (PositionItem is null)
                {
                    LastRetrievedPosition = TimeSpan.Zero;
                    return LastRetrievedPosition;
                }

                LastRetrievedPosition = TimeSpan.FromTicks(Convert.ToInt64(PositionItem.Position));
                return LastRetrievedPosition;
            }
        }
        public static async Task<List<string>> GetPlaylists()
        {
            using (var MyDb = new DatabaseContext())
            {
                return await MyDb.PlaylistDatas.Select(a => a.Playlist).ToListAsync();
            }
        }
        public static async Task<bool> IsPlaylistNameNew(string name)
        {
            using (var MyDb = new DatabaseContext())
            {
                return !(await MyDb.PlaylistDatas.Select(a => a.Playlist).ToListAsync()).Contains(name);
            }
        }
        public static async Task RemovePlaylist(string playlistName)
        {
            using (var MyDb = new DatabaseContext())
            {
                var playlistToRemove = await MyDb.PlaylistDatas.FirstOrDefaultAsync(a => a.Playlist == playlistName);

                if (!(playlistToRemove is null))
                {
                    MyDb.PlaylistDatas.Remove(playlistToRemove);
                }

                MyDb.PlaylistFileDatas.RemoveRange(MyDb.PlaylistFileDatas.Where(a => a.Playlist == playlistName));

                await MyDb.SaveChangesAsync();
            }
        }
        public static async Task<List<string>> GetVideosForPlaylist(string playlistName)
        {
            using (var MyDb = new DatabaseContext())
            {
                return await MyDb.PlaylistFileDatas.Where(a => a.Playlist == playlistName).Select(a => a.Path).ToListAsync();
            }
        }
        public static async Task<List<string>> GetPlaylistsForVideo(string path)
        {
            using (var MyDb = new DatabaseContext())
            {
                return await MyDb.PlaylistFileDatas.Where(a => a.Path == path).Select(a => a.Playlist).ToListAsync();
            }
        }

        public static async Task CreateNewPlaylist(string playlistName, string path)
        {
            using (var MyDb = new DatabaseContext())
            {
                //if (MyDb.PlaylistDatas.FirstOrDefaultAsync(a => a.Playlist == playlistName) is null)
                //{//check for already existing if the button is activated without checking
                MyDb.PlaylistDatas.Add(new PlaylistData { Playlist = playlistName });
                MyDb.PlaylistFileDatas.Add(new PlaylistFileData { Playlist = playlistName, Path = path });
                await MyDb.SaveChangesAsync();
                //    return true;
                //}
                //else
                //    return false;
            }
        }
        public static async Task AddToPlaylist(string playlistName, string path)
        {
            using (var MyDb = new DatabaseContext())
            {
                var item = await MyDb.PlaylistFileDatas.FirstOrDefaultAsync(a => a.Playlist == playlistName && a.Path == path);
                if (item is null)
                {
                    MyDb.PlaylistFileDatas.Add(new PlaylistFileData { Playlist = playlistName, Path = path });
                    await MyDb.SaveChangesAsync();
                }
            }
        }
        public static async Task RemoveFromPlaylist(string playlistName, string path)
        {
            using (var MyDb = new DatabaseContext())
            {
                var itemToRemove = await MyDb.PlaylistFileDatas.FirstOrDefaultAsync(a => a.Playlist == playlistName && a.Path == path);
                if (!(itemToRemove is null))
                {
                    MyDb.PlaylistFileDatas.Remove(itemToRemove);
                    await MyDb.SaveChangesAsync();
                }
            }
        }

        public static async Task RemoveFromAllPlaylists(string path)
        {
            using (var MyDb = new DatabaseContext())
            {
                var itemsToRemove = MyDb.PlaylistFileDatas.Where(a => a.Path == path);
                if (!(itemsToRemove is null))
                {
                    MyDb.PlaylistFileDatas.RemoveRange(itemsToRemove);
                    await MyDb.SaveChangesAsync();
                }
            }
        }

        public static async Task SaveOrUpdatePosition(string folderRelativeId, TimeSpan timePosition)
        {
            float position = timePosition.Ticks;
            using (var MyDb = new DatabaseContext())
            {
                if (MyDb.FileDatas.Count() == 0)
                {
                    await MyDb.FileDatas.AddAsync(new FileData { Path = folderRelativeId, Position = position });
                    await MyDb.SaveChangesAsync();
                    return;
                }
                var PositionItem = await MyDb.FileDatas.FirstOrDefaultAsync(a => a.Path == folderRelativeId);

                if (PositionItem is null)
                {
                    await MyDb.FileDatas.AddAsync(new FileData { Path = folderRelativeId, Position = position });
                    await MyDb.SaveChangesAsync();
                    return;
                }
                MyDb.FileDatas.Update(PositionItem);
                PositionItem.Position = position;
                await MyDb.SaveChangesAsync();
            }
        }

    }
}

