using Microsoft.Practices.Unity;
using Prism.Logging;
using SmartEarth.Common.Infrastructure.Resources.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SmartEarth.Common.Infrastructure.Resources.Controls
{
    /// <summary>
    /// Interaction logic for SmartEarthWindow.xaml
    /// </summary>
    public partial class SmartEarthWindow : Window, IWindow
    {
        #region Properties

        #region Services
        IUnityContainer Container { get; }
        ILoggerFacade Logger { get; }
        #endregion 

        #endregion

        #region Constructors
        public SmartEarthWindow(ILoggerFacade logger)
        {
            InitializeComponent();
            Logger = logger;
        }
        #endregion
    }
}
