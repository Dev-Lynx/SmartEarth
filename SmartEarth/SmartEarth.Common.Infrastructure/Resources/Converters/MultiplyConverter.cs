using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SmartEarth.Common.Infrastructure.Resources.Converters
{
    public class MultiplyConverter : IMultiValueConverter
    {
        #region Properties
        public bool Negate { get; set; }
        #endregion

        #region Constructors
        #endregion

        #region Methods

        #region IMultiValueConverter Implementation
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double result = 1.0;
            Core.Log.Debug("Multiply Values: ");
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] is double || values[i] is int)
                    result *= (double)values[i];

                Core.Log.Debug("{0}", values[i]);

                if (Negate) result *= -1;
            }
            Core.Log.Debug("Done");

            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new Exception("Not implemented");
        }
        #endregion

        #endregion
    }
}
