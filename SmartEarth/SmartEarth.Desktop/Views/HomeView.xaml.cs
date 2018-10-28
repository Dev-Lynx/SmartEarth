using Microsoft.Practices.Unity;
using Prism.Logging;
using Prism.Regions;
using SmartEarth.Common.Infrastructure;
using SmartEarth.Common.Infrastructure.Extensions;
using SmartEarth.Common.Infrastructure.Services.Interfaces;
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
    /// Interaction logic for HomeView.xaml
    /// </summary>
    public partial class HomeView : UserControl
    {
        #region Properties

        #region Services
        ILoggerFacade Logger { get; }
        IRegionManager RegionManager { get; }
        IUnityContainer Container { get; }
        IViewManager ViewManager { get; }
        #endregion

        #endregion

        #region Constructors
        public HomeView(ILoggerFacade logger, IRegionManager regionManager, IUnityContainer container, IViewManager viewManager)
        {
            InitializeComponent();
            Logger = logger;
            RegionManager = regionManager;
            Container = container;
            ViewManager = viewManager;

            RoutedEventHandler loaded = null;
            Loaded += loaded = (s, e) =>
            {
                Loaded -= loaded;
                RegionManager.Regions[Core.HOME_REGION].Add(Container.Resolve<RecentView>(), Core.RECENT_VIEW);
                RegionManager.Regions[Core.HOME_REGION].Add(container.Resolve<ImageView>(), Core.IMAGE_VIEW);


                RegionManager.RequestNavigateToView(Core.HOME_REGION, Core.RECENT_VIEW);
            };
        }
        #endregion


    }
}
