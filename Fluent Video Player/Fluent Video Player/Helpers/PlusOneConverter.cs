using System;
using Windows.UI.Xaml.Data;

namespace Fluent_Video_Player.Helpers
{
    public class PlusOneConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language) => ((int)value + 1).ToString();

        //not needed for one way data binding
        public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
    }
}
