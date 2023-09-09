using Fluent_Video_Player.Helpers;
using MahApps.Metro.IconPacks;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System.Windows.Input;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Fluent_Video_Player.Controls
{
    public sealed partial class FluentGridView : UserControl
    {
        private readonly PackIconMaterialDesign _listIcon = new PackIconMaterialDesign { Kind = PackIconMaterialDesignKind.ViewList };
        private readonly PackIconMaterialDesign _gridIcon = new PackIconMaterialDesign { Kind = PackIconMaterialDesignKind.ViewComfy };
        public static AutoSuggestBox Box;
        public FluentGridView() { InitializeComponent(); Box = MySearchBox; }

        #region PublicProps
        public AutoSuggestBox SearchBox => MySearchBox;
        public AdaptiveGridView MyGridView => MyAdaptiveView;
        #endregion

        #region DPs
        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            set { SetValue(IsLoadingProperty, value); }
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
            get { return (DataTemplate)GetValue(MyItemTemplateProperty); }
            set { SetValue(MyItemTemplateProperty, value); }
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


        #endregion

        #region PopupAnimation

        //private static void PopAnimate(object sender, bool Up)
        //{
        //    var item = ((GridViewItem)sender) as UIElement;
        //    var shadowPanel = item.FindDescendant<DropShadowPanel>();
        //    var itemScaleAnimation = new ScaleAnimation() { Duration = ScaleAnimationTime };
        //    var shadowOpacityAnimation = new OpacityAnimation() { To = 0.6f, Duration = ScaleAnimationTime };
        //    var shadowAnimation = new ScaleAnimation() { Duration = ScaleAnimationTime };
        //    if (Up)
        //    {
        //        Canvas.SetZIndex(item, 5);
        //        itemScaleAnimation.To = ScaleUpValue;
        //        shadowOpacityAnimation.To = 1f;
        //        shadowAnimation.To = ScaleUpValue;
        //    }
        //    else
        //    {
        //        Canvas.SetZIndex(item, 0);
        //        itemScaleAnimation.To = "1.0";
        //        shadowOpacityAnimation.To = 0.6f;
        //        shadowAnimation.To = "1.0";
        //    }
        //    itemScaleAnimation.StartAnimation(item);
        //    shadowOpacityAnimation.StartAnimation(shadowPanel);
        //    shadowAnimation.StartAnimation(shadowPanel);
        //}

        #endregion

        #region Events
        private ICommand _toggleViewCommand;
        public ICommand ToggleViewCommand => _toggleViewCommand ?? (_toggleViewCommand = new RelayCommand(OnToggleView));
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
            viewChangeButton.Content = _listIcon;
            MyDesiredWidth = (double)Application.Current.Resources["GridDesiredWidth"];
            MyItemHeight = (double)Application.Current.Resources["GridItemHeight"];
            ApplicationData.Current.LocalSettings.SaveString(SettingsStorageExtensions.CollectionViewKey, CurrentCollectionView.GridView.ToString());
        }
        private void TurnToListView()
        {
            viewChangeButton.Content = _gridIcon;
            MyDesiredWidth = (double)Application.Current.Resources["ListDesiredWidth"];
            MyItemHeight = (double)Application.Current.Resources["ListItemHeight"];
            ApplicationData.Current.LocalSettings.SaveString(SettingsStorageExtensions.CollectionViewKey, CurrentCollectionView.ListView.ToString());
        }
        #endregion

        private void MyAdaptiveView_Loaded(object sender, RoutedEventArgs e) => MyAdaptiveView.ItemsPanelRoot.Margin = new Thickness(12, 12, 12, 0);
    }
}
