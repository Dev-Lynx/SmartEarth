using Microsoft.Win32;
using SharpKml.Base;
using SharpKml.Dom;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsInput;
using WindowsInput.Native;

namespace SmartEarth.Demos.Automation
{
    class Program
    {

        #region Win32 Imports

        #region Win32 Constants
        const int SW_SHOWMINIMIZED = 2;
        const int SW_SHOWMAXIMIZED = 3;
        const int BN_CLICKED = 245;
        const UInt32 WM_CLOSE = 0x0010;
        const int WM_GETTEXT = 0x000D;
        #endregion

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString,
    int nMaxCount);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool EnumThreadWindows(int dwThreadId, EnumThreadDelegate lpfn, IntPtr lparam);

        public delegate bool Win32Callback(IntPtr hwnd, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumChildWindows(IntPtr parentHandle, Win32Callback callback, IntPtr lParam);

        /// <summary>
		/// The SendMessage API
		/// </summary>
		/// <param name="hWnd">handle to the required window</param>
		/// <param name="msg">the system/Custom message to send</param>
		/// <param name="wParam">first message parameter</param>
		/// <param name="lParam">second message parameter</param>
		/// <returns></returns>
		[DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(int hWnd, int msg, int wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, StringBuilder lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// The FindWindowEx API
        /// </summary>
        /// <param name="parentHandle">a handle to the parent window </param>
        /// <param name="childAfter">a handle to the child window to start search after</param>
        /// <param name="className">the class name for the window to search for</param>
        /// <param name="windowTitle">the name of the window to search for</param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetFocus(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
        #endregion

        #region Constants

        const string PRODUCT_NAME = "Smart Earth Automation Demo";

        #region Directories
        static readonly string WORK_BASE = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), PRODUCT_NAME);
        static readonly string TEMP_DIR = Path.Combine(WORK_BASE, "Temp");
        #endregion

        #endregion


        const string GoogleEarth = "googleearth";
        const string GOOGLE_EARTH_SAVE_TAB = "earth::modules::print::PrintToolbarClassWindow";
        static readonly string GoogleEarthPath = Path.Combine(Environment.ExpandEnvironmentVariables("%ProgramW6432%"), @"Google\Google Earth Pro\client\googleearth.exe");
        static Process GE { get; set; }

        [STAThreadAttribute]
        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Clear();
            Console.WriteLine("Welcome to Google Earth Automator!");
            Console.WriteLine("Allow me to handle all the Google Earth Hassle :)");


            START:
            Console.WriteLine();
            Console.Write("Please enter the coordinates of the location you would like to visit > ");

            Geospatial.Location desiredLocation = null;
            while (!Geospatial.Location.TryParse(Console.ReadLine(), out desiredLocation))
            {
                Console.WriteLine("The location coordinates you provided does not seem to be in the correct format.");
                Console.WriteLine("Please try again.");

                Console.WriteLine();
                Console.Write("Please enter the coordinates of the location you would like to visit > ");
            }

            var altitude = desiredLocation.Altitude.HasValue ? desiredLocation.Altitude.Value : 0.0;

            if (!Directory.Exists(TEMP_DIR))
                Directory.CreateDirectory(TEMP_DIR);

            var point = new SharpKml.Dom.Point();

            point.Coordinate = new SharpKml.Base.Vector(desiredLocation.Latitude.TotalDegrees, desiredLocation.Longitude.TotalDegrees, altitude);
            Kml kml = new Kml();
            Placemark placemark = new Placemark();
            placemark.Geometry = point;
            kml.Feature = placemark;

            string path = Path.Combine(TEMP_DIR, Guid.NewGuid().ToString() + ".kml");
            using (var file = new FileStream(path, FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(file))
            {
                Serializer serializer = new Serializer();
                serializer.Serialize(kml);
                writer.WriteLine(serializer.Xml);
                writer.Flush();
            }

            Console.WriteLine("That would be no problem...");
            Console.WriteLine("What would you name would like to save the image with?");
            Console.Write("Please enter the full path > ");



            string savePath = Console.ReadLine();
            string saveDir = string.Empty;

            try { saveDir = Path.GetDirectoryName(savePath); } catch { saveDir = string.Empty; }
            while (!Directory.Exists((saveDir)))
            {
                if (string.IsNullOrWhiteSpace(saveDir))
                {
                    saveDir = Directory.GetCurrentDirectory();
                    savePath = Path.Combine(saveDir, savePath);
                }
                try { Directory.CreateDirectory(saveDir); }
                catch
                {
                    Console.WriteLine("Directory you provided to save the file does not exist, cannot be created, Please input another file path.");
                    savePath = Console.ReadLine();
                    try { saveDir = Path.GetDirectoryName(savePath); } catch { saveDir = string.Empty; }
                }
            }

            if (Path.GetExtension(savePath) != ".jpg")
                savePath += ".jpg";


            var startInfo = new ProcessStartInfo(GoogleEarthPath)
            {
                
            };
           

            var processes = Process.GetProcesses();
            bool foundIt = false;

            for (int i = 0; i < processes.Length; i++)
            {
                //Console.WriteLine(processes[i].ProcessName);
                if (!processes[i].ProcessName.Contains(GoogleEarth))
                    continue;

                GE = Process.GetProcessById(processes[i].Id);
                foundIt = true;

                Console.WriteLine("Google Earth is already running...");
                break;
            }

            for (int i = 0; i < 3; i++) Console.WriteLine();

            if (!foundIt)
            {
                var gePath = GoogleEarthPath;
                while (!File.Exists(gePath))
                {
                    Console.WriteLine("Google Earth was not found, Please specify the path for the application.");
                    gePath = Console.ReadLine();
                }

                //Thread.Sleep(10000);
                var info = new ProcessStartInfo(path)
                {
                    Arguments = $"\"{path}\"",
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(path),
                    FileName = gePath,
                    Verb = "OPEN"
                };
                GE = Process.Start(info);
            }
            else
            {
                var info = new ProcessStartInfo(path)
                {
                    Arguments = $"\"{path}\"",
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(path),
                    FileName = GoogleEarthPath,
                    Verb = "OPEN"
                };
                Process.Start(info);
            }



            Console.WriteLine("Waiting for Google Earth to load....");
            while (GE.MainWindowHandle == IntPtr.Zero)
                Thread.Sleep(1000);

            bool geLoading = true;
            //CPUStatus status = new CPUStatus(GE);

            CpuUsage usage = new CpuUsage(GE);
            var loadTimer = new System.Timers.Timer()
            {
                Interval = 500,
                Enabled = true,
                AutoReset = true
            };

            int lowCount = 0;

            loadTimer.Elapsed += (s, e) =>
            {
                Console.Write("\rGoogle Earth CPU Usage: {0}%     ", usage.Usage);
                var value = usage.GetUsage();

                if (value < 5) lowCount++;
                else lowCount = 0;

                if (lowCount > 7) geLoading = false;
            };

            loadTimer.Start();


            /*
            CPUTracker.TrackProcess(GE);

            CPUTracker.CPUValuesUpdated += (s, e) =>
            {
                Console.Write("\rGoogle Earth CPU Usage: {0}    ", e.ToString());

                if (count > 100)
                    geLoading = false;
                count++;
            };
            */

            while (geLoading)
                await Task.Delay(1000);

            loadTimer.Stop();
            Console.WriteLine();
            Console.WriteLine("Google Earth has loaded successfully...");


            //CPUTracker.StopTracking();

            ShowWindow(GE.MainWindowHandle, SW_SHOWMAXIMIZED);
            SetForegroundWindow(GE.MainWindowHandle);

            /*
            // Wait for the application to become the main window...
            while (!GE.MainWindowHandle.Equals(GetForegroundWindow()))
            {
                GE.Refresh();
                Thread.Sleep(2000);
            }
            */

            // Iterate through the processes windows
            foreach (ProcessThread thread in GE.Threads)
                EnumThreadWindows(thread.Id
                    , (hWnd, lParam) =>
                    {
                        var msg = new StringBuilder(1000);
                        SendMessage(hWnd, WM_GETTEXT, msg.Capacity, msg);

                        // If Startup Tips is open, send a close message
                        if (msg.ToString() == "Start-up Tips")
                            SendMessage(hWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);

                        return true;

                    }, IntPtr.Zero);


            // toolbarFrameWindow
            // earth::modules::print::PrintToolbarClassWindow
            var mainWindowHandle = GE.MainWindowHandle;
            var saveTab = FindWindow(GE.MainWindowHandle
                , (ptr => GetWindowText(ptr)
                == "earth::modules::print::PrintToolbarClassWindow"));


            InputSimulator simulator = new InputSimulator();
            var keyboard = new KeyboardSimulator(simulator);
            if (saveTab == IntPtr.Zero || !IsWindowVisible(saveTab))
            {
                keyboard.ModifiedKeyStroke(new VirtualKeyCode[] { VirtualKeyCode.LMENU, VirtualKeyCode.LCONTROL }, VirtualKeyCode.VK_S);
               saveTab =  FindWindow(GE.MainWindowHandle
                , (ptr => GetWindowText(ptr)
                == "earth::modules::print::PrintToolbarClassWindow"));
            }
            
            //keyboard.ModifiedKeyStroke(new VirtualKeyCode[] { VirtualKeyCode.LMENU, VirtualKeyCode.LCONTROL }, VirtualKeyCode.VK_C);
            /*
            Thread.Sleep(1000);
            var saveButton = FindWindowEx(GE.MainWindowHandle, IntPtr.Zero, "Qt5QWindowIcon", "earth::modules::print::PrintToolbarClassWindow");
            Console.WriteLine(saveButton);
            SendMessage((int)saveButton, BN_CLICKED, 0, IntPtr.Zero);
            */

            Thread.Sleep(2000);
            Console.WriteLine("Searching through all windows in the process");
            //PrintAllChildWindows(GE.MainWindowHandle);


            if (!SetForegroundWindow(saveTab))
                Console.WriteLine("Failed to set save tab as the foreground window");

            // Press 3 tabs to navigate to the save button
            for (int i = 0; i < 3; i++)
                keyboard.KeyPress(VirtualKeyCode.TAB);

            keyboard.KeyPress(VirtualKeyCode.SPACE);


                     Thread.Sleep(1000);
            Console.WriteLine("Searching through the process for the save dialog box");
            //PrintAllChildWindows(GE.MainWindowHandle);

            /*
            while ((saveAsDialog = FindWindow(GE.MainWindowHandle, ptr => GetWindowText(ptr) == "Save As")) == IntPtr.Zero)
                Thread.Sleep(1000);
            */

            Console.WriteLine("Found the save as dialog box");
            Console.WriteLine("Waiting for it to load...");
            Thread.Sleep(3000);
            IntPtr saveAsDialog = GetForegroundWindow();

            IntPtr textBox = FindWindow(saveAsDialog, ptr => GetWindowClass(ptr) == "Edit");
            SetFocus(textBox);
            keyboard.TextEntry(savePath);
            keyboard.KeyPress(VirtualKeyCode.RETURN);

            //return;

            Thread.Sleep(3000);
            ShowWindow(GE.MainWindowHandle, SW_SHOWMINIMIZED);

            Console.WriteLine("Automation is now complete, saving image...");

            /*
            var data = Clipboard.GetDataObject();

            if (data == null)
            {
                Console.WriteLine("Failed to access clipboard data...");
                Console.WriteLine("Google Automator will now exit");
                return;
            }
            if (data.GetDataPresent(DataFormats.Bitmap))
            {
                using (var image = (Bitmap)data.GetData(DataFormats.Bitmap, true))
                    image.Save(savePath, ImageFormat.Png);
            }
            else
            {
                Console.WriteLine("There does not seem to be a bitmap format in the clipboard....");
                Console.WriteLine("The available formats are: ");
                foreach (var format in data.GetFormats())
                    Console.WriteLine("\t{0}");
                Console.WriteLine("Google Earth Automator will now exit...");
            }
            */

            Console.WriteLine("The image has been successfully saved at ({0})", savePath);

            Console.Write("Would you like to view the image? (Yes/No) > ");
            var answer = Console.ReadLine();

            if (answer.ToLower() == "y" || answer.ToLower() == "yes")
                try { Process.Start(savePath); } catch { }

            Console.Write("Well that's it!, would you like to run another automation sequence? > ");
            answer = Console.ReadLine();

            if (answer.ToLower() == "y" || answer.ToLower() == "yes")
                goto START;
        }

        static void DisplayProcessWindowNames(Process process)
        {
            if (process == null) return;

            foreach (ProcessThread thread in process.Threads)
                EnumThreadWindows(thread.Id
                    , (hWnd, lParam) =>
                    {
                        var msg = new StringBuilder(1000);
                        SendMessage(hWnd, WM_GETTEXT, msg.Capacity, msg);
                        Console.WriteLine("{0} - {1:x} ({2})", msg.ToString(), hWnd.ToInt32(), hWnd);
                        return true;


                        

                    }, IntPtr.Zero);
        }

        /// <summary>
        /// Find a child window that matches a set of conditions specified as a Predicate that receives hWnd.  Returns IntPtr.Zero
        /// if the target window not found.  Typical search criteria would be some combination of window attributes such as
        /// ClassName, Title, etc., all of which can be obtained using API functions you will find on pinvoke.net
        /// </summary>
        /// <remarks>
        ///     <para>Example: Find a window with specific title (use Regex.IsMatch for more sophisticated search)</para>
        ///     <code lang="C#"><![CDATA[var foundHandle = Win32.FindWindow(IntPtr.Zero, ptr => Win32.GetWindowText(ptr) == "Dashboard");]]></code>
        /// </remarks>
        /// <param name="parentHandle">Handle to window at the start of the chain.  Passing IntPtr.Zero gives you the top level
        /// window for the current process.  To get windows for other processes, do something similar for the FindWindow
        /// API.</param>
        /// <param name="target">Predicate that takes an hWnd as an IntPtr parameter, and returns True if the window matches.  The
        /// first match is returned, and no further windows are scanned.</param>
        /// <returns> hWnd of the first found window, or IntPtr.Zero on failure </returns>
        public static IntPtr FindWindow(IntPtr parentHandle, Predicate<IntPtr> target)
        {
            var result = IntPtr.Zero;
            if (parentHandle == IntPtr.Zero)
                parentHandle = Process.GetCurrentProcess().MainWindowHandle;
            EnumChildWindows(parentHandle, (hwnd, param) => {
                if (target(hwnd))
                {
                    result = hwnd;
                    return false;
                }
                return true;
            }, IntPtr.Zero);
            return result;
        }

        public static void DisplayWindowHandle(IntPtr handle)
        {
            if (handle == IntPtr.Zero) Console.WriteLine(0);
            var msg = new StringBuilder(1000);
            SendMessage(handle, WM_GETTEXT, msg.Capacity, msg);
            Console.WriteLine("{0} - {1:x}", msg.ToString(), handle.ToInt32());
        }

        /// <summary>
        /// Recursive way to find every window handle in a given process
        /// </summary>
        /// <param name="handle"></param>
        public static void PrintAllChildWindows(IntPtr handle)
        {
            if (handle == IntPtr.Zero) return;

            EnumChildWindows(handle, (hWnd, param) =>
            {
                var msg = new StringBuilder(1000);
                SendMessage(hWnd, WM_GETTEXT, msg.Capacity, msg);
                Console.WriteLine("{0} - {1:x} (Parent => {2:x})", msg.ToString(), hWnd.ToInt32(), handle.ToInt32());

                //PrintAllChildWindows(hWnd);
                return true;
            }, IntPtr.Zero);
        }



        public static string GetWindowText(IntPtr hWnd)
        {
            // Allocate correct string length first
            var sb = new StringBuilder(1000);
            SendMessage(hWnd, WM_GETTEXT, sb.Capacity, sb);
            return sb.ToString();
        }

        public static string GetWindowClass(IntPtr hWnd)
        {
            var sb = new StringBuilder(1000);
            GetClassName(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }
    }
}
