namespace Database
{
    public interface IDbService
    {
        Task AddToPlaylistAsync(string playlistName, string path);
        Task CreateNewPlaylistAsync(string playlistName, string path);
        Task<int> GetMyViewsAsync(string folderRelativeId);
        Task<List<string>> GetPlaylistsAsync();
        Task<List<string>> GetPlaylistsForVideoAsync(string path);
        Task<TimeSpan> GetPositionAsync(string folderRelativeId);
        Task<List<string>> GetVideosForPlaylistAsync(string playlistName);
        void Initialize();
        Task<bool> IsPlaylistNameNewAsync(string name);
        Task RemoveFromAllPlaylistsAsync(string path);
        Task RemoveFromPlaylistAsync(string playlistName, string path);
        Task RemovePlaylistAsync(string playlistName);
        Task SaveOrUpdatePositionAsync(string folderRelativeId, TimeSpan timePosition);
        Task UpdateViewsAsync(string folderRelativeId);
    }
}