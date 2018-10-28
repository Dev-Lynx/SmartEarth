using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestStack.White;
using TestStack.White.UIItems.WindowItems;

namespace SmartEarth.Demos.WhiteAutomation
{
    class Program
    {

        #region Properties
        const string GoogleEarth = "googleearth";
        const string GOOGLE_EARTH_SAVE_TAB = "earth::modules::print::PrintToolbarClassWindow";
        static readonly string GoogleEarthPath = Path.Combine(Environment.ExpandEnvironmentVariables("%ProgramW6432%"), @"Google\Google Earth Pro\client\googleearth.exe");
        #endregion

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
            Console.WriteLine("Welcome to SmartEarth's very own Google Earth Automator!");
            Console.WriteLine("Allow me to handle all the Google Earth Hassle :)");

            var processInfo = new ProcessStartInfo(GoogleEarthPath);
            var application = Application.AttachOrLaunch(processInfo);
            application.WaitWhileBusy();

            
            Window startUpTips = null;
            //if ((startUpTips = application.GetWindow("Start-up Tips")) != null) startUpTips.Click();

            //Window saveTab = application.GetWindow("earth::modules::print::PrintToolbarClassWindow");

            //if (saveTab != null && saveTab.Visible) saveTab.Close();
            /*
            var mainWindow = application.GetWindows().FirstOrDefault();
            mainWindow.Keyboard.HoldKey(TestStack.White.WindowsAPI.KeyboardInput.SpecialKeys.ALT);
            mainWindow.Keyboard.HoldKey(TestStack.White.WindowsAPI.KeyboardInput.SpecialKeys.CONTROL);
            mainWindow.Keyboard.Enter("s");
            mainWindow.Keyboard.LeaveKey(TestStack.White.WindowsAPI.KeyboardInput.SpecialKeys.ALT);
            mainWindow.Keyboard.LeaveKey(TestStack.White.WindowsAPI.KeyboardInput.SpecialKeys.CONTROL);
            */
            var windows = application.GetWindows();
            foreach (var window in windows)
            {
                Console.WriteLine(window.Name);
                foreach (var modalWindow in window.ModalWindows())
                    Console.WriteLine(modalWindow.Name);
            }
        }
    }
}
