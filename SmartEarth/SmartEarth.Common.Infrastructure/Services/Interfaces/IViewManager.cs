using SmartEarth.Common.Infrastructure.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartEarth.Common.Infrastructure.Services.Interfaces
{
    public interface IViewManager
    {
        /*
        double ZoomScale { get; }
        double ScrollOffsetX { get; }
        double ScrollOffsetY { get; }
        double ScrollableWidth { get; set; }
        double ScrollableHeight { get; set; }  
        */

        IViewable View { get; }
        ObservableCollection<IViewable> Recent { get; }


        void SetView(IViewable view);
        void LoadRecent();
    }
}
