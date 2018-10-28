using SmartEarth.Common.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartEarth.Common.Infrastructure.Services.Interfaces
{
    public interface IDatabaseManager
    {
        bool Initialized { get; }

        ObservableCollection<SmartEarthTask> ScheduledTasks { get; }
        ObservableCollection<SmartEarthTask> CompletedTasks { get; }
        ObservableCollection<ColorBox> Colors { get; }

        bool RemoveScheduledTask(SmartEarthTask task);

        bool SaveScheduledTask(SmartEarthTask task);
        bool SaveCompletedTask(SmartEarthTask task);
    }
}
