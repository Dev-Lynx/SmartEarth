using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using SmartEarth.Common.Infrastructure.Extensions;
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
    /// Interaction logic for TaskProgressView.xaml
    /// </summary>
    public partial class TaskProgressView : UserControl
    {
        #region Properties
        IUnityContainer Container { get; }
        #endregion

        public TaskProgressView()
        {
            InitializeComponent();
            Container = ServiceLocator.Current.GetInstance<IUnityContainer>();

            RoutedEventHandler loaded = null;
            Loaded += loaded = (s, e) =>
            {
                Loaded -= loaded;
                var vm = Container.Resolve<ViewModels.TaskProgressViewModel>();
                vm.LoadWindow(this.FindAncestor<Window>());
                DataContext = vm;
            };
        }
    }
}
