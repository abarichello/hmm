using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.HostingPlatforms;
using HeavyMetalMachines.ParentalControl;
using HeavyMetalMachines.Swordfish;
using Hoplon.Metrics.Api;
using Performance;
using Pocketverse;
using UniRx;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

namespace HeavyMetalMachines.Durango
{
	internal class DurangoPlatform : IPlatform
	{
		public DurangoPlatform()
		{
			this._layoutDetection = new DurangoKeyboardLayoutDetection();
		}

		public void UnityStartup()
		{
		}

		public IGameQuitHandler CreateQuitHandler(SwordfishServices swordfish, bool isServer)
		{
			return new VoidGameQuitHandler();
		}

		public IPerformanceReader CreatePerformanceReader()
		{
			return new UnityPerformanceReader();
		}

		public IKeyboardLayoutDetection GetKeyboardLayoutDetection()
		{
			return this._layoutDetection;
		}

		public bool GetDisplays(ref List<Display> outList)
		{
			return false;
		}

		public bool GetDesktopRect(out RectInt rect)
		{
			rect = default(RectInt);
			return false;
		}

		public bool GetCursorPos(out Vector2Int pos)
		{
			pos = default(Vector2Int);
			return false;
		}

		public bool GetClipCursor(out RectInt rect)
		{
			rect = default(RectInt);
			return false;
		}

		public bool ClipCursor(ref RectInt rect)
		{
			return false;
		}

		public IntPtr GetCurrentWindowHandle(string clientWindowText)
		{
			return IntPtr.Zero;
		}

		public IntPtr GetForegroundWindow()
		{
			return IntPtr.Zero;
		}

		public bool ShowWindow(IntPtr window, bool minimize = false)
		{
			return true;
		}

		public bool SetTopmostWindow(IntPtr window, bool value)
		{
			return true;
		}

		public bool SetFlashWindow(IntPtr window, bool value)
		{
			return true;
		}

		public bool SetWindowPosition(IntPtr window, int x, int y, int resX, int resY)
		{
			return false;
		}

		public bool GetClientRectForClipping(IntPtr window, out RectInt rect)
		{
			rect = default(RectInt);
			return false;
		}

		public bool GetClientRectOffset(IntPtr window, out Vector2Int offset)
		{
			offset = default(Vector2Int);
			return false;
		}

		public bool MoveWindow(IntPtr window, int x, int y, int width, int height, bool repaint)
		{
			return false;
		}

		public string GetWindowText(IntPtr window)
		{
			return Application.productName;
		}

		public bool GetWindowRect(IntPtr window, out RectInt rect)
		{
			rect = default(RectInt);
			return false;
		}

		public void ErrorMessageBox(string text, string caption)
		{
		}

		public void RaiseException(uint code, uint flags, uint argCount, IntPtr arguments)
		{
		}

		public ulong GetAvailablePhysicalMemory()
		{
			return (ulong)Profiler.GetTotalAllocatedMemoryLong();
		}

		public string GetApplicationId()
		{
			return Application.identifier;
		}

		public string GetCurrentDirectory()
		{
			return "G:/";
		}

		public string GetLogsDirectory()
		{
			return "T:/logs";
		}

		public string GetPersistentDataDirectory()
		{
			string text = string.Format("{0}/{1}", Application.persistentDataPath, Application.productName);
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			return text;
		}

		public string GetTemporaryDataDirectory()
		{
			string text = Path.Combine(Application.temporaryCachePath, Application.productName);
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			return text;
		}

		public string GetSessionImagePath()
		{
			return string.Format("{0}/SessionImage.jpg", this.GetCurrentDirectory());
		}

		public CultureInfo GetSystemCulture()
		{
			return CultureInfo.CurrentCulture;
		}

		public bool CheckSingleApplicationInstance()
		{
			return true;
		}

		public bool IsConsole()
		{
			return true;
		}

		public bool HasPhysicalKeyboard()
		{
			return false;
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
				return Observable.Do<Unit>(DurangoPlatform.ListenForSuspensionEvent(), delegate(Unit secondsPassed)
				{
					DurangoPlatform.Log.InfoFormat("Resumed from suspension. SecondsPassedOnSuspension={0}", new object[]
					{
						secondsPassed
					});
				});
			}
		}

		private static IObservable<Unit> ListenForSuspensionEvent()
		{
			return Observable.Never<Unit>();
		}

		private void StartListeningForActivations()
		{
		}

		public IObservable<SessionInvitationData> OnSessionInvitation
		{
			get
			{
				return Observable.DoOnSubscribe<SessionInvitationData>(this._sessionInvitationSubject, delegate()
				{
					DurangoPlatform.Log.InfoFormat("Someone started listening for session invitations.", new object[0]);
				});
			}
		}

		public IEnumerable<SessionInvitationData> GetSessionInvitations()
		{
			DurangoPlatform.Log.InfoFormat("Session invitation list was requested. PendingSessionInvitations={0}", new object[]
			{
				this._pendingSessionInvitations.Count
			});
			while (this._pendingSessionInvitations.Count > 0)
			{
				yield return this._pendingSessionInvitations.Dequeue();
			}
			yield break;
		}

		public void StartNotifyingSessionInvitations()
		{
			DurangoPlatform.Log.Info("Stopped enqueuing and started notifying session invitations.");
			this._shouldEnqueueActivationUris = false;
		}

		public void StopNotifyingSessionInvitations()
		{
			DurangoPlatform.Log.Info("Stopped notifying and started enqueuing session invitations.");
			this._shouldEnqueueActivationUris = true;
		}

		public int GetPrivilegeId(Privileges privilege)
		{
			return 0;
		}

		public GameModeTabs GetExclusiveCasualQueueName()
		{
			return GameModeTabs.NormalXboxLive;
		}

		public GameModeTabs GetExclusiveRankedQueueName()
		{
			return GameModeTabs.RankedXboxLive;
		}

		public IObservable<bool> ObserveFocusChange()
		{
			return Observable.Never<bool>();
		}

		private static bool TryGenerateSessionInvitation(string uri, out SessionInvitationData sessionInvitationData)
		{
			Match match = Regex.Match(uri, "\\&handle\\=(.*)\\&");
			if (match.Length < 2)
			{
				DurangoPlatform.Log.InfoFormat("Protocol activated URI {0} does not contains a handleId.", new object[]
				{
					uri
				});
				sessionInvitationData = default(SessionInvitationData);
				return false;
			}
			Match match2 = Regex.Match(uri, "\\&(joineeXuid|senderXuid)\\=(.*)(\\&|$)");
			if (match2.Length < 3)
			{
				DurangoPlatform.Log.InfoFormat("Protocol activated URI {0} does not contains a joinee/senderXuid.", new object[]
				{
					uri
				});
				sessionInvitationData = default(SessionInvitationData);
				return false;
			}
			string value = match.Groups[1].Value;
			string value2 = match2.Groups[2].Value;
			DurangoPlatform.Log.InfoFormat("Generated session invitation from activation URI. HandleId={0} InvitationIssuerId={1}", new object[]
			{
				value,
				value2
			});
			SessionInvitationData sessionInvitationData2 = default(SessionInvitationData);
			sessionInvitationData2.SessionId = value;
			sessionInvitationData2.InvitationId = value;
			sessionInvitationData2.ReferralOnlineId = value2;
			sessionInvitationData = sessionInvitationData2;
			return true;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(DurangoPlatform));

		private readonly IKeyboardLayoutDetection _layoutDetection;

		private readonly Queue<SessionInvitationData> _pendingSessionInvitations = new Queue<SessionInvitationData>();

		private bool _shouldEnqueueActivationUris = true;

		private readonly Subject<SessionInvitationData> _sessionInvitationSubject = new Subject<SessionInvitationData>();
	}
}
