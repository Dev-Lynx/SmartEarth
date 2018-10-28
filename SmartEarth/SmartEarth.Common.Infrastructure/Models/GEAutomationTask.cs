using NLog;
using Prism.Logging;
using Quartz;
using SmartEarth.Common.Infrastructure.Extensions;
using SmartEarth.Common.Infrastructure.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WindowsInput;
using WindowsInput.Native;

namespace SmartEarth.Common.Infrastructure.Models
{
    [Serializable]
    public class GEAutomationTask : SmartEarthTask, IViewable
    {
        #region Properties

        #region Statics
        static readonly TimeSpan WaitTimeout = new TimeSpan(0, 0, 30);
        #endregion


        Location _location = new Location();
        public Location Location { get => _location; set => SetProperty(ref _location, value); }

        Configuration _configuration;
        public override Configuration Configuration { get => _configuration; protected set => SetProperty(ref _configuration, value); }

        protected override Dictionary<string, object> Data
        {
            get
            {
                var data = base.Data;
                data.Add(nameof(Location), Location);
                return data;
            }
        }

        SmartEarthImage _image = new SmartEarthImage();
        public SmartEarthImage Image { get => _image; set => SetProperty(ref _image, value); }

        bool _expanded = false;
        public bool Expanded { get => _expanded; set => SetProperty(ref _expanded, value); }

        #region Statics
        const string GOOGLE_EARTH_STARTUP_TIPS = "Start-up Tips";
        const string GOOGLE_EARTH_NEW_PLACEMARK_WINDOW = "Google Earth - New Placemark";
        const string GOOGLE_EARTH_SAVE_TAB = "earth::modules::print::PrintToolbarClassWindow";
        #endregion

        #region Event Handlers
        EventHandler ForegroundWindowHandler;
        #endregion

        #region Internals
        ManualResetEvent Busy { get; } = new ManualResetEvent(true);
        InputSimulator Simulator { get; set; }
        IKeyboardSimulator Keyboard => Simulator.Keyboard;
        Logger Logger { get; } = LogManager.GetCurrentClassLogger();
        IntPtr CurrentHandle { get; set; }
        Process Earth { get; set; }
        IntPtr SaveTab { get; set; }
        string CurrentPath { get; set; }
        #endregion

        #endregion

        #region Constructors
        public GEAutomationTask()
        {

        }
        #endregion

        #region Methods

        #region IViewable Implementation
        public IPresentable Replicate() => new ScheduleTask(this);
        public IPresentable ReplicateDue(DateTime due) => new ScheduleTask(this) { Due = due };
        public async Task Load(bool crop = false, Int32Rect rect = default(Int32Rect)) => await Application.Current.Dispatcher.InvokeAsync(() => Image.Load(crop, rect));
        public async Task UnloadSource() => await Application.Current.Dispatcher.InvokeAsync(() => Image.UnloadSource());
        public async Task UnloadThumb() => await Application.Current.Dispatcher.InvokeAsync(() => Image.UnloadThumb());
        public async Task Reset() => await Application.Current.Dispatcher.InvokeAsync(() => Image.Reset());
        #endregion


        public void Initialize()
        {
            Win32Manager.ForegroundWindowChanged += ForegroundWindowHandler = (s, e) =>
            {
                //if (CurrentHandle != IntPtr.Zero && CurrentHandle != Win32Manager.ForegroundWindow)
                    //Win32Manager.ForegroundWindow = CurrentHandle;
                Core.Log.Debug("Foreground Window has been changed to {0}", Win32Manager.ForegroundWindow);
            };
            

            Simulator = new InputSimulator();
        }

        public void Cleanup()
        {
            Win32Manager.ForegroundWindowChanged -= ForegroundWindowHandler;
        }

        #region Overrides
        public override async Task Start(Configuration configuration)
        {
            Initialize();
            Configuration = new Configuration(configuration);

            // Launch the process
            if (!await Launch())
            {
                Update(0, "Failed to connect to Google Earth", TaskStatus.Failed);
                await Task.Delay(5000);
                return;
            }

            // Open the placemark window and input the values
            if (!await CreatePlacemark())
            {
                Update(0, "Failed to create placemark", TaskStatus.Failed);
                await Task.Delay(5000);
                return;
            }

            if (!await SaveTask())
            {
                Update(0, "Failed to save the task image", TaskStatus.Failed);
                await Task.Delay(5000);
                return;
            }

            if (!await AnalyseImage())
            {
                Update(0, "Failed to analyse the image", TaskStatus.Failed);
                await Task.Delay(5000);
                return;
            }

            
            Status = TaskStatus.Completed;
        }

        public override Task Pause()
        {
            Busy.Reset();
            
            return Task.CompletedTask;
        }

        public override Task Resume()
        {
            Busy.Set();
            return Task.CompletedTask;
        }

        public override Task Stop()
        {
            CancellationSource.Cancel();
            return Task.CompletedTask;
        }

        public override IJobDetail CompileDetail(Services.Interfaces.IScheduler scheduler)
        {
            var data = new JobDataMap();
            
            foreach (var key in Data.Keys) data.Add(key, Data[key]);
            data.Add(nameof(scheduler), scheduler);
            return JobBuilder.Create<GEAutomationTask>().UsingJobData(data).Build();
        }

        protected override void LoadDetails(JobDataMap data, bool implement = false)
        {
            base.LoadDetails(data);

            Services.Interfaces.IScheduler scheduler = null;
            try
            {
                Location = (Location)data[nameof(Location)];
                scheduler = (Services.Interfaces.IScheduler)data[nameof(scheduler)];
            }
            catch { return; }
            if (implement)
                scheduler.ImplementTask(this);
        }

        public override SmartEarthTask Duplicate()
        {
            return new GEAutomationTask()
            {
                Name = this.Name,
                Description = this.Description,
                Signature = this.Signature,
                Color = this.Color,
                Location = new Location(this.Location),
                Status = this.Status,
                TaskInformation = this.TaskInformation,
                Due = this.Due,
                Interval = this.Interval
            };
        }

        void Focus()
        {
            /*
            if (old != IntPtr.Zero)
                Win32Manager.AttachThreadInput(Win32Manager.GetWindowThreadProcessId(old, out uint lpdwProcessId), Win32Manager.GetCurrentThreadId(), false);

            Win32Manager.AttachThreadInput(Win32Manager.GetWindowThreadProcessId(CurrentHandle, out uint lpdwProcess), Win32Manager.GetCurrentThreadId(), true);
            */

            Win32Manager.ShowWindow(Earth.MainWindowHandle, Win32Manager.SW_SHOWMAXIMIZED);
            Win32Manager.ForegroundWindow = CurrentHandle;
            Win32Manager.ForceWindowToForeground(CurrentHandle);
        }
        #endregion

        #region Work
        async Task<bool> Launch()
        {
            LaunchStart:
            Logger.Debug("A google automation task was just started.");
            Update(10, "Connecting To Google Earth...");

            if (CancellationSource.IsCancellationRequested)
            {
                Update(0, "The task was cancelled", TaskStatus.Failed);
                return false;
            }
                

            var startInfo = new ProcessStartInfo()
            {
                UseShellExecute = true,
                WorkingDirectory = Path.GetDirectoryName(Configuration.GoogleEarthPath),
                FileName = Configuration.GoogleEarthPath
            };

            Earth = Win32Manager.AttachOrLaunch(startInfo);

            if (Earth == null || Earth.HasExited)
            {
                if (Earth.AttemptClose())
                    goto LaunchStart;
                return false;
            }

            try { var pid = Earth.Id; }
            catch
            {
                if (Earth.AttemptClose())
                    goto LaunchStart;
                return false;
            }

            Earth.EnableRaisingEvents = true;
            Earth.Exited += (s, ex) =>
            {
                Status = TaskStatus.Failed;
                CancellationSource.Cancel();
                
                Logger.Debug("Google Earth has exited...");
                return;
            };

            Update(15, "Connecting To Google Earth...");

            if (!await Earth.WaitTillIdle(WaitTimeout))
                return false;

            Update(20, "Connecting To Google Earth...");
            return true;
        }

        async Task<bool> CreatePlacemark()
        {
            const int maxAttempts = 5;
            int attempts = 0;

            StartCreation:
            attempts++;
            CurrentHandle = Earth.MainWindowHandle;
            Focus();

            Win32Manager.ShowWindow(CurrentHandle, Win32Manager.SW_SHOWMAXIMIZED);
            var startupTips = await Earth.WaitForThreadWindowByName(GOOGLE_EARTH_STARTUP_TIPS, CancellationSource.Token, WaitTimeout);
            if (startupTips != IntPtr.Zero) startupTips.Close();

            if (CancellationSource.IsCancellationRequested)
                return false;

            Update(20, "Creating A New Placemark...");

            Focus();


            // Press Ctrl + Alt + P
            // Open the new placemark window
            Keyboard.KeyDown(VirtualKeyCode.CONTROL);
            Keyboard.KeyDown(VirtualKeyCode.SHIFT);
            Keyboard.KeyPress(VirtualKeyCode.VK_P);
            Keyboard.KeyUp(VirtualKeyCode.CONTROL);
            Keyboard.KeyUp(VirtualKeyCode.SHIFT);

            var newPlacemark = await Earth.WaitForThreadWindowByName(GOOGLE_EARTH_NEW_PLACEMARK_WINDOW, CancellationSource.Token, WaitTimeout);
            if (newPlacemark == IntPtr.Zero)
            {
                if (attempts > maxAttempts)
                {
                    Update(0, "Failed to create a new placemark");
                    return false;
                }
                Update(15, "An error has occured, Attempting again...");
                goto StartCreation;
            }

            CurrentHandle = newPlacemark;

            //await Earth.WaitTillIdle(WaitTimeout);
            Focus();


            Keyboard.KeyDown(VirtualKeyCode.CONTROL);
            Keyboard.KeyPress(VirtualKeyCode.VK_A);
            Keyboard.KeyUp(VirtualKeyCode.CONTROL);
            if (!string.IsNullOrEmpty(Name))
                Keyboard.TextEntry(Name);

            Focus();
            int pressCount = 4;
            for (int i = 0; i < pressCount; i++)
                Keyboard.KeyPress(VirtualKeyCode.TAB);

            Focus();
            pressCount = 2;
            for (int i = 0; i < pressCount; i++)
                Keyboard.KeyPress(VirtualKeyCode.RIGHT);
            Focus();
            for (int i = 0; i < pressCount; i++)
                Keyboard.KeyPress(VirtualKeyCode.TAB);

            Focus();
            // Enter Latitude...
            Keyboard.KeyDown(VirtualKeyCode.CONTROL);
            Keyboard.KeyPress(VirtualKeyCode.VK_A);
            Keyboard.KeyUp(VirtualKeyCode.CONTROL);
            Keyboard.TextEntry(Location.Coordinates.Latitude.ToDouble().ToString());

            Focus();
            // Enter Longitude...
            Keyboard.KeyPress(VirtualKeyCode.TAB);
            Keyboard.KeyDown(VirtualKeyCode.CONTROL);
            Keyboard.KeyPress(VirtualKeyCode.VK_A);
            Keyboard.KeyUp(VirtualKeyCode.CONTROL);
            Keyboard.TextEntry(Location.Coordinates.Longitude.ToDouble().ToString());

            Focus();
            // Enter Range
            Keyboard.KeyPress(VirtualKeyCode.TAB);
            Keyboard.KeyDown(VirtualKeyCode.CONTROL);
            Keyboard.KeyPress(VirtualKeyCode.VK_A);
            Keyboard.KeyUp(VirtualKeyCode.CONTROL);
            Keyboard.TextEntry(Location.Range.ToString());

            Focus();
            // Enter Heading
            Keyboard.KeyPress(VirtualKeyCode.TAB);
            Keyboard.KeyDown(VirtualKeyCode.CONTROL);
            Keyboard.KeyPress(VirtualKeyCode.VK_A);
            Keyboard.KeyUp(VirtualKeyCode.CONTROL);
            Keyboard.TextEntry(Location.Heading.ToString());

            Focus();
            // Enter Tilt
            Keyboard.KeyDown(VirtualKeyCode.CONTROL);
            Keyboard.KeyPress(VirtualKeyCode.VK_A);
            Keyboard.KeyUp(VirtualKeyCode.CONTROL);
            Keyboard.KeyPress(VirtualKeyCode.TAB);
            Keyboard.TextEntry(Location.Tilt.ToString());

            Focus();
            // Enter Date
            Keyboard.KeyPress(VirtualKeyCode.TAB);
            Keyboard.KeyPress(VirtualKeyCode.DOWN);

            Focus();
            // Navigate backwards to the date bar
            pressCount = 12;
            Keyboard.KeyDown(VirtualKeyCode.SHIFT);
            for (int i = 0; i < pressCount; i++)
                Keyboard.KeyPress(VirtualKeyCode.TAB);
            Keyboard.KeyUp(VirtualKeyCode.SHIFT);



            var date = DateTime.Now;

            Focus();
            Keyboard.TextEntry(date.ToString("MMM", CultureInfo.InvariantCulture));
            Keyboard.TextEntry(date.Day.ToString());
            Keyboard.TextEntry(date.Year.ToString());
            Keyboard.TextEntry(date.Hour.ToString());
            Keyboard.KeyPress(VirtualKeyCode.TAB);
            Keyboard.TextEntry(date.Minute.ToString());
            Keyboard.TextEntry(date.ToString("tt", CultureInfo.InvariantCulture));

            Keyboard.KeyPress(VirtualKeyCode.RETURN);
            return true;
        }

        void Update(double progress, string message, TaskStatus status = TaskStatus.Running)
        {
            Status = status;
            Logger.Debug(message);
            Logs.Add(new Log(message));
            TaskInformation = message;
            Progress = progress;
        }


        async Task<bool> SaveTask()
        {
            if (CancellationSource.Token.IsCancellationRequested)
            {
                Update(0, "The task has been cancelled");
                return false;
            }

            const int maxAttempts = 5;
            int attempts = 0;


            SaveStart:

            attempts++;
            Update(40, "Waiting for Automation Image to load...");
            Win32Manager.ShowWindow(Earth.MainWindowHandle, Win32Manager.SW_SHOWMAXIMIZED);

            SaveTab = await Earth.MainWindowHandle.WaitForChildWindowByName(GOOGLE_EARTH_SAVE_TAB, CancellationSource.Token, WaitTimeout);
            if (SaveTab != IntPtr.Zero && Win32Manager.IsWindowVisible(SaveTab)) SaveTab.Close();

            CurrentHandle = Earth.MainWindowHandle;
            Focus();
            Keyboard.ModifiedKeyStroke(new VirtualKeyCode[] { VirtualKeyCode.LMENU, VirtualKeyCode.CONTROL }, VirtualKeyCode.VK_S);

            Update(45, "Waiting for Automation Image to load...");

            SaveTab = await Earth.MainWindowHandle.WaitForChildWindowByName(GOOGLE_EARTH_SAVE_TAB, CancellationSource.Token, WaitTimeout);

            if (SaveTab == IntPtr.Zero)
            {
                if (attempts >= maxAttempts)
                {
                    Update(0, "Failed to save the image...");
                    return false;
                }
                Logger.Debug("Restarting Save Sequence..");
                goto SaveStart;
            }

            CurrentHandle = SaveTab;
            Win32Manager.ForegroundWindow = SaveTab;

            Focus();

            for (int i = 0; i < 3; i++)
                Keyboard.KeyPress(VirtualKeyCode.TAB);
            Keyboard.KeyPress(VirtualKeyCode.SPACE);


            Update(60, "Attempting to save image...");

            await Earth.WaitTillIdle(WaitTimeout, CancellationSource.Token);
            var saveDialog = await Earth.WaitForThreadWindowByName("Save As", CancellationSource.Token, TimeSpan.FromSeconds(10));

            if (saveDialog == IntPtr.Zero)
            {
                if (attempts >= maxAttempts)
                {
                    Update(0, "Failed to save the image...");
                    return false;
                }

                var newPath = Earth.FindThreadWindowByName("Google Earth - New Path");
                if (newPath != IntPtr.Zero) newPath.Close();

                goto SaveStart;
            }


            CurrentHandle = saveDialog;
            var box = CurrentHandle.FindChildWindowByName("Edit");
            box.SetFocus();

            Update(70, "Attempting to save image...");

            CurrentPath = Path.Combine(Core.TEMP_DIR, Guid.NewGuid().ToString() + ".jpg");
            Keyboard.TextEntry(CurrentPath);

            Core.Log.Debug(Win32Manager.GetWindowText(box));
            Keyboard.KeyPress(VirtualKeyCode.RETURN);

            Status = TaskStatus.Completed;
            return true;
        }

        async Task<bool> AnalyseImage()
        {
            Update(75, "Finalizing Automation...");
            int pressCount = 0;
            Win32Manager.ShowWindow(Earth.MainWindowHandle, Win32Manager.SW_SHOWMAXIMIZED);
            CurrentHandle = SaveTab;
            Win32Manager.ForegroundWindow = SaveTab;
            Focus();

            // Navigate backwards to the beginning...
            pressCount = 3;
            Keyboard.KeyDown(VirtualKeyCode.SHIFT);
            for (int i = 0; i < pressCount; i++)
                Keyboard.KeyPress(VirtualKeyCode.TAB);
            Keyboard.KeyUp(VirtualKeyCode.SHIFT);

            SaveTab.Close();
            Update(80, "Analyzing Automation Image...");

            CurrentPath = await Watch(CurrentPath);
            Update(85, "Analyzing Automation Image...");

            string folder = Name;
            if (string.IsNullOrWhiteSpace(folder)) folder = "Unnamed Projects";
            folder = Path.Combine(Core.PROJECT_DIR, folder, DateTime.Now.ToLongDateString());

            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            string destination = Path.Combine(folder, "Image 1");

            for (int i = 2; File.Exists(destination); i++)
                destination = Path.Combine(folder, "Image " + i);

            CurrentPath = await Watch(CurrentPath);

            if (string.IsNullOrWhiteSpace(CurrentPath))
            {
                Update(0, "Failed to find automation image");
                return false;
            }

            Update(90, "Saving Image...");

            try
            {
                int read = -1;
                byte[] buffer = new byte[4096];

                // Copy the file from the temporary path to the new one...
                using (FileStream input = new FileStream(CurrentPath, FileMode.Open, FileAccess.Read))
                using (FileStream output = new FileStream(destination, FileMode.Create, FileAccess.Write))
                    while ((read = input.Read(buffer, 0, buffer.Length)) > 0) output.Write(buffer, 0, read);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to access the image ({0}) \n {1}", CurrentPath, ex);
                Update(0, "An error ocurred while analysing the image");
                return false;
            }

            Image = new SmartEarthImage(destination);
            return true;
        }

        async Task<string> Watch(string path)
        {
            if (File.Exists(path)) return path;
            using (FileSystemWatcher watcher = new FileSystemWatcher(Core.TEMP_DIR, "*"))
            {
                bool found = false;
                watcher.NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.FileName |
                    NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.Size |
                    NotifyFilters.Security;
                watcher.EnableRaisingEvents = true;
                watcher.Created += (s, e) =>
                {
                    if (path == e.FullPath)
                        found = true;
                };


                for (int i = 0; i < 60 && !found; i++)
                    await Task.Delay(1000);

                if (File.Exists(path)) return path;
                else return string.Empty;
            }
        }
        #endregion

        #endregion
    }
}
