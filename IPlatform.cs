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

namespace HeavyMetalMachines
{
	public interface IPlatform
	{
		void UnityStartup();

		IGameQuitHandler CreateQuitHandler(SwordfishServices swordfish, bool isServer);

		IPerformanceReader CreatePerformanceReader();

		IKeyboardLayoutDetection GetKeyboardLayoutDetection();

		bool GetDisplays(ref List<Display> outList);

		bool GetDesktopRect(out RectInt rect);

		bool GetCursorPos(out Vector2Int pos);

		bool GetClipCursor(out RectInt rect);

		bool ClipCursor(ref RectInt rect);

		IntPtr GetCurrentWindowHandle(string clientWindowText);

		IntPtr GetForegroundWindow();

		bool ShowWindow(IntPtr window, bool minimize = false);

		bool SetTopmostWindow(IntPtr window, bool value);

		bool SetFlashWindow(IntPtr window, bool value);

		bool SetWindowPosition(IntPtr window, int x, int y, int resX, int resY);

		bool GetClientRectForClipping(IntPtr window, out RectInt rect);

		bool GetClientRectOffset(IntPtr window, out Vector2Int offset);

		bool MoveWindow(IntPtr window, int x, int y, int width, int height, bool repaint);

		string GetWindowText(IntPtr window);

		bool GetWindowRect(IntPtr window, out RectInt rect);

		void ErrorMessageBox(string text, string caption);

		void RaiseException(uint code, uint flags, uint argCount, IntPtr arguments);

		ulong GetAvailablePhysicalMemory();

		string GetApplicationId();

		string GetCurrentDirectory();

		string GetLogsDirectory();

		string GetPersistentDataDirectory();

		string GetTemporaryDataDirectory();

		string GetSessionImagePath();

		CultureInfo GetSystemCulture();

		bool CheckSingleApplicationInstance();

		bool IsConsole();

		bool HasPhysicalKeyboard();

		bool isInBackground();

		IObservable<long> ShowEmptyStoreDialog();

		IObservable<string> ShowVirtualKeyboard(InputField.ContentType type, string initialText);

		IObservable<Unit> OnResumedFromSuspension { get; }

		IObservable<SessionInvitationData> OnSessionInvitation { get; }

		IEnumerable<SessionInvitationData> GetSessionInvitations();

		void StartNotifyingSessionInvitations();

		void StopNotifyingSessionInvitations();

		int GetPrivilegeId(Privileges privilege);

		GameModeTabs GetExclusiveCasualQueueName();

		GameModeTabs GetExclusiveRankedQueueName();

		IObservable<bool> ObserveFocusChange();
	}
}
