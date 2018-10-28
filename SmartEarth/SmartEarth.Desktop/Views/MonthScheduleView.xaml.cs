using Microsoft.Practices.Unity;
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
    /// Interaction logic for MonthScheduleView.xaml
    /// </summary>
    public partial class MonthScheduleView : UserControl, IView
    {
        #region Properties
        IUnityContainer Container { get; }
        #endregion

        public MonthScheduleView(IUnityContainer container)
        {
            InitializeComponent();
            Container = container;

            DataContext = Container.Resolve<ViewModels.MonthScheduleViewModel>();

            if (DataContext is IViewAware)
                ((IViewAware)DataContext).View = this;
        }
    }
}
