using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartEarth.Common.Infrastructure.Models
{
    public class Log
    {
        #region Properties
        public DateTime Time { get; set; } = DateTime.Now;
        public string Message { get; set; } = string.Empty;
        #endregion

        #region Constructors
        public Log() { }
        public Log(string message)
        {
            Message = message;
        }
        #endregion
    }
}
