using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace SmartEarth.Common.Infrastructure.Models
{
    [Serializable]
    public class Configuration : BindableBase
    {
        #region Properties

        #region Statics
        public static Configuration DefaultConfiguration { get; } = new Configuration()
        {
            GoogleEarthPath = Core.GOOGLE_EARTH_X64_PATH,
            Colors = LoadColors(), 
            ShowPendingTasks = false,
            ShowCompletedTasks = false
        };
        #endregion

        #region Bindables
        bool _showCompletedTasks = false;
        public bool ShowCompletedTasks { get => _showCompletedTasks; set => SetProperty(ref _showCompletedTasks, value); }

        bool _showPendingTasks = false;
        public bool ShowPendingTasks { get => _showPendingTasks; set => SetProperty(ref _showPendingTasks, value); }

        string _googleEarthPath = Core.GOOGLE_EARTH_X64_PATH;
        public string GoogleEarthPath { get => _googleEarthPath; set => SetProperty(ref _googleEarthPath, value); }
        #endregion


        public ObservableCollection<ColorBox> Colors { get; set; } = new ObservableCollection<ColorBox>();
        
        #endregion

        #region Constructors
        public Configuration() { }
        public Configuration(Configuration configuration)
        {
            GoogleEarthPath = configuration.GoogleEarthPath;

            foreach (var color in configuration.Colors) Colors.Add(color);
            ShowCompletedTasks = configuration.ShowCompletedTasks;
            ShowPendingTasks = configuration.ShowPendingTasks;
        }
        #endregion

        #region Methods

        #region Equity and Comparision
        static bool CompareConfigurations(Configuration c1, Configuration c2)
        {
            bool nullOne = ReferenceEquals(c1, null);
            bool nullTwo = ReferenceEquals(c2, null);

            if (nullOne && nullTwo) return true;
            else if (nullOne || nullTwo) return false;

            return c1.GoogleEarthPath.ToLower() == c2.GoogleEarthPath.ToLower();
        }
        #endregion

        #region Overrides
        public override bool Equals(object obj)
        {
            if (!(obj is Configuration)) return false;
            return CompareConfigurations(this, (Configuration)obj);
        }

        public static bool operator ==(Configuration c1, Configuration c2) => CompareConfigurations(c1, c2);
        public static bool operator !=(Configuration c1, Configuration c2) => CompareConfigurations(c1, c2);

        public override int GetHashCode()
        {
            object[] properties = new object[]
            {
                GoogleEarthPath
            };

            unchecked
            {
                int hash = 17;
                for (int i = 0; i < properties.Length; i++)
                    if (!ReferenceEquals(properties[i], null))
                        hash = hash * 23 + properties.GetHashCode();
                return hash;
            }
        }
        #endregion

        #region Helpers
        static ObservableCollection<ColorBox> LoadColors()
        {
            var collection = new ObservableCollection<ColorBox>();

            Type colors = typeof(Colors);

            object value = null;
            foreach (var properties in colors.GetProperties())
                if ((value = properties.GetValue(null)) is Color)
                    collection.Add(new ColorBox(UnCamel(properties.Name), (Color)value));
            return collection;
        }

        static string UnCamel(string text)
        {
            string buffer = string.Empty;
            char c = '\0';
            for (int i = 0; i < text.Length; i++)
            {
                c = text[i];
                if (!char.IsLetter(c)) continue;
                else if (char.IsLower(c) || i == 0)
                    buffer += c;
                else buffer = (buffer + ' ') + c;
            }
            return buffer;
        }
        #endregion

        #endregion
    }
}
