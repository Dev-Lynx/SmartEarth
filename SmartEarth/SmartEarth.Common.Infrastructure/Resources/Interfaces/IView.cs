using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SmartEarth.Common.Infrastructure.Resources.Interfaces
{
    public interface IView
    {
        #region Properties
        bool IsVisible { get; }
        Visibility Visibility { get; set; }

        #region Events
        event DependencyPropertyChangedEventHandler IsVisibleChanged;
        #endregion

        #endregion

        #region Methods
        bool Focus();
        #endregion
    }
}
