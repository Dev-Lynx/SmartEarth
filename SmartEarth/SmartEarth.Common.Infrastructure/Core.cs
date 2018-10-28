using NLog;
using NLog.Config;
using NLog.Targets;
using SmartEarth.Common.Infrastructure.Extensions;
using SmartEarth.Common.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace SmartEarth.Common.Infrastructure
{
    public static class Core
    {
        #region Properties
        public static Logger Log { get; } = LogManager.GetCurrentClassLogger();
        static string[] Arguments { get; set; } = new string[0];
        static bool IsMainInstance { get; set; }
        static Mutex Instance { get; set; }
        static Thread ListenServer { get; set; }
        static bool Initialized { get; set; }

        public static Dispatcher Dispatcher => Application.Current.Dispatcher;
        public static ResourceDictionary Resources => Application.Current.Resources;

        #region Solids
        public const string PRODUCT_NAME = "SmartEarth";
        public const string AUTHOR = "Prince Owen";

        #region Prism Constants

        #region Modules
        #endregion

        #region Regions
        public const string MENU_REGION = "Menu Region";
        public const string MAIN_REGION = "Main Region";
        public const string HOME_REGION = "Home Region";
        public const string SCHEDULE_REGION = "Schedule Region";
        #endregion

        #region Views
        public const string MENU_VIEW = "Menu View";
        public const string HOME_VIEW = "Home View";
        public const string RECENT_VIEW = "Recent View";
        public const string IMAGE_VIEW = "Image View";
        public const string SCHEDULE_VIEW = "Schedule View";
        public const string MONTH_SCHEDULE_VIEW = "Month Schedule View";
        public const string DAY_SCHEDULE_VIEW = "Day Schedule View";
        public const string NEW_TASK_VIEW = "New Task View";
        public const string COLOR_PICKER_VIEW = "Color Picker View";
        public const string LOADING_VIEW = "Loading View";
        #endregion

        #endregion

        #region Directories
        public readonly static string SYSTEM_DATA_DIR = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        public readonly static string BASE_DIR = Directory.GetCurrentDirectory();
        public readonly static string WORK_DIR = Path.Combine(SYSTEM_DATA_DIR, PRODUCT_NAME);
        public readonly static string TEMP_DIR = Path.Combine(WORK_DIR, "Temp");
        public readonly static string DATABASE_DIR = Path.Combine(WORK_DIR, "Data");
        public readonly static string LOG_DIR = Path.Combine(WORK_DIR, "Logs");
        public readonly static string PROJECT_DIR = Path.Combine(DATABASE_DIR, "Projects");
        #endregion

        #region Paths

        #region Names
        public const string SERVER_NAME = "SmartEarth Server";
        public const string ERROR_LOG_NAME = "Errors";
        public static readonly string CONFIGURAION_PATH = Path.Combine(DATABASE_DIR, "config.xml");

        public static readonly string ERROR_LOG_PATH = Path.Combine(LOG_DIR, ERROR_LOG_NAME + ".log");
        public const string CONSOLE_LOG_NAME = "console-debugger";
        public const string LOG_LAYOUT = "${longdate}|${uppercase:${level}}| ${message}";
        public const string DATABASE_SERVER_NAME = "localhost";
        public const int DATABASE_PORT = 4532;
        public const string DATABASE_USERNAME = "SmartEarth";
        public const string DATABASE_PASSWORD = "Invalid";
        public const string SMARTEARTH_TASK_DATABASE = "SmartEarthTasks";

        public const string SCHEDULED_TASK_DATABASE_DOCUMENT = "SmartEarthScheduledTask";
        public const string COMPLETED_TASK_DATABASE_DOCUMENT = "SmartEarthCompletedTask";
        #endregion

        public enum StartupArguments { SILENT, DISPLAY }

        public readonly static string[] STARTUP_ARGUMENTS = new string[]
        {
            "--silent", "--display"
        };

        public static readonly string LITE_DATABASE_PATH = Path.Combine(DATABASE_DIR, "Data.db");
        public static readonly string GOOGLE_EARTH_X64_PATH = Path.Combine(Environment.ExpandEnvironmentVariables("%ProgramW6432%"), @"Google\Google Earth Pro\client\googleearth.exe");

        #endregion

        #endregion

        #endregion

        #region Methods

        public static void Initialize(string[] arguments = null)
        {
            if (Initialized) return;
            Initialized = true;

            if (arguments != null)
                Arguments = arguments;
            ConfigureLogger();
#if DEBUG
            // Register and Initialize the Console Debugger
            Trace.Listeners.Add(new ConsoleTraceListener(true));
            Debug.Listeners.Add(new ConsoleTraceListener(true));
            ConsoleManager.Show();

            Log.Info("Welcome to the {0} Debugger", PRODUCT_NAME);
#endif

            if (IsMainInstance) ParseArguments();
            Win32Manager.Initialize();
            CreateDirectories(Core.WORK_DIR, Core.DATABASE_DIR, Core.LOG_DIR, Core.TEMP_DIR);
        }

        /// <summary>
        /// Easy and safe way to create multiple directories. 
        /// </summary>
        /// <param name="directories">The set of directories to create</param>
        public static void CreateDirectories(params string[] directories)
        {
            if (directories == null || directories.Length <= 0) return;

            foreach (var directory in directories)
                try
                {
                    if (Directory.Exists(directory)) continue;

                    Directory.CreateDirectory(directory);
                    Log.Info("A new directory has been created ({0})", directory);
                }
                catch (Exception e)
                {
                    Log.Error("Error while creating directory {0} - {1}", directory, e);
                }
        }
        
        static void ConfigureLogger()
        {
            var config = new LoggingConfiguration();

#if DEBUG
            var debugConsole = new ConsoleTarget()
            {
                Name = Core.CONSOLE_LOG_NAME, 
                Layout = Core.LOG_LAYOUT, 
                Header = "SmartEarth Debugger"
            };

            var debugRule = new LoggingRule("*", LogLevel.Debug, debugConsole);
            config.LoggingRules.Add(debugRule);
#endif

            var errorFileTarget = new FileTarget()
            {
                Name = Core.ERROR_LOG_NAME,
                FileName = Core.ERROR_LOG_PATH,
                Layout = Core.LOG_LAYOUT
            };

            config.AddTarget(errorFileTarget);

            var errorRule = new LoggingRule("*", LogLevel.Error, errorFileTarget);
            config.LoggingRules.Add(errorRule);

            LogManager.Configuration = config;

            LogManager.ReconfigExistingLoggers();
        }

        static async void AnalyzeInstance()
        {
            try
            {
                Instance = new Mutex(true, Assembly.GetExecutingAssembly().GetName().Name, out bool newInstance);
                IsMainInstance = newInstance;

                if (IsMainInstance)
                {
                    ListenServer = new Thread(Listen);
                    ListenServer.IsBackground = true;
                    ListenServer.Start();
                }
                else
                {
                    Instance.Dispose();

                    var args = new string[Arguments.Length + 1];
                    args[0] = Core.GetStartupArgument(Core.StartupArguments.DISPLAY);

                    for (int i = 0; i < args.Length; i++)
                        args[i] = Arguments[i - 1];

                    await SendMessage(args);
                    Application.Current.Shutdown();
                }
            }
            catch { }
        }

        static async void Listen()
        {
            while (true)
            {
                try
                {
                    using (var server = new NamedPipeServerStream(Core.SERVER_NAME))
                    using (var reader = new StreamReader(server))
                    {
                        await server.WaitForConnectionAsync();

                        var args = new List<string>();

                        while (!reader.EndOfStream)
                        {
                            string buffer = string.Empty;
                            char c = '\0';
                            bool allowSpace = false;

                            while (!reader.EndOfStream)
                            {
                                c = (char)reader.Read();

                                if (allowSpace && c == '"') allowSpace = false;
                                else if (c == '"') allowSpace = true;

                                if (c == '"') continue;
                                if (c == ' ' && !allowSpace) break;

                                buffer += c;
                            }

                            args.Add(buffer);
                        }
                        Application.Current.Dispatcher.Invoke(() => Core.ParseArguments(args.ToArray()));
                    }
                }
                catch (Exception ex)
                {
                    Core.Log.Error(ex, "An unexpected error occured in the server");
                }
            }
        }

        static async Task<bool> SendMessage(params string[] messages)
        {
            if (messages == null) return false;

            try
            {
                using (var mutex = Mutex.OpenExisting(Assembly.GetExecutingAssembly().GetName().Name))
                using (var client = new NamedPipeClientStream(Core.SERVER_NAME))
                using (var writer = new StreamWriter(client))
                {
                    while (!client.IsConnected)
                        await client.ConnectAsync();

                    for (int i = 0; i < messages.Length; i++)
                    {
                        writer.Write("\"{0}\"", messages[i]);

                        if (i < messages.Length - 1) writer.Write(" ");
                    }
                }
            }
            catch (Exception e)
            {
                Core.Log.Error(e);
                return false;
            }
            return true;
        }

        public static string GetStartupArgument(StartupArguments argument)
        {
            try { return STARTUP_ARGUMENTS[(int)argument]; }
            catch (Exception) { return ""; }
        }

        static void ParseArguments(params string[] arguments)
        {

        }
        #endregion
    }
}
