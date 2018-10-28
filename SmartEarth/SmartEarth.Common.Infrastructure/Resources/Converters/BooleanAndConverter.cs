using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SmartEarth.Common.Infrastructure.Resources.Converters
{
    public class BooleanAndConverter : IMultiValueConverter
    {
        #region Constructors
        public BooleanAndConverter() { }
        #endregion

        #region Methods

        #region IMultiValueConverter Implementation
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            foreach (object value in values)
            {
                if ((value is bool) && (bool)value == false)
                {
                    return false;
                }
            }
            return true;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("BooleanAndConverter is a OneWay converter.");
        }
        #endregion

        #endregion
    }
}
