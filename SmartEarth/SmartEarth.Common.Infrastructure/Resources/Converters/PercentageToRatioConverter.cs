using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SmartEarth.Common.Infrastructure.Resources.Converters
{
    public class PercentageToRatioConverter : IValueConverter
    {
        #region Methods

        #region IValueConverter Implementation
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is double)) return 0;

            double d = (double)value;

            return d / 100.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is double)) return 0;

            double d = (double)value;

            return d * 100.0;
        }
        #endregion

        #endregion
    }
}
