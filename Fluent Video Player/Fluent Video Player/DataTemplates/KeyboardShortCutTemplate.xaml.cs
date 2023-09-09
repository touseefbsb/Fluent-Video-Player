using Fluent_Video_Player.Helpers;
using Fluent_Video_Player.Models;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Fluent_Video_Player.DataTemplates
{
    public sealed partial class KeyboardShortCutTemplate : UserControl
    {
        public KeyboardShortCutTemplate() => InitializeComponent();

        public KeyboardShortCut MyShortCut
        {
            get { return (KeyboardShortCut)GetValue(MyShortCutProperty); }
            set { SetValue(MyShortCutProperty, value); }
        }
        public static readonly DependencyProperty MyShortCutProperty =
            DependencyProperty.Register("MyShortCut", typeof(KeyboardShortCut), typeof(KeyboardShortCutTemplate), new PropertyMetadata(null));


        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CheckView();
            SettingsStorageExtensions.OnCollectionViewSelected += SettingsStorageExtensions_OnCollectionViewSelected;
        }

        private void SettingsStorageExtensions_OnCollectionViewSelected(object sender, System.EventArgs e) => CheckView();

        private void UserControl_Unloaded(object sender, RoutedEventArgs e) => SettingsStorageExtensions.OnCollectionViewSelected -= SettingsStorageExtensions_OnCollectionViewSelected;
        private void CheckView()
        {
            switch (ApplicationData.Current.LocalSettings.ReadCurrentCollectionView())
            {
                case CurrentCollectionView.GridView:
                    TurnToGridView();
                    break;
                case CurrentCollectionView.ListView:
                    TurnToListView();
                    break;
            }
        }
        private void TurnToGridView()
        {
            GridVieww.Visibility = Visibility.Visible;
            ListVieww.Visibility = Visibility.Collapsed;
        }
        private void TurnToListView()
        {
            GridVieww.Visibility = Visibility.Collapsed;
            ListVieww.Visibility = Visibility.Visible;
        }
    }
}
