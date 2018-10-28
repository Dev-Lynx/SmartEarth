using SmartEarth.Common.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartEarth.Common.Infrastructure.Resources.Interfaces
{
    public interface ICalender
    {
        int StartYear { get; }
        int EndYear { get; }

        CalenderView Context { get; }
        Day CurrentDay { get; }
    }
}
