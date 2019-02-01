using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace HeavyMetalMachines.Platform
{
	public class WindowsPlatform
	{
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool GetSystemTimes(out System.Runtime.InteropServices.ComTypes.FILETIME lpIdleTime, out System.Runtime.InteropServices.ComTypes.FILETIME lpKernelTime, out System.Runtime.InteropServices.ComTypes.FILETIME lpUserTime);

		[DllImport("kernel32.dll")]
		public static extern int GetSystemDefaultLCID();

		[DllImport("kernel32.dll")]
		public static extern void RaiseException(uint dwExceptionCode, uint dwExceptionFlags, uint nNumberOfArguments, IntPtr lpArguments);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GlobalMemoryStatusEx([In] [Out] WindowsPlatform.MEMORYSTATUSEX lpBuffer);

		[DllImport("psapi.dll")]
		public static extern bool GetProcessMemoryInfo(IntPtr process, out WindowsPlatform.ProcessMemoryCountersEx ppsmemCounters, uint cb);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		public static extern int MessageBox(UIntPtr nHwnd, string strText, string strCaption, uint nType);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetCursorPos(out WindowsPlatform.POINT lpPoint);

		[DllImport("User32.dll")]
		public static extern int SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32.dll")]
		public static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool ShowWindow(IntPtr hWnd, WindowsPlatform.ShowWindowCommands nCmdShow);

		[DllImport("user32.dll")]
		public static extern bool ClipCursor(ref WindowsPlatform.RECT lpRect);

		[DllImport("user32.dll")]
		public static extern bool GetClipCursor(out WindowsPlatform.RECT lpRect);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SetWindowPos(IntPtr hwnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int wFlags);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool GetClientRect(IntPtr hwnd, out WindowsPlatform.RECT lpRect);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool ClientToScreen(IntPtr hwnd, out WindowsPlatform.POINT lpPoint);

		[DllImport("user32.dll")]
		public static extern IntPtr FindWindow(string className, string windowName);

		[DllImport("user32.dll")]
		public static extern bool GetWindowRect(IntPtr hwnd, out WindowsPlatform.RECT lpRect);

		[DllImport("kernel32.dll")]
		public static extern uint GetCurrentThreadId();

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool EnumThreadWindows(uint dwThreadId, WindowsPlatform.EnumWindowsProc lpEnumFunc, IntPtr lParam);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		public static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		public static extern int GetWindowTextLength(IntPtr hWnd);

		[DllImport("user32.dll")]
		public static extern IntPtr GetDesktopWindow();

		[DllImport("user32.dll")]
		public static extern bool EnumWindows(WindowsPlatform.EnumWindowsProc enumProc, IntPtr lParam);

		[DllImport("user32.dll")]
		public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, WindowsPlatform.MonitorEnumProc lpfnEnum, IntPtr dwData);

		[DllImport("user32.dll")]
		public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

		[DllImport("user32.dll")]
		public static extern bool IsWindowVisible(IntPtr hWnd);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr processId);

		[DllImport("Kernel32")]
		public static extern bool TerminateProcess(IntPtr hProcess, uint uExitCode);

		[DllImport("Kernel32")]
		public static extern IntPtr GetCurrentProcess();

		[DllImport("Kernel32")]
		public static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool FlashWindowEx(ref WindowsPlatform.FLASHWINFO pwfi);

		[DllImport("user32.dll")]
		public static extern IntPtr GetKeyboardLayout(uint thread);

		[DllImport("user32.dll")]
		public static extern int GetKeyboardLayoutName([Out] StringBuilder layoutName);

		public static void TerminateCurrentProcess()
		{
			IntPtr currentProcess = WindowsPlatform.GetCurrentProcess();
			if (WindowsPlatform.TerminateProcess(currentProcess, 0u))
			{
				WindowsPlatform.WaitForSingleObject(currentProcess, uint.MaxValue);
			}
		}

		public static IntPtr GetCurrentWindowHandle(string clientWindowText)
		{
			IntPtr windowHandle = IntPtr.Zero;
			uint currentThreadId = WindowsPlatform.GetCurrentThreadId();
			WindowsPlatform.EnumThreadWindows(currentThreadId, (IntPtr hWnd, IntPtr lParam) => WindowsPlatform.EnumThreadWindowsCallback(hWnd, clientWindowText, out windowHandle), IntPtr.Zero);
			if (windowHandle == IntPtr.Zero)
			{
				WindowsPlatform.EnumWindows((IntPtr hWnd, IntPtr lParam) => WindowsPlatform.EnumWindowsCallback(hWnd, currentThreadId, out windowHandle), IntPtr.Zero);
			}
			if (windowHandle == IntPtr.Zero)
			{
				windowHandle = WindowsPlatform.FindWindow(null, clientWindowText);
			}
			return windowHandle;
		}

		private static bool EnumThreadWindowsCallback(IntPtr hWnd, string clientWindowText, out IntPtr windowHandle)
		{
			windowHandle = IntPtr.Zero;
			int num;
			if (!WindowsPlatform.IsValidWindow(hWnd, out num))
			{
				return true;
			}
			StringBuilder stringBuilder = new StringBuilder(++num);
			WindowsPlatform.GetWindowText(hWnd, stringBuilder, num);
			if (stringBuilder.ToString() == clientWindowText)
			{
				windowHandle = hWnd;
				return false;
			}
			return true;
		}

		private static bool EnumWindowsCallback(IntPtr hWnd, uint currentThreadId, out IntPtr windowHandle)
		{
			windowHandle = IntPtr.Zero;
			int num;
			if (!WindowsPlatform.IsValidWindow(hWnd, out num))
			{
				return true;
			}
			if (currentThreadId == WindowsPlatform.GetWindowThreadProcessId(hWnd, IntPtr.Zero))
			{
				windowHandle = hWnd;
				return false;
			}
			return true;
		}

		private static bool IsValidWindow(IntPtr hWnd, out int textSize)
		{
			textSize = 0;
			return WindowsPlatform.IsWindowVisible(hWnd) && (textSize = WindowsPlatform.GetWindowTextLength(hWnd)) > 0;
		}

		[DllImport("User32")]
		public static extern IntPtr CallWindowProc(IntPtr previousWinProc, IntPtr hwnd, uint message, IntPtr wParam, IntPtr lParam);

		[DllImport("User32")]
		public static extern IntPtr SetWindowLongPtr(IntPtr hwnd, int nIndex, IntPtr newLong);

		[DllImport("User32")]
		public static extern IntPtr GetWindowLongPtr(IntPtr hwnd, int nIndex);

		[DllImport("User32")]
		public static extern void PostMessage(IntPtr hwnd, uint message, IntPtr wParam, IntPtr lParam);

		[DllImport("Kernel32.dll", CharSet = CharSet.Unicode)]
		public static extern bool CreateHardLink(string lpFileName, string lpExistingFileName, IntPtr lpSecurityAttributes);

		public const uint FLASHW_ALL = 3u;

		public const uint FLASHW_CAPTION = 1u;

		public const uint FLASHW_STOP = 0u;

		public const uint FLASHW_TIMER = 4u;

		public const uint FLASHW_TIMERNOFG = 12u;

		public const uint FLASHW_TRAY = 2u;

		public const uint INFINITE = 4294967295u;

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public class MEMORYSTATUSEX
		{
			public MEMORYSTATUSEX()
			{
				this.dwLength = (uint)Marshal.SizeOf(typeof(WindowsPlatform.MEMORYSTATUSEX));
			}

			public uint dwLength;

			public uint dwMemoryLoad;

			public ulong ullTotalPhys;

			public ulong ullAvailPhys;

			public ulong ullTotalPageFile;

			public ulong ullAvailPageFile;

			public ulong ullTotalVirtual;

			public ulong ullAvailVirtual;

			public ulong ullAvailExtendedVirtual;
		}

		public struct ProcessMemoryCountersEx
		{
			public uint cb;

			public uint PageFaultCount;

			public IntPtr PeakWorkingSetSize;

			public IntPtr WorkingSetSize;

			public IntPtr QuotaPeakPagedPoolUsage;

			public IntPtr QuotaPagedPoolUsage;

			public IntPtr QuotaPeakNonPagedPoolUsage;

			public IntPtr QuotaNonPagedPoolUsage;

			public IntPtr PagefileUsage;

			public IntPtr PeakPagefileUsage;

			public IntPtr PrivateUsage;
		}

		public enum EMessageBoxType : uint
		{
			MB_ABORTRETRYIGNORE = 2u,
			MB_CANCELTRYCONTINUE = 6u,
			MB_HELP = 16384u,
			MB_OK = 0u,
			MB_OKCANCEL,
			MB_RETRYCANCEL = 5u,
			MB_YESNO = 4u,
			MB_YESNOCANCEL = 3u
		}

		public enum EMessageBoxIcon : uint
		{
			MB_ICONEXCLAMATION = 48u,
			MB_ICONWARNING = 48u,
			MB_ICONINFORMATION = 64u,
			MB_ICONASTERISK = 64u,
			MB_ICONQUESTION = 32u,
			MB_ICONSTOP = 16u,
			MB_ICONERROR = 16u,
			MB_ICONHAND = 16u
		}

		public enum EMessageBoxDefaultButton : uint
		{
			MB_DEFBUTTON1,
			MB_DEFBUTTON2 = 256u,
			MB_DEFBUTTON3 = 512u,
			MB_DEFBUTTON4 = 768u
		}

		public enum EMessageBoxModality : uint
		{
			MB_APPLMODAL,
			MB_SYSTEMMODAL = 4096u,
			MB_TASKMODAL = 8192u
		}

		public enum EMiscelaneous : uint
		{
			MB_DEFAULT_DESKTOP_ONLY = 131072u,
			MB_RIGHT = 524288u,
			MB_RTLREADING = 1048576u,
			MB_SETFOREGROUND = 65536u,
			MB_TOPMOST = 262144u,
			MB_SERVICE_NOTIFICATION = 2097152u
		}

		public struct POINT
		{
			public POINT(int x, int y)
			{
				this.X = x;
				this.Y = y;
			}

			public int X;

			public int Y;
		}

		public enum ShowWindowCommands
		{
			Hide,
			Normal,
			ShowMinimized,
			Maximize,
			ShowMaximized = 3,
			ShowNoActivate,
			Show,
			Minimize,
			ShowMinNoActive,
			ShowNA,
			Restore,
			ShowDefault,
			ForceMinimize
		}

		public struct RECT
		{
			public int Left;

			public int Top;

			public int Right;

			public int Bottom;
		}

		public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

		public delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdc, ref WindowsPlatform.RECT lprcMonitor, IntPtr dwData);

		public struct FLASHWINFO
		{
			public uint cbSize;

			public IntPtr hwnd;

			public uint dwFlags;

			public uint uCount;

			public uint dwTimeout;
		}

		public static class HWND
		{
			public static readonly IntPtr NOTOPMOST = new IntPtr(-2);

			public static readonly IntPtr BROADCAST = new IntPtr(65535);

			public static readonly IntPtr TOPMOST = new IntPtr(-1);

			public static readonly IntPtr TOP = new IntPtr(0);

			public static readonly IntPtr BOTTOM = new IntPtr(1);
		}

		public static class SWP
		{
			public static readonly int NOSIZE = 1;

			public static readonly int NOMOVE = 2;

			public static readonly int NOZORDER = 4;

			public static readonly int NOREDRAW = 8;

			public static readonly int NOACTIVATE = 16;

			public static readonly int DRAWFRAME = 32;

			public static readonly int FRAMECHANGED = 32;

			public static readonly int SHOWWINDOW = 64;

			public static readonly int HIDEWINDOW = 128;

			public static readonly int NOCOPYBITS = 256;

			public static readonly int NOOWNERZORDER = 512;

			public static readonly int NOREPOSITION = 512;

			public static readonly int NOSENDCHANGING = 1024;

			public static readonly int DEFERERASE = 8192;

			public static readonly int ASYNCWINDOWPOS = 16384;
		}
	}
}
