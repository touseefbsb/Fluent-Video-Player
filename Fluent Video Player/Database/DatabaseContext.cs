using Microsoft.EntityFrameworkCore;

namespace Database;

public class DatabaseContext : DbContext
{
    private const string dbName = "Data Source=FVPDatabase5.db";//version 5 and above db name
    public DbSet<FileData> FileDatas { get; set; }
    public DbSet<PlaylistFileData> PlaylistFileDatas { get; set; }
    public DbSet<PlaylistData> PlaylistDatas { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlite(dbName);
}
