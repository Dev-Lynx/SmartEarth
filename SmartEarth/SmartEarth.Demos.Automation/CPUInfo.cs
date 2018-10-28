using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ComTypes = System.Runtime.InteropServices.ComTypes;

namespace SmartEarth.Demos.Automation
{
    public class CPUInfo
    {
        #region Properties
        const int MinimumElapsed = 250;
        public Process Process { get; }
        public ComTypes.FILETIME User { get; private set; }
        public ComTypes.FILETIME Kernel { get; private set; }
        public Int16 RawUsage { get; private set; } = -1;
        public DateTime LastUpdateTime { get; private set; } = DateTime.MinValue;
        public bool IsFirstRun => LastUpdateTime == DateTime.MinValue;
        public TimeSpan ProcessTotal { get; private set; }
        bool EnoughTimePassed => ((DateTime.Now - LastUpdateTime).TotalMilliseconds) > MinimumElapsed;
        long Count = 0;
        #endregion

        #region Constructors
        public CPUInfo(Process process)
        {
            Process = process;
        }
        #endregion

        #region Methods

        UInt64 SubtractTimes(ComTypes.FILETIME a, ComTypes.FILETIME b)
        {
            UInt64 aInt = ((UInt64)(a.dwHighDateTime << 32) | (UInt64)a.dwLowDateTime);
            UInt64 bInt = ((UInt64)(b.dwHighDateTime << 32) | (UInt64)b.dwLowDateTime);
            return aInt - bInt;
        }

        public long Update()
        {
            if (Interlocked.Increment(ref Count) == 1)
            {
                if (!EnoughTimePassed)
                    return Interlocked.Decrement(ref Count);

                var process = Process;

                if (process == null) Interlocked.Decrement(ref Count);

                ComTypes.FILETIME sysIdle, sysKernel, sysUser;
                TimeSpan procTime = process.TotalProcessorTime;

                if (!GetSystemTimes(out sysIdle, out sysKernel, out sysUser))
                    return Interlocked.Decrement(ref Count);

                if (!IsFirstRun)
                {
                    UInt64 sysKernelDiff = SubtractTimes(sysKernel, Kernel);
                    UInt64 sysUserDiff = SubtractTimes(sysUser, User);

                    UInt64 sysTotal = sysKernelDiff + sysUserDiff;
                    Int64 procTotal = procTime.Ticks - ProcessTotal.Ticks;

                    if (sysTotal > 0)
                        RawUsage = (short)((100 * procTotal) / (long)sysTotal);
                }

                ProcessTotal = procTime;
                Kernel = sysKernel;
                User = sysKernel;
                LastUpdateTime = DateTime.Now;
            }

            return Interlocked.Decrement(ref Count);
        }

        #region Dll Imports
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetSystemTimes(
            out ComTypes.FILETIME lpIdleTime
            , out ComTypes.FILETIME lpKernelTime
            , out ComTypes.FILETIME lpUserTime);
        #endregion

        #region Overrides

        public override string ToString() => $"{RawUsage} %";

        #endregion

        #endregion

    }
}
