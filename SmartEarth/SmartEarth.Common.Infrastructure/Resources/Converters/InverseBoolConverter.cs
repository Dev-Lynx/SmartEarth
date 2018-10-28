using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SmartEarth.Common.Infrastructure.Resources.Converters
{
    public class InverseBoolConverter : IValueConverter
    {
        #region Properties
        #endregion

        #region Methods

        #region IValueConverter Implementation
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool)) return false;
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool)) return false;
            return !(bool)value;
        }
        #endregion

        #endregion
    }
}
