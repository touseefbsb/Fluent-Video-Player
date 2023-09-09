using Fluent_Video_Player.Helpers;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Fluent_Video_Player.Views
{
    public sealed partial class WhatsNewDialog : ContentDialog
    {
        public WhatsNewDialog()
        {
            // TODO WTS: Update the contents of this dialog every time you release a new version of the app
            RequestedTheme = (Window.Current.Content as FrameworkElement).RequestedTheme;
            InitializeComponent();
            PrimaryButtonText = "OK".GetLocalized();
        }
    }
}
