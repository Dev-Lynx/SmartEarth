using Microsoft.Windows.Controls.Ribbon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SmartEarth.Common.Infrastructure.Resources.Controls
{
    public class SmartEarthRibbon : Ribbon
    {
        #region Constructors
        public SmartEarthRibbon()
        {
            Loaded += OnLoaded;
        }


        #endregion

        #region Methods

        #region Event Handlers
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var count = VisualTreeHelper.GetChildrenCount(this);

                // Remove the top part of the Menu...
                for (int i = 0; i < count; i++)
                {
                    var child = VisualTreeHelper.GetChild(this, i);
                    if (child is Grid) ((Grid)child).RowDefinitions[0].Height = new GridLength(0);
                }
            }
            catch (Exception ex)
            {
                Core.Log.Error("An Error has occured while loading a smart earth ribbon. /n{0}", ex);
            }
        }
        #endregion

        #endregion
    }
}
