using SmartEarth.Common.Infrastructure.Models;
using SmartEarth.Common.Infrastructure.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartEarth.Common.Infrastructure.Services.Interfaces
{
    public interface ITaskManager
    {
        SmartEarthTask CurrentTask { get; }
        void RegisterTask(ITask task);

        event EventHandler NewTaskRegistered;

        event EventHandler TaskStarted;
        event EventHandler TaskCompleted;
    }
}
