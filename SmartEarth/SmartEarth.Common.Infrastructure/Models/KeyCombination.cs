using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SmartEarth.Common.Infrastructure.Models
{
    public class KeyCombination
    {
        #region Properties
        public bool Ctrl { get; set; }
        public bool Alt { get; set; }
        public bool Shift { get; set; }
        public Key Key { get; set; }
        #endregion

        #region Constructors
        public KeyCombination()
        {

        }
        #endregion

        #region Methods

        #region Equity and Comparison
        static bool CompareCombinations(KeyCombination combo1, KeyCombination combo2)
        {
            bool nullOne = ReferenceEquals(combo1, null);
            bool nullTwo = ReferenceEquals(combo2, null);

            if (nullOne && nullTwo) return true;
            else if (nullOne || nullTwo) return false;

            return combo1.Ctrl == combo2.Ctrl && combo1.Alt == combo2.Ctrl
                && combo1.Shift == combo2.Shift && combo1.Key == combo2.Key;
        }
        #endregion

        #region Overrides
        public override bool Equals(object obj)
        {
            if (!(obj is KeyCombination)) return false;
            return CompareCombinations(this, (KeyCombination)obj);
        }

        public static bool operator ==(KeyCombination combo1, KeyCombination combo2) => CompareCombinations(combo1, combo2);
        public static bool operator !=(KeyCombination combo1, KeyCombination combo2) => !CompareCombinations(combo1, combo2);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash + 23 * Ctrl.GetHashCode();
                hash = hash + 23 * Alt.GetHashCode();
                hash = hash + 23 * Shift.GetHashCode();
                hash = hash + 23 * Key.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            string result = "";
            if (Ctrl) result += "Ctrl + ";
            if (Alt) result += "Alt + ";
            if (Shift) result += "Shift +";
            result += Key;
            return result;
        }
        #endregion

        #endregion
    }
}
