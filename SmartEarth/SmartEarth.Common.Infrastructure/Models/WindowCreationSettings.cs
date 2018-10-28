using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SmartEarth.Common.Infrastructure.Models
{
    public class WindowCreationSettings
    {
        #region Properties
        public string Title { get; set; }
        public SizeToContent SizeToContent { get; set; } = SizeToContent.Manual;
        public WindowStartupLocation StartupLocation { get; set; } = WindowStartupLocation.Manual;
        public bool RestrictResizing { get; set; } = false;
        public bool Topmost { get; set; }
        public bool IsModal { get; set; }
        public Style Style { get; set; }
        public object DataContext { get; set; }
        public Window Owner { get; set; }
        #endregion
    }
}
