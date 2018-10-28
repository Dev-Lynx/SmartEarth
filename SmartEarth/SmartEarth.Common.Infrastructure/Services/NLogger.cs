using NLog;
using Prism.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartEarth.Common.Infrastructure.Services
{
    public class NLogger : ILoggerFacade
    {
        public static Logger Instance { get { return Core.Log; } }

        public void Log(string message, Category category, Priority priority)
        {
            switch (category)
            {
                case Category.Debug:
                    Instance.Debug(message);
                    break;

                case Category.Exception:
                    Instance.Error(message);
                    break;

                case Category.Info:
                    Instance.Info(message);
                    break;

                case Category.Warn:
                    Instance.Warn(message);
                    break;
            };
        }
    }
}
