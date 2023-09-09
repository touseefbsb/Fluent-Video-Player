using MahApps.Metro.IconPacks;

namespace Fluent_Video_Player.Models
{
    public class FolderFilterItem
    {
        public PackIconMaterialDesignKind MyKind { get; private set; }
        public string MyTooltip { get; private set; }
        public FolderFilterItem(PackIconMaterialDesignKind kind, string tooltip)
        {
            MyKind = kind; MyTooltip = tooltip;
        }
    }
}
