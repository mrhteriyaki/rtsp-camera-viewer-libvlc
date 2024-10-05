using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace rtsp_camera_viewer
{
    public class WindowsAPI
    {
        Form1 localForm;
        public WindowsAPI(Form1 form)
        {
            localForm = form;
        }


        [DllImport("user32.dll")]
        public static extern int ShowWindow(int hwnd, SW command);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        public const int SWP_SHOWWINDOW = 0x0040;
        public static readonly IntPtr HWND_TOP = new IntPtr(0);

        public enum SW
        {
            SW_HIDE = 0,
            SW_SHOW = 5
        }


        // Constants for mouse events
        private const int WH_MOUSE_LL = 14;
        private const int WM_LBUTTONDOWN = 0x201;

        // Structure to hold information about the mouse event
        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public Point pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }


        // Delegate for the mouse hook procedure
        private delegate IntPtr MouseHookDelegate(int nCode, IntPtr wParam, IntPtr lParam);

        // Handle for the mouse hook
        private IntPtr mouseHookHandle;

        // Mouse hook procedure
        private IntPtr MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == new IntPtr(WM_LBUTTONDOWN))
            {
                // Left mouse button was clicked
                MSLLHOOKSTRUCT mouseStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                var activeWindow = GetForegroundWindow();
                var mPt = mouseStruct.pt;
                if (GetWindowTitle(activeWindow) == localForm.Text)
                {
                    Rectangle FrmRec = GetWindowPosition();
                    if (FrmRec.Contains(mPt)) // Within form location.
                    {
                        // Mouse pointer location relative to Window.
                        Point RelativePoint = new Point(mPt.X - FrmRec.X, mPt.Y - FrmRec.Y);
                        localForm.ClickCamera(RelativePoint);                       
                    }
                }
            }
            return CallNextHookEx(mouseHookHandle, nCode, wParam, lParam);
        }

        // Import the SetWindowsHookEx function from user32.dll
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, MouseHookDelegate lpfn, IntPtr hMod, uint dwThreadId);

        // Import the UnhookWindowsHookEx function from user32.dll
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        // Import the CallNextHookEx function from user32.dll
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        // Import the GetCurrentThreadId function from kernel32.dll
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern uint GetCurrentThreadId();

        // Import the GetModuleHandle function from kernel32.dll
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        // Start the mouse hook
        private MouseHookDelegate _mouseHookDelegate;
        public void StartMouseHook()
        {
            _mouseHookDelegate = new MouseHookDelegate(MouseHookProc);
            IntPtr hMod = GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName);
            mouseHookHandle = SetWindowsHookEx(WH_MOUSE_LL, _mouseHookDelegate, hMod, 0U);
        }

        // Stop the mouse hook
        public void StopMouseHook()
        {
            if (mouseHookHandle != IntPtr.Zero)
            {
                UnhookWindowsHookEx(mouseHookHandle);
                mouseHookHandle = IntPtr.Zero;
            }
        }




        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        private string GetWindowTitle(IntPtr hWnd)
        {
            var title = new System.Text.StringBuilder(256);
            if (GetWindowText(hWnd, title, title.Capacity) > 0)
            {
                return title.ToString();
            }
            return "";
        }

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);





        // Import the necessary Windows API functions
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

        // Define the RECT structure to store the window coordinates
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        // Function to get the window position
        private Rectangle GetWindowPosition(IntPtr hWnd)
        {
            var rect = default(RECT);
            if (GetWindowRect(hWnd, ref rect))
            {
                return new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
            }
            else
            {
                return Rectangle.Empty;
            }
        }

        // Example of how to use the GetWindowPosition function
        public Rectangle GetWindowPosition()
        {
            // Replace 'YourWindowTitle' with the title of the window you want to get the position of
            IntPtr windowHandle = FindWindow(null, localForm.Text);

            if (windowHandle != IntPtr.Zero)
            {
                return GetWindowPosition(windowHandle);
            }
            else
            {
                throw new Exception("window position error");
            }
        }

        // Function to find a window by its title
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);




    }
}
