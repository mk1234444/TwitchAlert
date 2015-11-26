using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace TwitchAlert.classes
{
    //public class IntToImageSourceConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        int index = (int)value;
    //        switch (index)
    //        {
    //            case 0:
    //                return new BitmapImage(new Uri(@"/CricketScores;component/Images/Soccer-icon.png", UriKind.Relative));
    //            case 1:
    //                return new BitmapImage(new Uri(@"/CricketScores;component/Images/cricketIcon.png", UriKind.Relative));
    //            case 2:
    //                return new BitmapImage(new Uri(@"/CricketScores;component/Images/newsIcon.png", UriKind.Relative));
    //            case 3:
    //                return new BitmapImage(new Uri(@"/CricketScores;component/Images/culture-Icon.png", UriKind.Relative));
    //            case 4:
    //                return new BitmapImage(new Uri(@"/CricketScores;component/Images/techIcon2.png", UriKind.Relative));
    //            default:
    //                return null;
    //        }
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    public class BoolToIsLiveConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (bool)value ? "Is Live" : "Is No Longer Live";

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

