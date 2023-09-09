namespace Fluent_Video_Player.Database
{
    public class FileData
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public float Position { get; set; } = 0.0f;
        public int Views { get; set; }
    }
}
