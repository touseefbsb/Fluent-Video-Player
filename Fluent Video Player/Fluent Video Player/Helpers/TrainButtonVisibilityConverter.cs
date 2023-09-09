using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Fluent_Video_Player.Helpers
{
    public class TrainButtonVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int itemsCount)
                return itemsCount == 0 ? Visibility.Collapsed : Visibility.Visible;

            return Visibility.Visible;
        }

        //not needed for one way data binding
        public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
    }
}
