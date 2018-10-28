using SmartEarth.Common.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SmartEarth.Common.Infrastructure.Resources.Converters
{
    public class AngleToStringConverter : IValueConverter
    {
        #region Methods

        #region IValueConveter Implementation
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Angle) return value.ToString();
            else return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
            {
                Angle angle = (string)value;
                return angle;
            }
            else return new Angle();
        }
        #endregion

        #endregion
    }
}
