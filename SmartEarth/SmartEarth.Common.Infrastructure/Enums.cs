using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartEarth.Common.Infrastructure
{
    public enum TaskStatus { Pending, Queueing, Running, Missed, Failed, Completed }
    public enum Repetition { Hourly, Daily, Weekly, Monthly, Yearly }
    public enum ExpirationType { Count, Date }
    public enum Meridian { AM, PM }
    public enum Direction { Left, Right, Up, Down }
    public enum NavigateDirection { None, Left, Right, FarLeft, FarRight }
    public enum CalenderView { Hour, Day, Week, Month, Year }
}
