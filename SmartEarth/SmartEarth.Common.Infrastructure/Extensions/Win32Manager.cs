using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ComTypes = System.Runtime.InteropServices.ComTypes;

namespace SmartEarth.Common.Infrastructure.Extensions
{

    public static class Win32Manager
    {
        #region Enums
        [Flags]
        public enum ThreadAccess : int
        {
            TERMINATE = (0x0001),
            SUSPEND_RESUME = (0x0002),
            GET_CONTEXT = (0x0008),
            SET_CONTEXT = (0x0010),
            SET_INFORMATION = (0x0020),
            QUERY_INFORMATION = (0x0040),
            SET_THREAD_TOKEN = (0x0080),
            IMPERSONATE = (0x0100),
            DIRECT_IMPERSONATION = (0x0200)
        }

        public enum KeyEvent : int
        {
            WM_KEYDOWN = 256,
            WM_KEYUP = 257,
            WM_SYSKEYUP = 261,
            WM_SYSKEYDOWN = 260
        }
        #endregion

        #region Properties
        public static IntPtr ForegroundWindow
        {
            get => GetForegroundWindow();
            set
            {
                if (!SetForegroundWindow(value)) Core.Log.Debug("Failed to set {0} as foreground window", value);
                else Core.Log.Debug("Foreground window has successfully been set to {0}", value);
            }
        }
        static WinEventDelegate winEventDelegate;

        #region Constants
        public const int SW_SHOWMINIMIZED = 2;
        public const int SW_SHOWMAXIMIZED = 3;
        public const int SW_SHOW = 5;
        public const int BN_CLICKED = 245;
        public const UInt32 WM_CLOSE = 0x0010;
        public const int WM_GETTEXT = 0x000D;

        const uint WINEVENT_OUTOFCONTEXT = 0;
        const uint EVENT_SYSTEM_FOREGROUND = 3;
        #endregion

        #region Events
        public static event EventHandler ForegroundWindowChanged;
        #endregion

        #endregion

        #region Methods

        public static void Initialize()
        {
            winEventDelegate = new WinEventDelegate(WinEventProc);
            SetWinEventHook(EVENT_SYSTEM_FOREGROUND
                , EVENT_SYSTEM_FOREGROUND, IntPtr.Zero
                , winEventDelegate, 0, 0, WINEVENT_OUTOFCONTEXT);
        }

        #region Window Management
        [DllImport("user32.dll")]
        public static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool BringWindowToTop(this IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "ShowWindow", SetLastError = true)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, StringBuilder lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetFocus(this IntPtr hWnd);

        [DllImportAttribute("user32.dll", EntryPoint = "BlockInput")]
        [return: MarshalAsAttribute(UnmanagedType.Bool)]
        static extern bool BlockInput([MarshalAsAttribute(UnmanagedType.Bool)] bool fBlockIt);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetSystemTimes(out ComTypes.FILETIME lpIdleTime, out ComTypes.FILETIME lpKernelTime, out ComTypes.FILETIME lpUserTime);

        [DllImport("User32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        public static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentThreadId();

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetKeyboardLayout(uint dwLayout);

        #region Enumerations
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumChildWindows(IntPtr parentHandle, EnumDelegate callback, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool EnumThreadWindows(int dwThreadId, EnumDelegate callback, IntPtr lParam);
        #endregion

        #region Delegates
        public delegate bool EnumDelegate(IntPtr hwnd, IntPtr lParam);
        #endregion

        #endregion

        #region Thread Management
        [DllImport("kernel32.dll")]
        static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);
        [DllImport("kernel32.dll")]
        static extern uint SuspendThread(IntPtr hThread);
        [DllImport("kernel32.dll")]
        static extern int ResumeThread(IntPtr hThread);
        #endregion

        #region Hooks

        [DllImport("user32.dll")]
        public static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, UIntPtr wParam, IntPtr lParam);

        #region Delegates
        public delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);
        public delegate IntPtr LowLevelKeyboardProc(int nCode, UIntPtr wParam, IntPtr lParam);
        #endregion

        #endregion

        #region Helpers
        public static IntPtr FindChildWindow(this IntPtr parent, Predicate<IntPtr> target)
        {
            var result = IntPtr.Zero;
            if (parent == IntPtr.Zero)
                parent = Process.GetCurrentProcess().MainWindowHandle;
            EnumChildWindows(parent, (hWnd, param) =>
            {
                bool found = target(hWnd);
                if (found) result = hWnd;
                return !found;
            }, IntPtr.Zero);
            return result;
        }

        public static IntPtr FindChildWindowByName(this IntPtr parent, string name) => FindChildWindow(parent, hWnd => GetWindowText(hWnd) == name);
        public static IntPtr FindChildWindowByClass(this IntPtr parent, string name) => FindChildWindow(parent, hWnd => GetWindowClass(hWnd) == name);

        public static IntPtr FindThreadWindow(this Process process, Predicate<IntPtr> target)
        {
            if (process == null) process = Process.GetCurrentProcess();

            var result = IntPtr.Zero;
            foreach (ProcessThread thread in process.Threads)
                EnumThreadWindows(thread.Id, (hWnd, param) =>
                {
                    bool found = target(hWnd);
                    if (found) result = hWnd;
                    return !found;
                }, IntPtr.Zero);
            return result;
        }

        public static IntPtr FindThreadWindowByName(this Process process, string name) => FindThreadWindow(process, hWnd => GetWindowText(hWnd) == name);
        public static IntPtr FindThreadWindowByClass(this Process process, string name) => FindThreadWindow(process, hWnd => GetWindowClass(hWnd) == name);

        public static string GetWindowText(this IntPtr hWnd)
        {
            var builder = new StringBuilder(1000);
            SendMessage(hWnd, WM_GETTEXT, builder.Capacity, builder);
            return builder.ToString();
        }

        public static string GetWindowClass(this IntPtr hWnd)
        {
            var builder = new StringBuilder(1000);
            GetClassName(hWnd, builder, builder.Capacity);
            return builder.ToString();
        }

        public static void Close(this IntPtr hWnd) => SendMessage(hWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
        #endregion

        #region Process Management
        public static Process AttachOrLaunch(ProcessStartInfo info)
        {
            try
            {
                var name = Path.GetFileNameWithoutExtension(info.FileName);
                var processes = Process.GetProcessesByName(name);
                if (processes.Length <= 0) return Process.Start(info);
                return processes.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Core.Log.Error("An error occured during an attempt to launch a process. \nProcess Path ({0}) \n{1}", info.FileName, ex);
                return null;
            }
        }

        public static bool AttemptClose(this Process process)
        {
            try { process.Kill(); }
            catch { }

            return !Process.GetProcesses().Contains(process); 
        }

        public static void Suspend(this Process process)
        {
            foreach (ProcessThread thread in process.Threads)
            {
                var pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)thread.Id);
                if (pOpenThread == IntPtr.Zero)
                    break;
                SuspendThread(pOpenThread);
            }
        }
        public static void Resume(this Process process)
        {
            foreach (ProcessThread thread in process.Threads)
            {
                var pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)thread.Id);
                if (pOpenThread == IntPtr.Zero)
                    break;
                ResumeThread(pOpenThread);
            }
        }

        public static void ForceWindowToForeground(this IntPtr hWnd)
        {
            BringWindowToTop(hWnd);
            ShowWindow(hWnd, SW_SHOW);
        }

        #region CPU Tracking
        public static async Task<bool> WaitTillIdle(this Process process, TimeSpan? expireTimeout = null, CancellationToken token = (default(CancellationToken)), double refreshInterval=500, int maxCpuPercentage=5)
        {
            if (token == null) token = new CancellationToken();
            TimeSpan waitTimeout = expireTimeout ?? Timeout.InfiniteTimeSpan;

            bool idle = false;
            ComTypes.FILETIME user, kernel, sysIdle, sysKernel, sysUser;
            user = kernel = new ComTypes.FILETIME();
            short usage = 0; DateTime lastUpdate = DateTime.MinValue;
            TimeSpan processTotal = TimeSpan.MinValue;
            var timer = new System.Timers.Timer() { Interval = refreshInterval, Enabled = true, AutoReset = true };

            var timeoutTimer = new Timer((s) => idle = true, null, waitTimeout, Timeout.InfiniteTimeSpan);

            int key = 0, lowCount = 0;

            timer.Elapsed += (s, e) =>
            {
                if (token.IsCancellationRequested || process.HasExited)
                {
                    timer.Stop();
                    return;
                }



                if (Interlocked.Increment(ref key) != 1) return;
                if (!GetSystemTimes(out sysIdle, out sysKernel, out sysUser))
                {
                    usage = 0;
                    goto AnalyzeUsage;
                }

                var procTime = process.TotalProcessorTime;
                if (lastUpdate != DateTime.MinValue)
                {
                    UInt64 sysKernelDiff = SubtractTimes(sysKernel, kernel);
                    UInt64 sysUserDiff = SubtractTimes(sysUser, user);

                    UInt64 sysTotal = sysKernelDiff + sysUserDiff;
                    Int64 procTotal = procTime.Ticks - processTotal.Ticks;

                    if (sysTotal > 0)
                        usage = (short)((100 * procTotal) / (long)sysTotal);
                }

                processTotal = procTime;
                kernel = sysKernel;
                user = sysUser;
                lastUpdate = DateTime.Now;

                AnalyzeUsage:
                if (usage < maxCpuPercentage) lowCount++;
                else lowCount = 0;

                if (lowCount > 10)
                {
                    timer.Stop();
                    idle = true;
                }

                Core.Log.Debug("Waiting for {0} to become idle. ({1})", process, usage);
                Interlocked.Decrement(ref key);
            };

            while (!idle && (token == null || !token.IsCancellationRequested) && timer.Enabled) await Task.Delay(1000);
            timeoutTimer.Dispose();
            timer.Dispose();
            return idle;
        }

        static UInt64 SubtractTimes(ComTypes.FILETIME a, ComTypes.FILETIME b)
        {
            UInt64 aInt = ((UInt64)(a.dwHighDateTime << 32) | (UInt64)a.dwLowDateTime);
            UInt64 bInt = ((UInt64)(b.dwHighDateTime << 32) | (UInt64)b.dwLowDateTime);
            return aInt - bInt;
        }
        #endregion

        public static void BlockInput() => BlockInput(true);
        public static void UnBlockInput() => BlockInput(false);

        public static async Task<IntPtr> WaitForThreadWindow(this Process process, Predicate<IntPtr> target, CancellationToken token = default(CancellationToken), TimeSpan? expireTimeout = null, int checkInterval = 1000)
        {
            TimeSpan expire = expireTimeout ?? Timeout.InfiniteTimeSpan;
            if (token == null) token = new CancellationToken();
            bool timedOut = false;
            Timer timer = new Timer((s) => timedOut = true, null, expire, Timeout.InfiniteTimeSpan);

            IntPtr handle = IntPtr.Zero;
            while ((handle = FindThreadWindow(process, target)) == IntPtr.Zero && !timedOut)
                await Task.Delay(checkInterval, token);

            timer.Dispose();
            return handle;
        }

        public static async Task<IntPtr> WaitForThreadWindowByName(this Process process, string name, CancellationToken token = default(CancellationToken), TimeSpan? expireTimeout = null, int checkInterval = 1000)
            => await WaitForThreadWindow(process, hWnd => (GetWindowText(hWnd) == name), token, expireTimeout, checkInterval);

        public static async Task<IntPtr> WaitForThreadWindowByClass(this Process process, string name, CancellationToken token = default(CancellationToken), TimeSpan? expireTimeout = null, int checkInterval = 1000)
            => await WaitForThreadWindow(process, hWnd => (GetWindowClass(hWnd) == name), token, expireTimeout, checkInterval);

        public static async Task<IntPtr> WaitForChildWindow(this IntPtr parent, Predicate<IntPtr> target, CancellationToken token = default(CancellationToken), TimeSpan? expireTimeout = null, int checkInterval = 1000)
        {
            TimeSpan expire = expireTimeout ?? Timeout.InfiniteTimeSpan;
            if (token == null) token = new CancellationToken();
            bool timedOut = false;
            Timer timer = new Timer((s) => timedOut = true, null, expire, Timeout.InfiniteTimeSpan);

            IntPtr handle = IntPtr.Zero;
            while ((handle = FindChildWindow(parent, target)) == IntPtr.Zero && !timedOut)
                await Task.Delay(checkInterval, token);

            timer.Dispose();
            return handle;
        }

        public static async Task<IntPtr> WaitForChildWindowByName(this IntPtr parent, string name, CancellationToken token = default(CancellationToken), TimeSpan? expireTimeout = null, int checkInterval = 1000)
            => await WaitForChildWindow(parent, hWnd => (GetWindowText(hWnd) == name), token, expireTimeout, checkInterval);

        public static async Task<IntPtr> WaitForChildWindowByClass(this IntPtr parent, string name, CancellationToken token = default(CancellationToken), TimeSpan? expireTimeout = null, int checkInterval = 1000)
            => await WaitForChildWindow(parent, hWnd => (GetWindowClass(hWnd) == name), token, expireTimeout, checkInterval);

        #endregion

        #region Event Handlers

        static void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            ForegroundWindowChanged?.Invoke(null, EventArgs.Empty);
        }

        #endregion

        #endregion
    }
}
