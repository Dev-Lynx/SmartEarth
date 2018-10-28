using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ComTypes = System.Runtime.InteropServices.ComTypes;

namespace SmartEarth.Demos.ProcessMonitor
{
    public class CpuUsage
    {
        #region Properties 
        const int MinimumElapsed = 250;
        public Int16 Usage { get; private set; } = -1;
        public Process Process { get; private set; }
        public DateTime LastUpdateTime { get; private set; } = DateTime.MinValue;
        public TimeSpan ProcessTotal { get; private set; } = TimeSpan.MinValue;
        long _count = 0;

        ComTypes.FILETIME Kernel { get; set; }
        ComTypes.FILETIME User { get; set; }

        bool EnoughTimePassed => ((DateTime.Now - LastUpdateTime).TotalMilliseconds) > MinimumElapsed;
        bool IsFirstRun => LastUpdateTime == DateTime.MinValue;
        #endregion

        #region Constructors
        public CpuUsage(Process process)
        {
            Process = process;
        }
        #endregion

        #region Methods

        #region Dll Imports
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetSystemTimes(
            out ComTypes.FILETIME lpIdleTime
            , out ComTypes.FILETIME lpKernelTime
            , out ComTypes.FILETIME lpUserTime);
        #endregion

        public short GetUsage()
        {
            if (Interlocked.Increment(ref _count) == 1)
            {
                if (!EnoughTimePassed)
                {
                    Interlocked.Decrement(ref _count);
                    return Usage;
                }

                ComTypes.FILETIME sysIdle, sysKernel, sysUser;
                TimeSpan procTime = Process.TotalProcessorTime;

                if (!GetSystemTimes(out sysIdle, out sysKernel, out sysUser))
                {
                    Interlocked.Decrement(ref _count);
                    return Usage;
                }

                if (!IsFirstRun)
                {
                    UInt64 sysKernelDiff = SubtractTimes(sysKernel, Kernel);
                    UInt64 sysUserDiff = SubtractTimes(sysUser, User);

                    UInt64 sysTotal = sysKernelDiff + sysUserDiff;
                    Int64 procTotal = procTime.Ticks - ProcessTotal.Ticks;

                    if (sysTotal > 0)
                        Usage = (short)((100 * procTotal) / (long)sysTotal);
                }

                ProcessTotal = procTime;
                Kernel = sysKernel;
                User = sysUser;

                LastUpdateTime = DateTime.Now;
            }
            Interlocked.Decrement(ref _count);

            return Usage;
        }

        UInt64 SubtractTimes(ComTypes.FILETIME a, ComTypes.FILETIME b)
        {
            UInt64 aInt = ((UInt64)(a.dwHighDateTime << 32) | (UInt64)a.dwLowDateTime);
            UInt64 bInt = ((UInt64)(b.dwHighDateTime << 32) | (UInt64)b.dwLowDateTime);
            return aInt - bInt;
        }
        #endregion



    }
}
