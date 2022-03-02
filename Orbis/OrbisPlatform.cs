using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.HostingPlatforms;
using HeavyMetalMachines.ParentalControl;
using HeavyMetalMachines.Swordfish;
using Hoplon.Metrics.Api;
using Orbis;
using Performance;
using Pocketverse;
using UniRx;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

namespace HeavyMetalMachines.Orbis
{
	internal class OrbisPlatform : IPlatform
	{
		public OrbisPlatform()
		{
			this._imeDialogBuffer = Marshal.AllocHGlobal(512);
			this._imeDialogArray = new byte[512];
			this._layoutDetection = new OrbisKeyboardLayoutDetection();
		}

		[DllImport("orbis.plugin")]
		private static extern uint Initialize();

		[DllImport("orbis.plugin")]
		private static extern void SetOnSessionInvitationCallback(IntPtr functionAddress);

		public IObservable<SessionInvitationData> OnSessionInvitation
		{
			get
			{
				return this._onSessionInvitation;
			}
		}

		~OrbisPlatform()
		{
			Marshal.FreeHGlobal(this._imeDialogBuffer);
		}

		public void UnityStartup()
		{
		}

		private void InitializePlugin()
		{
			OrbisPlatform.Initialize();
			OrbisPlatform.SetOnSessionInvitationCallback(OrbisSessionInvitation.GetOnSessionInvitationDelegateAddress());
			OrbisSessionInvitation.OnOrbisSessionInvitation += this.HandleOnNativeSessionInvitation;
		}

		private void HandleOnNativeSessionInvitation(OrbisSessionInvitationData orbisData)
		{
			SessionInvitationData sessionInvitationData = default(SessionInvitationData);
			sessionInvitationData.SessionId = orbisData.SessionId;
			sessionInvitationData.InvitationId = orbisData.InvitationId;
			sessionInvitationData.ReferralOnlineId = orbisData.ReferralOnlineId;
			sessionInvitationData.ReferralAccountId = orbisData.ReferralAccountId;
			SessionInvitationData sessionInvitationData2 = sessionInvitationData;
			if (this._shouldEnqueueSessionInvitations)
			{
				this._sessionInvitationDataQueue.Enqueue(sessionInvitationData2);
			}
			else
			{
				this._onSessionInvitation.OnNext(sessionInvitationData2);
			}
		}

		public void StartNotifyingSessionInvitations()
		{
			OrbisPlatform.Log.Info("Stopped enqueuing and started notifying session invitations.");
			this._shouldEnqueueSessionInvitations = false;
		}

		public void StopNotifyingSessionInvitations()
		{
			OrbisPlatform.Log.Info("Stopped notifying and started enqueuing session invitations.");
			this._shouldEnqueueSessionInvitations = true;
		}

		public IEnumerable<SessionInvitationData> GetSessionInvitations()
		{
			while (this._sessionInvitationDataQueue.Count > 0)
			{
				SessionInvitationData sessionInvitationData = this._sessionInvitationDataQueue.Dequeue();
				yield return sessionInvitationData;
			}
			yield break;
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
			return string.Empty;
		}

		public string GetCurrentDirectory()
		{
			return "/app0";
		}

		public string GetLogsDirectory()
		{
			string text = Path.Combine(this.GetTemporaryDataDirectory(), "logs");
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			return text;
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
			return Observable.Defer<long>(delegate()
			{
				if (MsgDialog.Initialize() == 0U || MsgDialog.OpenSysMsg(UserService.GetInitialUser(), MsgDialog.SysMsgType.EmptyStore) == 0U)
				{
					MsgDialog.Terminate();
					return Observable.Throw<long>(new Exception());
				}
				IObservable<long> observable = Observable.Last<long>(Observable.TakeWhile<long>(Observable.EveryUpdate(), (long _) => MsgDialog.UpdateStatus() == CommonDialog.Status.Running));
				if (OrbisPlatform.<>f__mg$cache0 == null)
				{
					OrbisPlatform.<>f__mg$cache0 = new Action(MsgDialog.Terminate);
				}
				IObservable<long> observable2 = Observable.DoOnTerminate<long>(observable, OrbisPlatform.<>f__mg$cache0);
				if (OrbisPlatform.<>f__mg$cache1 == null)
				{
					OrbisPlatform.<>f__mg$cache1 = new Action(MsgDialog.Terminate);
				}
				return Observable.DoOnCancel<long>(observable2, OrbisPlatform.<>f__mg$cache1);
			});
		}

		public IObservable<string> ShowVirtualKeyboard(InputField.ContentType type, string initialText)
		{
			return Observable.Defer<string>(delegate()
			{
				if (ImeDialog.Initialize() == 0U)
				{
					return Observable.Throw<string>(new Exception());
				}
				ImeDialog.Type type2 = OrbisPlatform.TranslateKeyboardType(type);
				uint num;
				uint num2;
				if (ImeDialog.GetPanelSize(type2, out num, out num2) == 0U)
				{
					ImeDialog.Terminate();
					return Observable.Throw<string>(new Exception());
				}
				if (!string.IsNullOrEmpty(initialText))
				{
					int bytes = Encoding.Unicode.GetBytes(initialText, 0, initialText.Length, this._imeDialogArray, 0);
					Marshal.Copy(this._imeDialogArray, 0, this._imeDialogBuffer, bytes);
					Marshal.WriteInt16((IntPtr)(this._imeDialogBuffer.ToInt64() + (long)bytes), 0);
				}
				else
				{
					Marshal.WriteInt16(this._imeDialogBuffer, 0);
				}
				num = ((uint)Screen.width >> 1) - (num >> 1);
				num2 = ((uint)Screen.height >> 1) - (num2 >> 1);
				if (ImeDialog.Open(UserService.GetInitialUser(), type2, num, num2, this._imeDialogBuffer, 512) == 0U)
				{
					ImeDialog.Terminate();
					return Observable.Throw<string>(new Exception());
				}
				IObservable<string> observable = Observable.Select<long, string>(Observable.Last<long>(Observable.TakeWhile<long>(Observable.EveryUpdate(), (long _) => ImeDialog.GetStatus() == ImeDialog.Status.Running)), (long _) => (ImeDialog.GetResult() != ImeDialog.EndStatus.Ok) ? null : this.GetImeDialogText());
				if (OrbisPlatform.<>f__mg$cache2 == null)
				{
					OrbisPlatform.<>f__mg$cache2 = new Action(ImeDialog.Terminate);
				}
				return Observable.DoOnTerminate<string>(observable, OrbisPlatform.<>f__mg$cache2);
			});
		}

		public IObservable<Unit> OnResumedFromSuspension
		{
			get
			{
				return Observable.Never<Unit>();
			}
		}

		private string GetImeDialogText()
		{
			return Marshal.PtrToStringUni(this._imeDialogBuffer) ?? string.Empty;
		}

		private static ImeDialog.Type TranslateKeyboardType(InputField.ContentType type)
		{
			switch (type)
			{
			default:
				return ImeDialog.Type.Default;
			case 2:
			case 3:
				return ImeDialog.Type.Number;
			case 6:
				return ImeDialog.Type.Mail;
			}
		}

		public IObservable<long> ShowChatRestrictionDialog()
		{
			return Observable.Defer<long>(delegate()
			{
				if (MsgDialog.Initialize() == 0U || MsgDialog.OpenSysMsg(UserService.GetInitialUser(), MsgDialog.SysMsgType.PsnChatRestriction) == 0U)
				{
					MsgDialog.Terminate();
					return Observable.Throw<long>(new Exception());
				}
				IObservable<long> observable = Observable.Last<long>(Observable.TakeWhile<long>(Observable.EveryUpdate(), (long _) => MsgDialog.UpdateStatus() == CommonDialog.Status.Running));
				if (OrbisPlatform.<>f__mg$cache3 == null)
				{
					OrbisPlatform.<>f__mg$cache3 = new Action(MsgDialog.Terminate);
				}
				IObservable<long> observable2 = Observable.DoOnTerminate<long>(observable, OrbisPlatform.<>f__mg$cache3);
				if (OrbisPlatform.<>f__mg$cache4 == null)
				{
					OrbisPlatform.<>f__mg$cache4 = new Action(MsgDialog.Terminate);
				}
				return Observable.DoOnCancel<long>(observable2, OrbisPlatform.<>f__mg$cache4);
			});
		}

		public IObservable<long> ShowUGCRestrictionDialog()
		{
			return Observable.Defer<long>(delegate()
			{
				if (MsgDialog.Initialize() == 0U || MsgDialog.OpenSysMsg(UserService.GetInitialUser(), MsgDialog.SysMsgType.PsnUgcRestriction) == 0U)
				{
					MsgDialog.Terminate();
					return Observable.Throw<long>(new Exception());
				}
				IObservable<long> observable = Observable.Last<long>(Observable.TakeWhile<long>(Observable.EveryUpdate(), (long _) => MsgDialog.UpdateStatus() == CommonDialog.Status.Running));
				if (OrbisPlatform.<>f__mg$cache5 == null)
				{
					OrbisPlatform.<>f__mg$cache5 = new Action(MsgDialog.Terminate);
				}
				IObservable<long> observable2 = Observable.DoOnTerminate<long>(observable, OrbisPlatform.<>f__mg$cache5);
				if (OrbisPlatform.<>f__mg$cache6 == null)
				{
					OrbisPlatform.<>f__mg$cache6 = new Action(MsgDialog.Terminate);
				}
				return Observable.DoOnCancel<long>(observable2, OrbisPlatform.<>f__mg$cache6);
			});
		}

		public int GetPrivilegeId(Privileges privilege)
		{
			return 0;
		}

		public GameModeTabs GetExclusiveCasualQueueName()
		{
			return GameModeTabs.NormalPSN;
		}

		public GameModeTabs GetExclusiveRankedQueueName()
		{
			return GameModeTabs.RankedPSN;
		}

		public IObservable<bool> ObserveFocusChange()
		{
			return Observable.Never<bool>();
		}

		private static readonly BitLogger Log = new BitLogger(typeof(OrbisPlatform));

		private bool _shouldEnqueueSessionInvitations = true;

		private const int ImeDialogBufferSize = 512;

		private string npTitleId = string.Empty;

		private readonly IKeyboardLayoutDetection _layoutDetection;

		private readonly Queue<SessionInvitationData> _sessionInvitationDataQueue = new Queue<SessionInvitationData>(8);

		private readonly IntPtr _imeDialogBuffer;

		private readonly byte[] _imeDialogArray;

		private readonly Subject<SessionInvitationData> _onSessionInvitation = new Subject<SessionInvitationData>();

		[CompilerGenerated]
		private static Action <>f__mg$cache0;

		[CompilerGenerated]
		private static Action <>f__mg$cache1;

		[CompilerGenerated]
		private static Action <>f__mg$cache2;

		[CompilerGenerated]
		private static Action <>f__mg$cache3;

		[CompilerGenerated]
		private static Action <>f__mg$cache4;

		[CompilerGenerated]
		private static Action <>f__mg$cache5;

		[CompilerGenerated]
		private static Action <>f__mg$cache6;
	}
}
