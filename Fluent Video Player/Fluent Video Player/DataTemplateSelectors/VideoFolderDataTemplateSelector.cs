using Fluent_Video_Player.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Fluent_Video_Player.DataTemplateSelectors
{
    public class VideoFolderDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate VideoTemplate { get; set; }
        public DataTemplate FolderTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            switch (item)
            {
                case Video _:
                    return VideoTemplate;
                case Folder _:
                    return FolderTemplate;
            }

            return base.SelectTemplateCore(item);
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => SelectTemplateCore(item);
        

    }
}
