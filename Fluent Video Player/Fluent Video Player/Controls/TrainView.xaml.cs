using Fluent_Video_Player.Helpers;
using Fluent_Video_Player.Models;
using Fluent_Video_Player.Services;
using Fluent_Video_Player.Views;
using System;
using System.Windows.Input;
using Fluent_Video_Player.Extensions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using MahApps.Metro.IconPacks;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Fluent_Video_Player.Controls
{
    public sealed partial class TrainView : UserControl
    {
        public TrainView() => InitializeComponent();

        #region DPs
        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            set { SetValue(IsLoadingProperty, value); }
        }
        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register("IsLoading", typeof(bool), typeof(TrainView), new PropertyMetadata(null));


        public object MyItemsSource
        {
            get => GetValue(MyItemsSourceProperty);
            set => SetValue(MyItemsSourceProperty, value);
        }
        public static readonly DependencyProperty MyItemsSourceProperty =
            DependencyProperty.Register("MyItemsSource", typeof(object), typeof(TrainView), new PropertyMetadata(null));

        public string MyTitle
        {
            get => (string)GetValue(MyTitleProperty);
            set => SetValue(MyTitleProperty, value);
        }
        public static readonly DependencyProperty MyTitleProperty =
            DependencyProperty.Register("MyTitle", typeof(string), typeof(TrainView), new PropertyMetadata(null));

        public PackIconMaterialDesignKind MyKind
        {
            get { return (PackIconMaterialDesignKind)GetValue(MyKindProperty); }
            set { SetValue(MyKindProperty, value); }
        }

        public static readonly DependencyProperty MyKindProperty =
            DependencyProperty.Register("MyKind", typeof(PackIconMaterialDesignKind), typeof(TrainView), new PropertyMetadata(null));


        public int MyItemsCount
        {
            get { return (int)GetValue(MyItemsCountProperty); }
            set { SetValue(MyItemsCountProperty, value); }
        }
        public static readonly DependencyProperty MyItemsCountProperty =
            DependencyProperty.Register("MyItemsCount", typeof(int), typeof(TrainView), new PropertyMetadata(null));

        #endregion

        #region Commands
        private ICommand _viewAllCommand;
        private ICommand _itemClickCommand;
        private ICommand _scrollCommand;
        public ICommand ViewAllCommand => _viewAllCommand ?? (_viewAllCommand = new RelayCommand(OnViewAll));
        public ICommand ScrollCommand => _scrollCommand ?? (_scrollCommand = new RelayCommand<string>(OnScroll));


        public ICommand ItemClickCommand => _itemClickCommand ?? (_itemClickCommand = new RelayCommand<Video>(OnItemClick));

        private async void OnItemClick(Video video)
        {
            if (!(video is null))
            {
                await MediaHelper.MyMediaPlayer.SetNewSource(video.MyVideoFile);
                NavigationService.Navigate(typeof(PlayerPage));
            }
        }

        private void OnViewAll()
        {
            switch (MyTitle)
            {
                case "Library":
                    NavigationService.Navigate<LibraryPage>();
                    break;
                case "History":
                    NavigationService.Navigate<HistoryPage>();
                    break;
            }
        }
        private void OnScroll(string obj)
        {
            switch (obj)
            {
                case "Right":
                    MyTrainView.SmoothScrollNavigation(Convert.ToInt32(MyTrainView.ActualWidth), ScrollNavigationDirection.Right);
                    break;
                case "Left":
                    MyTrainView.SmoothScrollNavigation(Convert.ToInt32(MyTrainView.ActualWidth), ScrollNavigationDirection.Left);
                    break;
                default:
                    break;
            }
        }

        #endregion

        private void MyAdaptiveView_Loaded(object sender, RoutedEventArgs e)
        {
            MyTrainView.ItemsPanelRoot.Margin = new Thickness(20, 0, 20, 0);
            TitleBlock.Text = MyTitle.GetLocalized();
            switch (MyTitle)
            {
                case "Library":
                    NoItemsBlock.Text = "NoItems".GetLocalized();                    
                    break;
                case "History":
                    NoItemsBlock.Text = "NoHistory".GetLocalized();
                    break;
                default:
                    break;
            }
        }
    }
}
