using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace SmartEarth.Common.Infrastructure.Resources.Converters
{
    public class InvisibleWhenZeroConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is int) && !(value is double)) return Visibility.Visible;

            int n = (int)value;

            if (n == 0) return Visibility.Hidden;
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Visibility)) return 1;
            return ((Visibility)value) != Visibility.Visible ? 1 : 0;
        }
    }
}
