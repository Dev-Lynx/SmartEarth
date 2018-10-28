using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartEarth.Common.Infrastructure.Models
{
    public class Angle : BindableBase
    {
        #region Properties
        public double Max { get; set; } = 360;
        public double Min { get; set; } = 0;

        double _angle = 0.0;
        public double Value
        {
            get => _angle;
            set
            {
                if (value < Min) value = Min;
                else if (value > Max) value = Max;
                
                SetProperty(ref _angle, value);
            }
        }
        #endregion

        #region Constructors
        public Angle() { }
        public Angle(double angle) { Value = angle; }
        public Angle(double angle, double max, double min)
        {
            Value = angle;
            Max = max;
            Min = min;
        }
        #endregion

        #region Methods

        #region Equity 
        static bool CompareAngles(Angle a, Angle b)
        {
            bool nullOne = ReferenceEquals(null, a);
            bool nullTwo = ReferenceEquals(null, b);

            if (nullOne && nullTwo) return true;
            else if (nullTwo || nullOne) return false;

            return a.Value == b.Value;
        }
        #endregion

        #region Overrides
        public override bool Equals(object obj)
        {
            if (!(obj is Angle)) return false;
            return CompareAngles(this, (Angle)obj);
        }

        public static bool operator ==(Angle a, Angle b) => CompareAngles(a, b);
        public static bool operator !=(Angle a, Angle b) => CompareAngles(a, b);
        public static implicit operator Angle(double d) => new Angle(d);
        public static implicit operator double(Angle angle) => angle.Value;

        public static implicit operator Angle(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return 0;

            // Remove letters at the end of the string
            int index = s.Length - 1;
            char c = s[index--];
            while (char.IsLetter(c) && index > 0)
            {
                c = s[index--];
                s = s.Remove(index);
            }

            if (double.TryParse(s, out double d)) return d;
            else return 0;
        }

        public static implicit operator string(Angle angle)
        {
            if (angle == null) return string.Empty;
            return angle.ToString();
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Value.GetHashCode();
                hash = hash * 23 + Max.GetHashCode();
                hash = hash * 23 + Min.GetHashCode();
                return hash;
            }
        }

        public override string ToString() => $"{Value}°";
        #endregion

        #endregion
    }
}
