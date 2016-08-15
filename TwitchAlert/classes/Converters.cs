using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Data;
//using System.Windows.Media;

namespace TwitchAlert.classes
{
    public class BoolToIsLiveConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(string))
                throw new InvalidOperationException("BoolToIsLiveConverter: The target must be of type string!");
            return (bool)value ? "Is Live" : "Is Offline";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

