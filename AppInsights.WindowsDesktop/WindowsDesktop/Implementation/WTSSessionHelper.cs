#if NET461 || NETCOREAPP3_0 || NETFX_CORE
using System;
#if !NETFX_CORE
using System.Runtime.InteropServices;
using System.Windows.Interop;
#endif

namespace Microsoft.ApplicationInsights.WindowsDesktop
{
    internal class WTSSessionHelper
#if !NETFX_CORE
        : IDisposable
#endif
    {
        public static bool IsRemoteDesktop
        {
            get
            {
#if NETFX_CORE
                return Windows.System.RemoteDesktop.InteractiveSession.IsRemote;
#else
                return GetSystemMetrics(SM_REMOTESESSION) != 0;
#endif
            }
        }

#if !NETFX_CORE
        private IntPtr _hwnd = IntPtr.Zero;

        public WTSSessionHelper(System.Windows.Window window)
        {
            if (window == null)
                return;
            var wih = new System.Windows.Interop.WindowInteropHelper(window);
            _hwnd = wih.Handle;
            var mainWindowHwndSource = System.Windows.PresentationSource.FromVisual(window) as HwndSource;
            if (mainWindowHwndSource != null)
                mainWindowHwndSource.AddHook(new HwndSourceHook(WindowHwndMessageFilter));
            WTSRegisterSessionNotification(_hwnd, NOTIFY_FOR_THIS_SESSION);
        }

        public static bool IsRemoteControlled => GetSystemMetrics(SM_REMOTECONTROL) != 0;

        public event EventHandler<bool> RemoteDesktopChanged;
        public event EventHandler<bool> RemoteControlChanged;
        public event EventHandler<WTSSessionState> SessionStateChanged;

        public void Dispose()
        {
            if (_hwnd != IntPtr.Zero)
            {
                WTSUnRegisterSessionNotification(_hwnd);
                _hwnd = IntPtr.Zero;
            }
        }

        private IntPtr WindowHwndMessageFilter(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_WTSSESSION_CHANGE:
                    WTSSessionState param = (WTSSessionState)wParam;
                    switch (param)
                    {
                        case WTSSessionState.WTS_SessionConnectedToConsole:
                            SessionStateChanged?.Invoke(this, param);
                            RemoteControlChanged?.Invoke(this, true);
                            break;
                        case WTSSessionState.WTS_SessionDisconnectedFromConsole:
                            SessionStateChanged?.Invoke(this, param);
                            RemoteControlChanged?.Invoke(this, false);
                            break;
                        case WTSSessionState.WTS_SessonConnectedToRemoteTerminal:
                            SessionStateChanged?.Invoke(this, param);
                            RemoteDesktopChanged?.Invoke(this, true);
                            break;
                        case WTSSessionState.WTS_SessionDisconnectedFromRemoteTerminal:
                            SessionStateChanged?.Invoke(this, param);
                            RemoteDesktopChanged?.Invoke(this, false);
                            break;
                        case WTSSessionState.WTS_UserLoggedOnToSession:
                        case WTSSessionState.WTS_UserLoggedOffFromSession:
                        case WTSSessionState.WTS_SessionLocked:
                        case WTSSessionState.WTS_SessionUnlocked:
                        case WTSSessionState.WTS_RemoteControlStatusChanged:
                            SessionStateChanged?.Invoke(this, param);
                            break;
                    }
                    break;
                default:
                    break;
            }
            return IntPtr.Zero;
        }

        /// <summary>
        /// The Windows Terminal Services Session State
        /// </summary>
        public enum WTSSessionState : int
        {
            /// <summary>A session was connected to the console terminal.</summary>
            WTS_SessionConnectedToConsole = 0x1,
            /// <summary>A session was disconnected from the console terminal.</summary>
            WTS_SessionDisconnectedFromConsole = 0x2,
            /// <summary>A session was connected to the remote terminal.</summary>
            WTS_SessonConnectedToRemoteTerminal = 0x3,
            /// <summary>A session was disconnected from the remote terminal.</summary>
            WTS_SessionDisconnectedFromRemoteTerminal = 0x4,
            /// <summary>A user has logged on to the session.</summary>
            WTS_UserLoggedOnToSession = 0x5,
            /// <summary>A user has logged off the session.</summary>
            WTS_UserLoggedOffFromSession = 0x6,
            /// <summary>A session has been locked.</summary>
            WTS_SessionLocked = 0x7,
            /// <summary>A session has been unlocked.</summary>
            WTS_SessionUnlocked = 0x8,
            /// <summary>A session has changed its remote controlled status.</summary>
            WTS_RemoteControlStatusChanged = 0x9,
        }


        private const int SM_REMOTESESSION = 0x1000;
        private const int SM_REMOTECONTROL = 0x2001;
        private const int NOTIFY_FOR_THIS_SESSION = 0;
        private const int WM_WTSSESSION_CHANGE = 0x2b1;

        [DllImport("user32.dll")]
        static extern int GetSystemMetrics(int systemMetric);

        [DllImport("wtsapi32.dll", SetLastError = true)]
        static extern bool WTSRegisterSessionNotification(IntPtr hWnd, [MarshalAs(UnmanagedType.U4)] int dwFlags);

        [DllImport("wtsapi32.dll", SetLastError = true)]
        static extern bool WTSUnRegisterSessionNotification(IntPtr hWnd);
#endif
    }
}
#endif
