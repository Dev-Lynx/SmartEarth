using Prism.Logging;
using SmartEarth.Common.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartEarth.Common.Infrastructure.Extensions
{
    public static class LoggerExtensions
    {
        public static void Error(this ILoggerFacade logger, Exception ex)
        {
            if (logger == null || ex == null) return;
            if (logger is NLogger)
                NLogger.Instance.Error(ex);
            else logger.Log(ex.Message, Category.Exception, Priority.High);
        }

        public static void Debug(this ILoggerFacade logger, string message, Priority priority = Priority.Medium)
        {
            logger.Log(message, Category.Debug, priority);
        }

        public static void Debug(this ILoggerFacade logger, string message, params object[] args)
        {
            NLogger.Instance.Debug(message, args);
        }

        public static void Error(this ILoggerFacade logger, string message)
        {
            logger.Log(message, Category.Exception, Priority.High);
        }

        public static void Error(this ILoggerFacade logger, string message, params object[] args)
        {
            NLogger.Instance.Error(message, args);
        }

        public static void Log(this ILoggerFacade logger, string message, params object[] args)
        {
            NLogger.Instance.Info(message, args);
        }
    }
}
