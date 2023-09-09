using Microsoft.UI.Xaml.Controls;
using System;
using Windows.UI.Xaml.Data;

namespace Fluent_Video_Player.Helpers
{
    public class BoolToBackVisibileConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (bool)value == true ? NavigationViewBackButtonVisible.Visible : NavigationViewBackButtonVisible.Collapsed;            
        }

        //not needed for one way data binding
        public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
    }
}
