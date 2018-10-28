using Microsoft.Practices.Unity;
using Prism.Regions;
using SmartEarth.Common.Infrastructure;
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
    /// Interaction logic for ScheduleView.xaml
    /// </summary>
    public partial class ScheduleView : UserControl
    {
        #region Properties
        IRegionManager RegionManager { get; }
        IUnityContainer Container { get; }
        #endregion

        public ScheduleView(IRegionManager regionManager, IUnityContainer container)
        {
            InitializeComponent();
            RegionManager = regionManager;
            Container = container;
            DataContext = Container.Resolve<ViewModels.ScheduleViewModel>();

            RoutedEventHandler loaded = null;
            Loaded += loaded = (s, e) =>
            {
                Loaded -= loaded;
                

                RegionManager.Regions[Core.SCHEDULE_REGION].Add(Container.Resolve<Views.MonthScheduleView>(), Core.MONTH_SCHEDULE_VIEW);
                RegionManager.Regions[Core.SCHEDULE_REGION].Add(Container.Resolve<Views.DayScheduleView>(), Core.DAY_SCHEDULE_VIEW);
                RegionManager.Regions[Core.SCHEDULE_REGION].Add(Container.Resolve<Views.NewTaskView>(), Core.NEW_TASK_VIEW);
                RegionManager.Regions[Core.SCHEDULE_REGION].Add(Container.Resolve<Views.ColorPickerView>(), Core.COLOR_PICKER_VIEW);
            };
        }
    }
}
