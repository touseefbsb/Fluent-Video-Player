using Fluent_Video_Player.DataTemplates.ViewModels;
using Fluent_Video_Player.Helpers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Fluent_Video_Player.DataTemplates
{
    public sealed partial class PlaylistTemplate : UserControl
    {
        public PlaylistTemplateViewModel ViewModel { get; } = new PlaylistTemplateViewModel();
        public PlaylistTemplate() => InitializeComponent();

        public FluentPlaybackItem MyVideo
        {
            get => (FluentPlaybackItem)GetValue(MyVideoProperty);
            set => SetValue(MyVideoProperty, value);
        }
        public static readonly DependencyProperty MyVideoProperty =
            DependencyProperty.Register("MyVideo", typeof(FluentPlaybackItem), typeof(VideoTemplate), new PropertyMetadata(null));

        private void UserControl_Loaded(object sender, RoutedEventArgs e) => ViewModel.Initialize();
    }
}
