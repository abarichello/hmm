using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.HostingPlatforms;
using HeavyMetalMachines.ParentalControl;
using HeavyMetalMachines.Swordfish;
using Hoplon.Metrics.Api;
using Performance;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Windows
{
	internal class WindowsPlatform : IPlatform
	{
		public WindowsPlatform()
		{
			this._flashInfo.Size = Convert.ToUInt32(Marshal.SizeOf(this._flashInfo));
			this._flashInfo.Count = uint.MaxValue;
			this._flashInfo.Timeout = 0U;
			this._keyboardDetection = new WindowsKeyboardLayoutDetection();
		}

		~WindowsPlatform()
		{
			if (this._singleAppInstanceMutex != null)
			{
				this._singleAppInstanceMutex.ReleaseMutex();
			}
		}

		public void UnityStartup()
		{
		}

		public IGameQuitHandler CreateQuitHandler(SwordfishServices swordfish, bool isServer)
		{
			return new WindowsGameQuitHandler(swordfish, isServer);
		}

		public IPerformanceReader CreatePerformanceReader()
		{
			return new NativePerformanceReader();
		}

		public IKeyboardLayoutDetection GetKeyboardLayoutDetection()
		{
			return this._keyboardDetection;
		}

		public bool GetDisplays(ref List<Display> outList)
		{
			this._displayWorkList = outList;
			bool result = User32.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, new User32.MonitorEnumProc(this.MonitorEnumProc), IntPtr.Zero);
			this._displayWorkList = null;
			return result;
		}

		public bool GetDesktopRect(out RectInt rect)
		{
			User32.Rectangle rectangle;
			bool windowRect = User32.GetWindowRect(User32.GetDesktopWindow(), out rectangle);
			rect = rectangle.AsRectInt();
			return windowRect;
		}

		public bool GetCursorPos(out Vector2Int pos)
		{
			User32.Point point;
			bool cursorPos = User32.GetCursorPos(out point);
			pos = point.AsVector2Int();
			return cursorPos;
		}

		public bool GetClipCursor(out RectInt rect)
		{
			User32.Rectangle rectangle;
			bool clipCursor = User32.GetClipCursor(out rectangle);
			rect = rectangle.AsRectInt();
			return clipCursor;
		}

		public bool ClipCursor(ref RectInt rect)
		{
			User32.Rectangle rectangle = new User32.Rectangle
			{
				Left = rect.xMin,
				Top = rect.yMin,
				Right = rect.xMax,
				Bottom = rect.yMax
			};
			return User32.ClipCursor(ref rectangle);
		}

		public IntPtr GetCurrentWindowHandle(string clientWindowText)
		{
			IntPtr windowHandle = IntPtr.Zero;
			uint currentThreadId = Kernel32.GetCurrentThreadId();
			User32.EnumThreadWindows(currentThreadId, (IntPtr hWnd, IntPtr lParam) => WindowsPlatform.EnumThreadWindowsCallback(hWnd, clientWindowText, out windowHandle), IntPtr.Zero);
			if (windowHandle == IntPtr.Zero)
			{
				User32.EnumWindows((IntPtr hWnd, IntPtr lParam) => WindowsPlatform.EnumWindowsCallback(hWnd, currentThreadId, out windowHandle), IntPtr.Zero);
			}
			if (windowHandle == IntPtr.Zero)
			{
				windowHandle = User32.FindWindow(null, clientWindowText);
			}
			return windowHandle;
		}

		public IntPtr GetForegroundWindow()
		{
			return User32.GetForegroundWindow();
		}

		public bool ShowWindow(IntPtr window, bool minimize = false)
		{
			return User32.ShowWindow(window, (!minimize) ? User32.ShowWindowCommands.Normal : User32.ShowWindowCommands.Minimize);
		}

		public bool SetTopmostWindow(IntPtr window, bool value)
		{
			return User32.SetWindowPos(window, (!value) ? User32.HWND.NOTOPMOST : User32.HWND.TOPMOST, 0, 0, 0, 0, User32.Swp.NoSize | User32.Swp.NoMove | User32.Swp.ShowWindow);
		}

		public bool SetFlashWindow(IntPtr window, bool value)
		{
			this._flashInfo.HWnd = window;
			this._flashInfo.Flags = ((!value) ? User32.FlashStatus.Stop : User32.FlashStatus.All);
			return User32.FlashWindowEx(ref this._flashInfo);
		}

		public bool SetWindowPosition(IntPtr window, int x, int y, int resX, int resY)
		{
			return User32.SetWindowPos(window, (IntPtr)0, x, y, resX, resY, (resX * resY != 0) ? ((User32.Swp)0) : User32.Swp.NoSize);
		}

		public bool GetClientRectForClipping(IntPtr window, out RectInt rect)
		{
			rect = default(RectInt);
			User32.Rectangle rectangle;
			if (!User32.GetClientRect(window, out rectangle))
			{
				return false;
			}
			User32.Point point;
			if (!User32.ClientToScreen(window, out point))
			{
				return false;
			}
			rectangle.Left = point.X;
			rectangle.Top = point.Y;
			rectangle.Right += point.X;
			rectangle.Bottom += point.Y;
			rect = rectangle.AsRectInt();
			return true;
		}

		public bool GetClientRectOffset(IntPtr window, out Vector2Int offset)
		{
			offset = Vector2Int.zero;
			User32.Rectangle rectangle;
			if (!User32.GetClientRect(window, out rectangle))
			{
				return false;
			}
			User32.Point point;
			User32.ClientToScreen(window, out point);
			offset = point.AsVector2Int();
			return true;
		}

		public bool MoveWindow(IntPtr window, int x, int y, int width, int height, bool repaint)
		{
			return User32.MoveWindow(window, x, y, width, height, repaint);
		}

		public string GetWindowText(IntPtr window)
		{
			int num = User32.GetWindowTextLength(window);
			StringBuilder stringBuilder = new StringBuilder(++num);
			User32.GetWindowText(window, stringBuilder, num);
			return stringBuilder.ToString();
		}

		public bool GetWindowRect(IntPtr window, out RectInt rect)
		{
			User32.Rectangle rectangle;
			bool windowRect = User32.GetWindowRect(window, out rectangle);
			rect = rectangle.AsRectInt();
			return windowRect;
		}

		public void ErrorMessageBox(string text, string caption)
		{
			User32.MessageBox(text, caption, User32.MessageBoxType.Ok, User32.MessageBoxIcon.IconStop, User32.MessageBoxOptions.None, User32.MessageBoxModality.TaskModal);
		}

		public void RaiseException(uint code, uint flags, uint argCount, IntPtr arguments)
		{
			Kernel32.RaiseException(code, flags, argCount, arguments);
		}

		public ulong GetAvailablePhysicalMemory()
		{
			Kernel32.MemoryStatusEx memoryStatusEx = new Kernel32.MemoryStatusEx();
			Kernel32.GlobalMemoryStatusEx(memoryStatusEx);
			return memoryStatusEx.AvailablePhysicalMemory / 1048576UL;
		}

		public string GetApplicationId()
		{
			return string.Empty;
		}

		public string GetCurrentDirectory()
		{
			return Directory.GetCurrentDirectory();
		}

		public string GetLogsDirectory()
		{
			return string.Format("{0}/logs", this.GetCurrentDirectory());
		}

		public string GetPersistentDataDirectory()
		{
			return this.GetCurrentDirectory();
		}

		public string GetTemporaryDataDirectory()
		{
			return Application.temporaryCachePath;
		}

		public string GetSessionImagePath()
		{
			return string.Format("{0}/SessionImage.jpg", this.GetCurrentDirectory());
		}

		public CultureInfo GetSystemCulture()
		{
			return new CultureInfo(Kernel32.GetSystemDefaultLCID());
		}

		public bool CheckSingleApplicationInstance()
		{
			bool result;
			this._singleAppInstanceMutex = new Mutex(false, "Global\\Hoplon_HMM_Handle", ref result);
			return result;
		}

		public bool IsConsole()
		{
			return false;
		}

		public bool HasPhysicalKeyboard()
		{
			return true;
		}

		public bool isInBackground()
		{
			return false;
		}

		public IObservable<long> ShowEmptyStoreDialog()
		{
			return Observable.Return<long>(0L);
		}

		public IObservable<string> ShowVirtualKeyboard(InputField.ContentType type, string initialText)
		{
			return Observable.Return<string>(string.Empty);
		}

		public IObservable<Unit> OnResumedFromSuspension
		{
			get
			{
				return Observable.Never<Unit>();
			}
		}

		public IObservable<SessionInvitationData> OnSessionInvitation { get; private set; }

		public IEnumerable<SessionInvitationData> GetSessionInvitations()
		{
			yield break;
		}

		public void StartNotifyingSessionInvitations()
		{
		}

		public void StopNotifyingSessionInvitations()
		{
		}

		private bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdc, ref User32.Rectangle lprcMonitor, IntPtr dwData)
		{
			Display item = new Display
			{
				Index = this._displayWorkList.Count,
				Rect = lprcMonitor.AsRectInt(),
				Res = new Resolution(lprcMonitor.Right - lprcMonitor.Left, lprcMonitor.Bottom - lprcMonitor.Top)
			};
			this._displayWorkList.Add(item);
			return true;
		}

		private static bool IsValidWindow(IntPtr hWnd, out int textSize)
		{
			textSize = 0;
			return User32.IsWindowVisible(hWnd) && (textSize = User32.GetWindowTextLength(hWnd)) > 0;
		}

		private static bool EnumWindowsCallback(IntPtr hWnd, uint currentThreadId, out IntPtr windowHandle)
		{
			windowHandle = IntPtr.Zero;
			int num;
			if (!WindowsPlatform.IsValidWindow(hWnd, out num))
			{
				return true;
			}
			if (currentThreadId == User32.GetWindowThreadProcessId(hWnd, IntPtr.Zero))
			{
				windowHandle = hWnd;
				return false;
			}
			return true;
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
			User32.GetWindowText(hWnd, stringBuilder, num);
			if (stringBuilder.ToString() == clientWindowText)
			{
				windowHandle = hWnd;
				return false;
			}
			return true;
		}

		public int GetPrivilegeId(Privileges privilege)
		{
			return 0;
		}

		public GameModeTabs GetExclusiveCasualQueueName()
		{
			throw new NotImplementedException("WindowsPlatform.GetExclusiveCasualQueueName");
		}

		public GameModeTabs GetExclusiveRankedQueueName()
		{
			throw new NotImplementedException("WindowsPlatform.GetExclusiveRankedQueueName");
		}

		public IObservable<bool> ObserveFocusChange()
		{
			return Observable.Never<bool>();
		}

		private readonly IKeyboardLayoutDetection _keyboardDetection;

		private User32.FlashWindowInfo _flashInfo;

		private List<Display> _displayWorkList;

		private Mutex _singleAppInstanceMutex;
	}
}
