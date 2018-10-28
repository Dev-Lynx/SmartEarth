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
    public class LengthToThicknessConverter : IValueConverter
    {
        #region Properties
        public bool Left { get; set; } = true;
        public bool Top { get; set; } = true;
        public bool Right { get; set; } = true;
        public bool Bottom { get; set; } = true;
        public bool Negate { get; set; } = false;
        #endregion

        #region Constructors
        public LengthToThicknessConverter() { }
        #endregion

        #region Methods

        #region IValueConverter Implementation
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is double) && !(value is int)) return new Thickness(0);

            double left, top, right, bottom = 0;
            left = top = right = bottom = Negate ? -(double)value : (double)value;

            if (!Left) left = 0;
            if (!Top) top = 0;
            if (!Right) right = 0;
            if (!Bottom) bottom = 0;

            return new Thickness(left, top, right, bottom);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Thickness)) return .0;

            return ((Thickness)value).Left;
        }
        #endregion

        #endregion
    }
}
