using Microsoft.EntityFrameworkCore;

namespace Database;

public class DbService : IDbService
{
    public TimeSpan LastRetrievedPosition = TimeSpan.Zero;
    public void Initialize()
    {
        using var MyDb = new DatabaseContext();
        MyDb.Database.Migrate();
    }

    public async Task<int> GetMyViewsAsync(string folderRelativeId)
    {
        using var MyDb = new DatabaseContext();
        if (MyDb is not null)
        {
            var fileDataItem = await MyDb.FileDatas.FirstOrDefaultAsync(a => a.Path == folderRelativeId);
            return fileDataItem?.Views ?? 0;
        }
        return 0;
    }

    public async Task UpdateViewsAsync(string folderRelativeId)
    {
        using var MyDb = new DatabaseContext();
        if (MyDb is not null)
        {
            if (!MyDb.FileDatas.Any())
            {
                _ = await MyDb.FileDatas.AddAsync(new FileData { Path = folderRelativeId, Views = 1 });
                _ = await MyDb.SaveChangesAsync();
                return;
            }
            var viewItem = await MyDb.FileDatas.FirstOrDefaultAsync(a => a.Path == folderRelativeId);

            if (viewItem == null)
            {
                MyDb.FileDatas?.Add(new FileData { Path = folderRelativeId, Views = 1 });
                _ = await MyDb.SaveChangesAsync();
                return;
            }
            _ = MyDb.FileDatas.Update(viewItem);
            viewItem.Views++;
            _ = await MyDb.SaveChangesAsync();
        }
    }

    public async Task<TimeSpan> GetPositionAsync(string folderRelativeId)
    {
        using var MyDb = new DatabaseContext();
        if (!MyDb.FileDatas.Any())
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
    public async Task<List<string>> GetPlaylistsAsync()
    {
        using var MyDb = new DatabaseContext();
        return await MyDb.PlaylistDatas.Select(a => a.Playlist).ToListAsync();
    }
    public async Task<bool> IsPlaylistNameNewAsync(string name)
    {
        using var MyDb = new DatabaseContext();
        return !(await MyDb.PlaylistDatas.Select(a => a.Playlist).ToListAsync()).Contains(name);
    }
    public async Task RemovePlaylistAsync(string playlistName)
    {
        using var MyDb = new DatabaseContext();
        var playlistToRemove = await MyDb.PlaylistDatas.FirstOrDefaultAsync(a => a.Playlist == playlistName);

        if (playlistToRemove is not null)
        {
            _ = MyDb.PlaylistDatas.Remove(playlistToRemove);
        }

        MyDb.PlaylistFileDatas.RemoveRange(MyDb.PlaylistFileDatas.Where(a => a.Playlist == playlistName));

        _ = await MyDb.SaveChangesAsync();
    }
    public async Task<List<string>> GetVideosForPlaylistAsync(string playlistName)
    {
        using var MyDb = new DatabaseContext();
        return await MyDb.PlaylistFileDatas.Where(a => a.Playlist == playlistName).Select(a => a.Path).ToListAsync();
    }
    public async Task<List<string>> GetPlaylistsForVideoAsync(string path)
    {
        using var MyDb = new DatabaseContext();
        return await MyDb.PlaylistFileDatas.Where(a => a.Path == path).Select(a => a.Playlist).ToListAsync();
    }

    public async Task CreateNewPlaylistAsync(string playlistName, string path)
    {
        using var MyDb = new DatabaseContext();
        _ = MyDb.PlaylistDatas.Add(new PlaylistData { Playlist = playlistName });
        _ = MyDb.PlaylistFileDatas.Add(new PlaylistFileData { Playlist = playlistName, Path = path });
        _ = await MyDb.SaveChangesAsync();
    }
    public async Task AddToPlaylistAsync(string playlistName, string path)
    {
        using var MyDb = new DatabaseContext();
        var item = await MyDb.PlaylistFileDatas.FirstOrDefaultAsync(a => a.Playlist == playlistName && a.Path == path);
        if (item is null)
        {
            _ = MyDb.PlaylistFileDatas.Add(new PlaylistFileData { Playlist = playlistName, Path = path });
            _ = await MyDb.SaveChangesAsync();
        }
    }
    public async Task RemoveFromPlaylistAsync(string playlistName, string path)
    {
        using var MyDb = new DatabaseContext();
        var itemToRemove = await MyDb.PlaylistFileDatas.FirstOrDefaultAsync(a => a.Playlist == playlistName && a.Path == path);
        if (itemToRemove is not null)
        {
            _ = MyDb.PlaylistFileDatas.Remove(itemToRemove);
            _ = await MyDb.SaveChangesAsync();
        }
    }

    public async Task RemoveFromAllPlaylistsAsync(string path)
    {
        using var MyDb = new DatabaseContext();
        var itemsToRemove = MyDb.PlaylistFileDatas.Where(a => a.Path == path);
        if (itemsToRemove is not null)
        {
            MyDb.PlaylistFileDatas.RemoveRange(itemsToRemove);
            _ = await MyDb.SaveChangesAsync();
        }
    }

    public async Task SaveOrUpdatePositionAsync(string folderRelativeId, TimeSpan timePosition)
    {
        float position = timePosition.Ticks;
        using var MyDb = new DatabaseContext();
        if (!MyDb.FileDatas.Any())
        {
            _ = await MyDb.FileDatas.AddAsync(new FileData { Path = folderRelativeId, Position = position });
            _ = await MyDb.SaveChangesAsync();
            return;
        }
        var PositionItem = await MyDb.FileDatas.FirstOrDefaultAsync(a => a.Path == folderRelativeId);

        if (PositionItem is null)
        {
            _ = await MyDb.FileDatas.AddAsync(new FileData { Path = folderRelativeId, Position = position });
            _ = await MyDb.SaveChangesAsync();
            return;
        }
        _ = MyDb.FileDatas.Update(PositionItem);
        PositionItem.Position = position;
        _ = await MyDb.SaveChangesAsync();
    }

}
