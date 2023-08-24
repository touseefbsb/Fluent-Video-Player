using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.UI.Controls;
using Fluent_Video_Player.Extensions;
using Microsoft.UI.Xaml;
using System.Windows.Input;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;
using Fluent_Video_Player.Core.Enums;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Fluent_Video_Player.UserControls
{
    public sealed partial class FluentGridView : UserControl
    {
        public static AutoSuggestBox Box;
        public FluentGridView() { InitializeComponent(); Box = MySearchBox; }

        #region PublicProps
        public AutoSuggestBox SearchBox => MySearchBox;
        public AdaptiveGridView MyGridView => MyAdaptiveView;
        #endregion PublicProps

        #region DPs
        public bool IsLoading
        {
            get => (bool)GetValue(IsLoadingProperty);
            set => SetValue(IsLoadingProperty, value);
        }
        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register("IsLoading", typeof(bool), typeof(FluentGridView), new PropertyMetadata(null));
        public object MyItemsSource
        {
            get => GetValue(MyItemsSourceProperty);
            set => SetValue(MyItemsSourceProperty, value);
        }
        public static readonly DependencyProperty MyItemsSourceProperty =
            DependencyProperty.Register("MyItemsSource", typeof(object), typeof(FluentGridView), new PropertyMetadata(null));

        public string MyTitle
        {
            get => (string)GetValue(MyTitleProperty);
            set => SetValue(MyTitleProperty, value);
        }
        public static readonly DependencyProperty MyTitleProperty =
            DependencyProperty.Register("MyTitle", typeof(string), typeof(FluentGridView), new PropertyMetadata(null));

        public double MyDesiredWidth
        {
            get => (double)GetValue(MyDesiredWidthProperty);
            set => SetValue(MyDesiredWidthProperty, value);
        }
        public static readonly DependencyProperty MyDesiredWidthProperty =
            DependencyProperty.Register("MyDesiredWidth", typeof(double), typeof(FluentGridView), new PropertyMetadata(null));

        public double MyItemHeight
        {
            get => (double)GetValue(MyItemHeightProperty);
            set => SetValue(MyItemHeightProperty, value);
        }
        public static readonly DependencyProperty MyItemHeightProperty =
            DependencyProperty.Register("MyItemHeight", typeof(double), typeof(FluentGridView), new PropertyMetadata(null));

        public DataTemplate MyItemTemplate
        {
            get => (DataTemplate)GetValue(MyItemTemplateProperty);
            set => SetValue(MyItemTemplateProperty, value);
        }
        public static readonly DependencyProperty MyItemTemplateProperty =
            DependencyProperty.Register("MyItemTemplate", typeof(DataTemplate), typeof(FluentGridView), new PropertyMetadata(null));

        public DataTemplateSelector MyItemTemplateSelector
        {
            get => (DataTemplateSelector)GetValue(MyItemTemplateSelectorProperty);
            set => SetValue(MyItemTemplateSelectorProperty, value);
        }
        public static readonly DependencyProperty MyItemTemplateSelectorProperty =
            DependencyProperty.Register("MyItemTemplateSelector", typeof(DataTemplateSelector), typeof(FluentGridView), new PropertyMetadata(null));

        public object MyCustomHeader
        {
            get => GetValue(MyCustomHeaderProperty);
            set => SetValue(MyCustomHeaderProperty, value);
        }
        public static readonly DependencyProperty MyCustomHeaderProperty =
            DependencyProperty.Register("MyCustomHeader", typeof(object), typeof(FluentGridView), new PropertyMetadata(null));

        public object MyCustomHeaderIcon
        {
            get => GetValue(MyCustomHeaderIconProperty);
            set => SetValue(MyCustomHeaderIconProperty, value);
        }
        public static readonly DependencyProperty MyCustomHeaderIconProperty =
            DependencyProperty.Register("MyCustomHeaderIcon", typeof(object), typeof(FluentGridView), new PropertyMetadata(null));

        public int ItemsCount
        {
            get => (int)GetValue(ItemsCountProperty);
            set => SetValue(ItemsCountProperty, value);
        }
        public static readonly DependencyProperty ItemsCountProperty =
            DependencyProperty.Register("ItemsCount", typeof(int), typeof(FluentGridView), new PropertyMetadata(null));

        #endregion DPs

        #region Events
        private ICommand _toggleViewCommand;
        public ICommand ToggleViewCommand => _toggleViewCommand ??= new RelayCommand(OnToggleView);
        private void OnToggleView() => ReverseView();

        private void AppBarButton_Loaded(object sender, RoutedEventArgs e) => CheckView();

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
        private void ReverseView()
        {
            switch (ApplicationData.Current.LocalSettings.ReadCurrentCollectionView())
            {
                case CurrentCollectionView.GridView:
                    TurnToListView();
                    break;
                case CurrentCollectionView.ListView:
                    TurnToGridView();
                    break;
            }
        }
        private void TurnToGridView()
        {
            ToggleViewButton.Icon = new SymbolIcon { Symbol = Symbol.AllApps };
            MyDesiredWidth = (double)Application.Current.Resources["GridDesiredWidth"];
            MyItemHeight = (double)Application.Current.Resources["GridItemHeight"];
            ApplicationData.Current.LocalSettings.SaveString(SettingsStorageExtensions.CollectionViewKey, nameof(CurrentCollectionView.GridView));
        }
        private void TurnToListView()
        {
            ToggleViewButton.Icon = new SymbolIcon { Symbol = Symbol.GridView };
            MyDesiredWidth = (double)Application.Current.Resources["ListDesiredWidth"];
            MyItemHeight = (double)Application.Current.Resources["ListItemHeight"];
            ApplicationData.Current.LocalSettings.SaveString(SettingsStorageExtensions.CollectionViewKey, nameof(CurrentCollectionView.ListView));
        }
        #endregion

        private void MyAdaptiveView_Loaded(object sender, RoutedEventArgs e) => MyAdaptiveView.ItemsPanelRoot.Margin = new Thickness(12, 12, 12, 0);
    }
}
