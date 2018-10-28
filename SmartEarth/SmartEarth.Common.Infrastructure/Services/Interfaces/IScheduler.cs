using SmartEarth.Common.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartEarth.Common.Infrastructure.Services.Interfaces
{
    public interface IScheduler
    {
        event EventHandler Loaded;
        bool UserWaiting { get; }
        bool IsBusy { get; }
        Calender Schedule { get; }
        void ImplementTask(SmartEarthTask task);
        void StartNow(SmartEarthTask task);
        Task ScheduleTask(SmartEarthTask task);
        Task Load();
        Task Unload();
    }
}
