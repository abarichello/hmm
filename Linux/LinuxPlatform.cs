using System;
using System.Collections.Generic;
using System.Globalization;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.HostingPlatforms;
using HeavyMetalMachines.ParentalControl;
using HeavyMetalMachines.Swordfish;
using Hoplon.Metrics.Api;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Linux
{
	internal class LinuxPlatform : IPlatform
	{
		public void UnityStartup()
		{
			throw new NotImplementedException();
		}

		public IGameQuitHandler CreateQuitHandler(SwordfishServices swordfish, bool isServer)
		{
			throw new NotImplementedException();
		}

		public IPerformanceReader CreatePerformanceReader()
		{
			throw new NotImplementedException();
		}

		public IKeyboardLayoutDetection GetKeyboardLayoutDetection()
		{
			throw new NotImplementedException();
		}

		public bool GetDisplays(ref List<Display> outList)
		{
			throw new NotImplementedException();
		}

		public bool GetDesktopRect(out RectInt rect)
		{
			throw new NotImplementedException();
		}

		public bool GetCursorPos(out Vector2Int pos)
		{
			throw new NotImplementedException();
		}

		public bool GetClipCursor(out RectInt rect)
		{
			throw new NotImplementedException();
		}

		public bool ClipCursor(ref RectInt rect)
		{
			throw new NotImplementedException();
		}

		public IntPtr GetCurrentWindowHandle(string clientWindowText)
		{
			throw new NotImplementedException();
		}

		public IntPtr GetForegroundWindow()
		{
			throw new NotImplementedException();
		}

		public bool ShowWindow(IntPtr window, bool minimize = false)
		{
			throw new NotImplementedException();
		}

		public bool SetTopmostWindow(IntPtr window, bool value)
		{
			throw new NotImplementedException();
		}

		public bool SetFlashWindow(IntPtr window, bool value)
		{
			throw new NotImplementedException();
		}

		public bool SetWindowPosition(IntPtr window, int x, int y, int resX, int resY)
		{
			throw new NotImplementedException();
		}

		public bool GetClientRectForClipping(IntPtr window, out RectInt rect)
		{
			throw new NotImplementedException();
		}

		public bool GetClientRectOffset(IntPtr window, out Vector2Int offset)
		{
			throw new NotImplementedException();
		}

		public bool MoveWindow(IntPtr window, int x, int y, int width, int height, bool repaint)
		{
			throw new NotImplementedException();
		}

		public string GetWindowText(IntPtr window)
		{
			throw new NotImplementedException();
		}

		public bool GetWindowRect(IntPtr window, out RectInt rect)
		{
			throw new NotImplementedException();
		}

		public void ErrorMessageBox(string text, string caption)
		{
			throw new NotImplementedException();
		}

		public void RaiseException(uint code, uint flags, uint argCount, IntPtr arguments)
		{
			throw new NotImplementedException();
		}

		public ulong GetAvailablePhysicalMemory()
		{
			throw new NotImplementedException();
		}

		public string GetApplicationId()
		{
			throw new NotImplementedException();
		}

		public string GetCurrentDirectory()
		{
			throw new NotImplementedException();
		}

		public string GetLogsDirectory()
		{
			throw new NotImplementedException();
		}

		public string GetPersistentDataDirectory()
		{
			throw new NotImplementedException();
		}

		public string GetTemporaryDataDirectory()
		{
			throw new NotImplementedException();
		}

		public string GetSessionImagePath()
		{
			throw new NotImplementedException();
		}

		public CultureInfo GetSystemCulture()
		{
			throw new NotImplementedException();
		}

		public bool CheckSingleApplicationInstance()
		{
			throw new NotImplementedException();
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
			throw new NotImplementedException();
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
				throw new NotImplementedException();
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

		public int GetPrivilegeId(Privileges privilege)
		{
			return 0;
		}

		public GameModeTabs GetExclusiveCasualQueueName()
		{
			throw new NotImplementedException();
		}

		public GameModeTabs GetExclusiveRankedQueueName()
		{
			throw new NotImplementedException();
		}

		public IObservable<bool> ObserveFocusChange()
		{
			throw new NotImplementedException();
		}
	}
}
