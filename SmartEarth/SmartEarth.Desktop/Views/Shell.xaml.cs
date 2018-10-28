using Microsoft.Practices.Unity;
using Prism.Regions;
using SmartEarth.Common.Infrastructure;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SmartEarth.Desktop.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Shell : Window, IShell
    {
        #region Properties
        IRegionManager RegionManager { get; }
        IUnityContainer Container { get; }
        #endregion

        #region Constructors
        public Shell(IRegionManager regionManager, IUnityContainer container)
        {
            InitializeComponent();
            RegionManager = regionManager;
            Container = container;

            RoutedEventHandler loaded = null;
            Loaded += loaded = (s, e) =>
            {
                Loaded -= loaded;
                Container.RegisterInstance<IShell>(this);
                DataContext = Container.Resolve<ViewModels.ShellViewModel>();
                RegionManager.Regions[Core.MENU_REGION].Add(Container.Resolve<Views.MenuView>(), Core.MENU_VIEW);
                RegionManager.Regions[Core.MAIN_REGION].Add(Container.Resolve<Views.HomeView>(), Core.HOME_VIEW);
                RegionManager.Regions[Core.MAIN_REGION].Add(Container.Resolve<Views.LoadingView>(), Core.LOADING_VIEW);
                RegionManager.Regions[Core.MAIN_REGION].Add(Container.Resolve<Views.ScheduleView>(), Core.SCHEDULE_VIEW);
            }; 
        }
        #endregion
    }
}
