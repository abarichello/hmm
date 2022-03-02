using System;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace HeavyMetalMachines.Windows
{
	internal static class User32
	{
		[DllImport("user32.dll")]
		public static extern bool ClipCursor(ref User32.Rectangle lpRect);

		[DllImport("user32.dll")]
		public static extern bool GetClipCursor(out User32.Rectangle lpRect);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, User32.Swp wFlags);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool GetClientRect(IntPtr hWnd, out User32.Rectangle lpRect);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool ClientToScreen(IntPtr hWnd, out User32.Point lpPoint);

		[DllImport("user32.dll")]
		public static extern IntPtr FindWindow(string className, string windowName);

		[DllImport("user32.dll")]
		public static extern bool GetWindowRect(IntPtr hWnd, out User32.Rectangle lpRect);

		[DllImport("user32.dll")]
		public static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool ShowWindow(IntPtr hWnd, User32.ShowWindowCommands nCmdShow);

		public static void MessageBox(string text, string caption, User32.MessageBoxType type = User32.MessageBoxType.Ok, User32.MessageBoxIcon icon = User32.MessageBoxIcon.IconInformation, User32.MessageBoxOptions options = User32.MessageBoxOptions.None, User32.MessageBoxModality modality = User32.MessageBoxModality.ApplModal)
		{
			User32.MessageBox(UIntPtr.Zero, text, caption, (uint)(type | (User32.MessageBoxType)icon | (User32.MessageBoxType)options | (User32.MessageBoxType)modality));
		}

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		private static extern int MessageBox(UIntPtr hWnd, string strText, string strCaption, uint nType);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetCursorPos(out User32.Point lpPoint);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool EnumThreadWindows(uint dwThreadId, User32.EnumWindowsProc lpEnumFunc, IntPtr lParam);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		public static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		public static extern int GetWindowTextLength(IntPtr hWnd);

		[DllImport("user32.dll")]
		public static extern IntPtr GetDesktopWindow();

		[DllImport("user32.dll")]
		public static extern bool EnumWindows(User32.EnumWindowsProc enumProc, IntPtr lParam);

		[DllImport("user32.dll")]
		public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, User32.MonitorEnumProc lpfnEnum, IntPtr dwData);

		[DllImport("user32.dll")]
		public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

		[DllImport("user32.dll")]
		public static extern bool IsWindowVisible(IntPtr hWnd);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr processId);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool FlashWindowEx(ref User32.FlashWindowInfo pwfi);

		[DllImport("user32.dll")]
		public static extern IntPtr GetKeyboardLayout(uint thread);

		[DllImport("user32.dll")]
		public static extern int GetKeyboardLayoutName([Out] StringBuilder layoutName);

		[DllImport("User32")]
		public static extern IntPtr CallWindowProc(IntPtr previousWinProc, IntPtr hWnd, User32.WindowMessage message, IntPtr wParam, IntPtr lParam);

		[DllImport("User32")]
		public static extern IntPtr SetWindowLongPtr(IntPtr hWnd, User32.WindowLongIndex nIndex, IntPtr newLong);

		[DllImport("User32")]
		public static extern void PostMessage(IntPtr hWnd, User32.WindowMessage message, IntPtr wParam, IntPtr lParam);

		public static int GetXLParam(IntPtr lParam)
		{
			return (int)((short)((long)lParam));
		}

		public static int GetYLParam(IntPtr lParam)
		{
			return (int)((short)((long)lParam >> 16));
		}

		public enum WindowMessage : uint
		{
			SysKeydown = 260U,
			SysCommand = 274U,
			Close = 16U
		}

		public enum SysCommand
		{
			Close = 61536
		}

		public enum WindowLongIndex
		{
			ExtendedStyle = -20,
			HInstance = -6,
			Id = -12,
			Style = -16,
			UserData = -21,
			WindowProc = -4
		}

		[Flags]
		public enum MessageBoxType : uint
		{
			AbortRetryIgnore = 2U,
			CancelTryContinue = 6U,
			Help = 16384U,
			Ok = 0U,
			OkCancel = 1U,
			RetryCancel = 5U,
			YesNo = 4U,
			YesNoCancel = 3U
		}

		[Flags]
		public enum MessageBoxIcon : uint
		{
			IconExclamation = 48U,
			IconWarning = 48U,
			IconInformation = 64U,
			IconAsterisk = 64U,
			IconQuestion = 32U,
			IconStop = 16U,
			IconError = 16U,
			IconHand = 16U
		}

		[Flags]
		public enum MessageBoxOptions : uint
		{
			None = 0U,
			DefaultDesktopOnly = 131072U,
			Right = 524288U,
			RtlReading = 1048576U,
			SetForeground = 65536U,
			TopMost = 262144U,
			ServiceNotification = 2097152U
		}

		[Flags]
		public enum MessageBoxModality : uint
		{
			ApplModal = 0U,
			SystemModal = 4096U,
			TaskModal = 8192U
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

		public struct Point
		{
			public Point(int x, int y)
			{
				this.X = x;
				this.Y = y;
			}

			public Vector2Int AsVector2Int()
			{
				return new Vector2Int(this.X, this.Y);
			}

			public int X;

			public int Y;
		}

		public struct Rectangle
		{
			public RectInt AsRectInt()
			{
				return new RectInt(this.Left, this.Top, this.Right - this.Left, this.Bottom - this.Top);
			}

			public int Left;

			public int Top;

			public int Right;

			public int Bottom;
		}

		public struct FlashWindowInfo
		{
			public uint Size;

			public IntPtr HWnd;

			public User32.FlashStatus Flags;

			public uint Count;

			public uint Timeout;
		}

		public enum FlashStatus : uint
		{
			All = 3U,
			Caption = 1U,
			Stop = 0U,
			Timer = 4U,
			TimerNoForeground = 12U,
			Tray = 2U
		}

		public static class HWND
		{
			public static readonly IntPtr NOTOPMOST = new IntPtr(-2);

			public static readonly IntPtr BROADCAST = new IntPtr(65535);

			public static readonly IntPtr TOPMOST = new IntPtr(-1);

			public static readonly IntPtr TOP = new IntPtr(0);

			public static readonly IntPtr BOTTOM = new IntPtr(1);
		}

		[Flags]
		public enum Swp
		{
			NoSize = 1,
			NoMove = 2,
			NoZOrder = 4,
			NoRedraw = 8,
			NoActivate = 16,
			DrawFrame = 32,
			FrameChanged = 32,
			ShowWindow = 64,
			HideWindow = 128,
			NoCopyBits = 256,
			NoOwnerZOrder = 512,
			NoReposition = 512,
			NoSendChanging = 1024,
			DeferErase = 8192,
			AsyncWindowPos = 16384
		}

		public enum VirtualKeyCode
		{
			F4 = 115
		}

		public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

		public delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdc, ref User32.Rectangle lprcMonitor, IntPtr dwData);
	}
}
