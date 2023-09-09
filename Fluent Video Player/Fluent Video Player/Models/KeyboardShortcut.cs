using MahApps.Metro.IconPacks;

namespace Fluent_Video_Player.Models
{
    public class KeyboardShortCut
    {
        public string Description { get; set; }
        public string ShortCut { get; set; }
        public PackIconMaterialDesignKind Kind { get; set; }
        public KeyboardShortCut THIS => this;
    }
}
