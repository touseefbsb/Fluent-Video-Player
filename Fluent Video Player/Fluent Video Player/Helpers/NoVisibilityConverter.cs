using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Fluent_Video_Player.Helpers
{
    public class NoVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int itemsCount)
                return itemsCount == 0 ? Visibility.Visible : Visibility.Collapsed;

            return Visibility.Collapsed;
        }

        //not needed for one way data binding
        public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
    }
}
