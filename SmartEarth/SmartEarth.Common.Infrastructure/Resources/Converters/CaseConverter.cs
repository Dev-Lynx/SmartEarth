using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace SmartEarth.Common.Infrastructure.Resources.Converters
{
    public class CaseConverter : IValueConverter
    {
        #region Properties
        public CharacterCasing Case { get; set; } = CharacterCasing.Upper;
        #endregion

        #region Constructors
        public CaseConverter() { }
        #endregion

        #region Methods
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is string)) return string.Empty;
            string s = (string)value;

            switch (Case)
            {
                case CharacterCasing.Lower: return s.ToLower();
                case CharacterCasing.Normal: return s;
                case CharacterCasing.Upper: return s.ToUpper();
            }
            return s;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
        #endregion
    }
}
