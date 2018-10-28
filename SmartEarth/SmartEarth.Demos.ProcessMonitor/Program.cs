using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SmartEarth.Demos.ProcessMonitor
{
    class Program
    {
        #region Properties
        const string GoogleEarth = "googleearth";
        static Process Process { get; set; }
        #endregion


        static void Main(string[] args) => MainAsync(args).GetAwaiter().GetResult();

        static async Task MainAsync(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            START:
            PrintInstructions();
            string name = Console.ReadLine();


            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("Active Processes");
                Console.WriteLine("-------------------");
                foreach (var process in Process.GetProcesses())
                    Console.WriteLine(process.ProcessName);

                Console.WriteLine();
                Console.WriteLine();
                goto START;
            }
            else
            {
                
                foreach (var process in Process.GetProcesses())
                {
                    if (!process.ProcessName.Contains(name))
                        continue;

                    Console.WriteLine("Press ESC to stop");
                    Process = Process.GetProcessById(process.Id);
                }

                if (Process == null)
                {
                    Console.WriteLine("Sorry, the process name you provided does not seem to be among any running applications, Please try again...");
                    goto START;
                }

                CpuUsage usage = new CpuUsage(Process);

                var loadTimer = new System.Timers.Timer()
                {
                    Interval = 500,
                    Enabled = true,
                    AutoReset = true
                };

                loadTimer.Elapsed += (s, e) =>
                {
                    Console.Write("\r{0} CPU Usage: {1}%     ", Process.ProcessName, usage.Usage);
                    usage.GetUsage();
                };

                loadTimer.Start();

                while (Console.ReadKey().Key != ConsoleKey.Escape)
                    await Task.Delay(1000);

                loadTimer.Stop();
                Process = null;
                goto START;
            }

            
        }

        static void PrintInstructions()
        {
            Console.WriteLine();
            Console.Write("Please enter the name of the process you would like to monitor\nLeave it blank if you would like to view a list of all the available processes > ");
        }
    }
}
