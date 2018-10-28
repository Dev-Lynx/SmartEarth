using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WindowsInput.Native;

namespace SmartEarth.Common.Infrastructure.Extensions
{
    public static class InputManager
    {
        #region Properties
        static IntPtr Hook { get; set; } = IntPtr.Zero;
        static LowLevelKeyboardProc HookedLowLevelKeyboardProc { get; set; }
        #region Events

        #endregion

        #region Enums
        /// <summary>
        /// Key event
        /// </summary>
        public enum KeyEvent : int
        {
            /// <summary>
            /// Key down
            /// </summary>
            WM_KEYDOWN = 256,

            /// <summary>
            /// Key up
            /// </summary>
            WM_KEYUP = 257,

            /// <summary>
            /// System key up
            /// </summary>
            WM_SYSKEYUP = 261,

            /// <summary>
            /// System key down
            /// </summary>
            WM_SYSKEYDOWN = 260
        }
        #endregion

        #endregion

        #region Methods

        public static void Initialize()
        {
            HookedLowLevelKeyboardProc = LowLevelKeyboardProcHandler;
            Hook = SetHook(HookedLowLevelKeyboardProc);
        }

        #region Imports
        public static int WH_KEYBOARD_LL = 13;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, UIntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        #region Conversions

        [DllImport("user32.dll")]
        private static extern int ToUnicodeEx(uint wVirtKey, uint wScanCode, byte[] lpKeyState, [Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder pwszBuff, int cchBuff, uint wFlags, IntPtr dwhkl);

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKeyEx(uint uCode, uint uMapType, IntPtr dwhkl);
        #endregion

        #endregion

        #region Delegates

        public delegate IntPtr LowLevelKeyboardProc(int nCode, UIntPtr wParam, IntPtr lParam);
        /// <summary>
        /// Asynchronous callback hook.
        /// </summary>
        /// <param name="character">Character</param>
        /// <param name="keyEvent">Keyboard event</param>
        /// <param name="vkCode">VKCode</param>
        delegate void KeyboardCallbackAsync(KeyEvent keyEvent, int vkCode, string character);
        #endregion

        public static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process process = Process.GetCurrentProcess())
            using (ProcessModule module = process.MainModule)
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(module.ModuleName), 0);
        }

        #region Internals
        /// <summary>
        /// Actual callback hook.
        /// 
        /// <remarks>Calls asynchronously the asyncCallback.</remarks>
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        static IntPtr LowLevelKeyboardProcHandler(int nCode, UIntPtr wParam, IntPtr lParam)
        {
            string chars = "";

            if (nCode >= 0)
                if (wParam.ToUInt32() == (int)KeyEvent.WM_KEYDOWN ||
                    wParam.ToUInt32() == (int)KeyEvent.WM_KEYUP ||
                    wParam.ToUInt32() == (int)KeyEvent.WM_SYSKEYDOWN ||
                    wParam.ToUInt32() == (int)KeyEvent.WM_SYSKEYUP)
                {
                    // Captures the character(s) pressed only on WM_KEYDOWN
                    chars = VKCodeToString((uint)Marshal.ReadInt32(lParam),
                        (wParam.ToUInt32() == (int)KeyEvent.WM_KEYDOWN ||
                        wParam.ToUInt32() == (int)KeyEvent.WM_SYSKEYDOWN));
                    Core.Log.Debug(chars);
                    //hookedKeyboardCallbackAsync.BeginInvoke((KeyEvent)wParam.ToUInt32(), Marshal.ReadInt32(lParam), chars, null, null);
                }
                         
            return (IntPtr)(-1);//CallNextHookEx(Hook, nCode, wParam, lParam);
        }
        #endregion

        #region Conversions
        private static uint lastVKCode = 0;
        private static uint lastScanCode = 0;
        private static byte[] lastKeyState = new byte[255];
        private static bool lastIsDead = false;

        /// <summary>
        /// Convert VKCode to Unicode.
        /// <remarks>isKeyDown is required for because of keyboard state inconsistencies!</remarks>
        /// </summary>
        /// <param name="VKCode">VKCode</param>
        /// <param name="isKeyDown">Is the key down event?</param>
        /// <returns>String representing single unicode character.</returns>
        public static string VKCodeToString(uint VKCode, bool isKeyDown)
        {
            // ToUnicodeEx needs StringBuilder, it populates that during execution.
            System.Text.StringBuilder sbString = new System.Text.StringBuilder(5);

            byte[] bKeyState = new byte[255];
            bool bKeyStateStatus;
            bool isDead = false;

            // Gets the current windows window handle, threadID, processID
            IntPtr currentHWnd = Win32Manager.ForegroundWindow;
            uint currentProcessID;
            uint currentWindowThreadID = Win32Manager.GetWindowThreadProcessId(currentHWnd, out currentProcessID);

            // This programs Thread ID
            uint thisProgramThreadId = Win32Manager.GetCurrentThreadId();

            // Attach to active thread so we can get that keyboard state
            if (Win32Manager.AttachThreadInput(thisProgramThreadId, currentWindowThreadID, true))
            {
                // Current state of the modifiers in keyboard
                bKeyStateStatus = Win32Manager.GetKeyboardState(bKeyState);

                // Detach
                Win32Manager.AttachThreadInput(thisProgramThreadId, currentWindowThreadID, false);
            }
            else
            {
                // Could not attach, perhaps it is this process?
                bKeyStateStatus = Win32Manager.GetKeyboardState(bKeyState);
            }

            // On failure we return empty string.
            if (!bKeyStateStatus)
                return "";

            // Gets the layout of keyboard
            IntPtr HKL = Win32Manager.GetKeyboardLayout(currentWindowThreadID);

            // Maps the virtual keycode
            uint lScanCode = MapVirtualKeyEx(VKCode, 0, HKL);

            // Keyboard state goes inconsistent if this is not in place. In other words, we need to call above commands in UP events also.
            if (!isKeyDown)
                return "";

            // Converts the VKCode to unicode
            int relevantKeyCountInBuffer = ToUnicodeEx(VKCode, lScanCode, bKeyState, sbString, sbString.Capacity, (uint)0, HKL);

            string ret = "";

            switch (relevantKeyCountInBuffer)
            {
                // Dead keys (^,`...)
                case -1:
                    isDead = true;

                    // We must clear the buffer because ToUnicodeEx messed it up, see below.
                    ClearKeyboardBuffer(VKCode, lScanCode, HKL);
                    break;

                case 0:
                    break;

                // Single character in buffer
                case 1:
                    ret = sbString[0].ToString();
                    break;

                // Two or more (only two of them is relevant)
                case 2:
                default:
                    ret = sbString.ToString().Substring(0, 2);
                    break;
            }

            // We inject the last dead key back, since ToUnicodeEx removed it.
            // More about this peculiar behavior see e.g: 
            //   http://www.experts-exchange.com/Programming/System/Windows__Programming/Q_23453780.html
            //   http://blogs.msdn.com/michkap/archive/2005/01/19/355870.aspx
            //   http://blogs.msdn.com/michkap/archive/2007/10/27/5717859.aspx
            if (lastVKCode != 0 && lastIsDead)
            {
                System.Text.StringBuilder sbTemp = new System.Text.StringBuilder(5);
                ToUnicodeEx(lastVKCode, lastScanCode, lastKeyState, sbTemp, sbTemp.Capacity, (uint)0, HKL);
                lastVKCode = 0;

                return ret;
            }

            // Save these
            lastScanCode = lScanCode;
            lastVKCode = VKCode;
            lastIsDead = isDead;
            lastKeyState = (byte[])bKeyState.Clone();

            return ret;
        }

        private static void ClearKeyboardBuffer(uint vk, uint sc, IntPtr hkl)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder(10);

            int rc;
            do
            {
                byte[] lpKeyStateNull = new Byte[255];
                rc = ToUnicodeEx(vk, sc, lpKeyStateNull, sb, sb.Capacity, 0, hkl);
            } while (rc < 0);
        }
        #endregion

        #endregion
    }
}
