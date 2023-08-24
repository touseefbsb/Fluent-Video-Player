using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Fluent_Video_Player.Converters;

public class NoVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        value is int itemsCount ? itemsCount == 0 ? Visibility.Visible : Visibility.Collapsed : (object)Visibility.Collapsed;

    //not needed for one way data binding
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
