using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using ComTypes = System.Runtime.InteropServices.ComTypes;
using System.Timers;
using System.Diagnostics;

namespace SmartEarth.Demos.Automation
{
    public static class CPUTracker
    {
        #region Properties
        static long _count = 0;
        const int MinimumRefreshRate = 250;
        public static bool Tracking { get; private set; }
        public static IntPtr TrackHandle { get; private set; }
        public static CPUInfo Info { get; private set; }
        public static Timer TrackTimer { get; private set; }
        
        public static event EventHandler<CPUInfo> CPUValuesUpdated;

        #region DLL Imports

        #region Process Constants
        public const int PROCESS_ENTRY_32_SIZE = 296;
        public const uint TH32CS_SNAPPROCESS = 0x00000002;
        public const uint PROCESS_ALL_ACCESS = 0x1F0FFF;

        public static readonly IntPtr PROCESS_HANDLE_ERROR = new IntPtr(-1);
        #endregion

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(uint DesiredAccess
            , bool InheritHandle
            , int ProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetSystemTimes(out ComTypes.FILETIME lpIdleTime
            , out ComTypes.FILETIME lpKernel
            , out ComTypes.FILETIME lpUserTime);

        [DllImport("kernel32.dll")]
        public static extern bool GetProcessTimes(IntPtr ProcessHandle
            , out ComTypes.FILETIME CreationTime
            , out ComTypes.FILETIME ExitTime
            , out ComTypes.FILETIME KernelTime
            , out ComTypes.FILETIME UserTime);
        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Initialize the tracking process
        /// </summary>
        /// <param name="processID">The ID pointing the process to be tracked</param>
        /// <param name="updateInterval">Measures how often the Cpu usage values of the process should be tracked</param>
        public static void TrackProcess(Process process, int updateInterval = 1000)
        {
            StopTracking();
            var handle = OpenProcess(PROCESS_ALL_ACCESS, false, process.Id);

            if (handle == IntPtr.Zero) return;
            TrackHandle = handle;

            TrackTimer = new Timer(updateInterval)
            {
                AutoReset = true, Enabled = true
            };

            Info = new CPUInfo(process);

            UpdateInfo(null, null);

            TrackTimer.Elapsed += UpdateInfo;
            TrackTimer.Start();
            Tracking = true;
        }

        static void UpdateInfo(object sender, ElapsedEventArgs e)
        {
            Info.Update();
            CPUValuesUpdated?.Invoke(null, Info);
        }

        static DateTime FileTimeToDateTime(ComTypes.FILETIME time)
        {
            try
            {
                if (time.dwLowDateTime < 0) time.dwLowDateTime = 0;
                if (time.dwHighDateTime < 0) time.dwHighDateTime = 0;

                long rawFileTime = (((long)time.dwHighDateTime) << 32) + time.dwLowDateTime;
                return DateTime.FromFileTimeUtc(rawFileTime);
            }
            catch { return new DateTime(); }
        }

        public static void StopTracking()
        {
            if (!Tracking) return;
            TrackTimer.Stop();
            Tracking = false;
        }
        #endregion


    }
}
