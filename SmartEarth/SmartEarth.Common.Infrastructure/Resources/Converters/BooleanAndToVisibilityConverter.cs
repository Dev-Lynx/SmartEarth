using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace SmartEarth.Common.Infrastructure.Resources.Converters
{
    public class BooleanAndToVisibilityConverter : IMultiValueConverter
    {
        #region Properties
        public Visibility HiddenVisibility { get; set; } = Visibility.Collapsed;
        #endregion


        #region Methods

        #region IMultiValueConverter Implementation
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            foreach (object value in values)
                if ((value is bool) && (bool)value == false)
                    return HiddenVisibility;

            return Visibility.Visible;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("BooleanAndToVisibilityConverter is a OneWay converter.");
        }
        #endregion

        #endregion
    }
}
