using SmartEarth.Common.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartEarth.Common.Infrastructure.Models.Interfaces
{
    public interface ITask
    {
        TaskStatus Status { get; }
        Task Start(Configuration configuration);
    }
}
