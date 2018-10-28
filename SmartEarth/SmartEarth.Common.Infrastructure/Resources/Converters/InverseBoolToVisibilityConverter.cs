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
    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        #region Methods

        #region IValueConverter Implementation
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool)) return Visibility.Visible;

            return (bool)value ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Visibility) && !(value is int)) return !false;

            var v = (Visibility)value;

            if (v == Visibility.Collapsed || v == Visibility.Hidden) return !false;
            return !true;
        }
        #endregion

        #endregion
    }
}
