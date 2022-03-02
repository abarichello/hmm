using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Infra;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Social;
using ClientAPI;
using ClientAPI.Exceptions;
using ClientAPI.Matchmaking.Lobby;
using ClientAPI.MessageHub;
using ClientAPI.Objects.Partial;
using HeavyMetalMachines.CustomMatch;
using HeavyMetalMachines.CustomMatch.DataTransferObjects;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.ParentalControl;
using HeavyMetalMachines.ParentalControl.Restrictions;
using HeavyMetalMachines.Players.Business;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.GenericConfirmWindow;
using HeavyMetalMachines.VoiceChat.Business;
using Hoplon.Localization.TranslationTable;
using Pocketverse;
using Pocketverse.MuralContext;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.VFX.PlotKids
{
	public class CustomMatchController : SingletonMonoBehaviour<CustomMatchController>, ICleanupListener
	{
		private static HMMHub Hub
		{
			get
			{
				HMMHub result;
				if ((result = CustomMatchController._hub) == null)
				{
					result = (CustomMatchController._hub = GameHubBehaviour.Hub);
				}
				return result;
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public static event CustomMatchController.LobbyCreateOrJoinErrorDelegate EvtCreateOrJoinLobbyError;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public static event Action OnLeaveLobby;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public static event Action OnJoinLobby;

		private void ThrowLobbyCreationOrJoinError(bool showErrorText, string errorMsg = null)
		{
			string format = string.Format("[CustomMatchController] ThrowLobbyCreationOrJoinError. Show Error Text: {0} Error Message: {1}", showErrorText, (!string.IsNullOrEmpty(errorMsg)) ? errorMsg : "null");
			if (showErrorText)
			{
				SingletonMonoBehaviour<CustomMatchController>.Log.ErrorFormat(format, new object[0]);
			}
			else
			{
				SingletonMonoBehaviour<CustomMatchController>.Log.DebugFormat(format, new object[0]);
			}
			if (CustomMatchController.EvtCreateOrJoinLobbyError != null)
			{
				CustomMatchController.EvtCreateOrJoinLobbyError(showErrorText, errorMsg);
			}
		}

		private void OnEnable()
		{
			base.StartCoroutine(this.Init());
		}

		private void OnDisable()
		{
			this.TryUnregisterMatchmakingInstabilityCallback();
			this.TrySubscribeLobbyEvents(false);
		}

		public void InitializeSwordfishConnected()
		{
			this.TrySubscribeLobbyEvents(true);
			base.StartCoroutine(this.Init());
		}

		public void DisposeFromSwordfishConnection()
		{
			this.TrySubscribeLobbyEvents(false);
			CustomMatchController.Hub.Swordfish.Connection.ListenToSwordfishConnected -= this.OnSwordfishConnected;
		}

		private IEnumerator Init()
		{
			while (CustomMatchController.Hub.Swordfish.Connection == null)
			{
				yield return null;
			}
			CustomMatchController.Hub.Swordfish.Connection.ListenToSwordfishConnected += this.OnSwordfishConnected;
			yield break;
		}

		private void OnSwordfishConnected()
		{
			this.TrySubscribeLobbyEvents(true);
		}

		private void TrySubscribeLobbyEvents(bool targetState)
		{
			if (this._subscribedToLobbyEvents == targetState)
			{
				return;
			}
			this._subscribedToLobbyEvents = targetState;
			if (targetState)
			{
				CustomMatchController.Hub.ClientApi.lobby.LobbyError += new EventHandlerEx<MatchmakingLobbyErrorEventArgs>(this.OnLobbyError);
				CustomMatchController.Hub.ClientApi.lobby.JoinedLobby += new EventHandlerEx<MatchmakingUpdateLobbyMembersEventArgs>(this.JoinedLobby);
				CustomMatchController.Hub.ClientApi.lobby.LobbyMemberAdded += new EventHandlerEx<MatchmakingLobbyMemberAddedEventArgs>(this.OnLobbyMemberAdded);
				return;
			}
			CustomMatchController.Hub.ClientApi.lobby.LobbyError -= new EventHandlerEx<MatchmakingLobbyErrorEventArgs>(this.OnLobbyError);
			CustomMatchController.Hub.ClientApi.lobby.JoinedLobby -= new EventHandlerEx<MatchmakingUpdateLobbyMembersEventArgs>(this.JoinedLobby);
		}

		private void OnLobbyMemberAdded(object sender, MatchmakingLobbyMemberAddedEventArgs eventargs)
		{
			this.BlockVoiceIfNeeded();
		}

		private void JoinedLobby(object sender, MatchmakingUpdateLobbyMembersEventArgs eventargs)
		{
			this.BlockVoiceIfNeeded();
		}

		private void BlockVoiceIfNeeded()
		{
			IPlayer player = this._getLocalPlayer.Get();
			Lobby lobby = CustomMatchController.Hub.ClientApi.lobby.GetCurrentLobby();
			PlayerIdentification[] array = (from p in lobby.GetMembers()
			select new PlayerIdentification
			{
				PlayerId = p.PlayerId,
				UniversalId = p.UniversalID
			}).ToArray<PlayerIdentification>();
			IFetchAndStorePlayersParentalControlInfo fetchAndStorePlayersParentalControlInfo = this._diContainer.Resolve<IFetchAndStorePlayersParentalControlInfo>();
			DisposableExtensions.AddTo<IDisposable>(ObservableExtensions.Subscribe<Unit>(Observable.SelectMany<Unit, Unit>(Observable.ContinueWith<Unit, Unit>(fetchAndStorePlayersParentalControlInfo.FetchAndStore(array), this.WaitForVoiceChatLoginIfNeeded()), (Unit _) => this.BlockVoiceChatIfNeeded(lobby.GetMembers()))), this);
		}

		private IObservable<Unit> WaitForVoiceChatLoginIfNeeded()
		{
			return Observable.Defer<Unit>(delegate()
			{
				if (this._voiceChatLogin.CheckLoggedOut())
				{
					return Observable.First<Unit>(this._voiceChatLogin.OnVoiceChatLogin());
				}
				return Observable.Return(Unit.Default);
			});
		}

		private IObservable<Unit> BlockVoiceChatIfNeeded(IEnumerable<LobbyMember> playersIds)
		{
			IVoiceChatRestriction voiceChatRestriction = this._diContainer.Resolve<IVoiceChatRestriction>();
			IBlockVoiceChat blockVoiceChat = this._diContainer.Resolve<IBlockVoiceChat>();
			return Observable.Merge<Unit>(from p in playersIds
			where voiceChatRestriction.IsEnabledByPlayer(p.PlayerId)
			select blockVoiceChat.BlockPlayer(p.PlayerId));
		}

		private void OnLobbyError(object sender, MatchmakingLobbyErrorEventArgs eventargs)
		{
			SingletonMonoBehaviour<CustomMatchController>.Log.Warn(string.Format("Lobby Error, cause: {0}. Is Joining Custom Match: {1}", eventargs.ErrorType, this._joiningCustomMatch));
			CustomMatchController.Hub.GuiScripts.SharedPreGameWindow.HideWaitingWindow(base.GetType());
			this.VerifyCustomMatchTypeError(eventargs);
			this._joiningCustomMatch = false;
		}

		private void VerifyCustomMatchTypeError(MatchmakingLobbyErrorEventArgs eventargs)
		{
			if (this._joiningCustomMatch)
			{
				string customMatchJoinErrorDraft = this.GetCustomMatchJoinErrorDraft(eventargs.ErrorType);
				string formatted = this._localizeKey.GetFormatted(customMatchJoinErrorDraft, TranslationContext.CustomMatch, new object[0]);
				this.ThrowErrorJoiningLobby(formatted, false, false);
				return;
			}
			string formatted2 = this._localizeKey.GetFormatted(CustomMatchController.GetCustomMatchErrorDialogText(eventargs.ErrorType), TranslationContext.CustomMatch, new object[0]);
			CustomMatchController.ShowCustomMatchDialog(formatted2);
		}

		private string GetCustomMatchJoinErrorDraft(LobbyMatchmakingMessage.LobbyMessageErrorType errorType)
		{
			switch (errorType)
			{
			case 0:
				return "CUSTOMMATCH_LOBBY_JOIN_ERROR_WRONGTOKEN";
			case 1:
				return "CUSTOMMATCH_LOBBY_JOIN_ERROR_BLOCKEDUSER";
			case 2:
				return "CUSTOMMATCH_LOBBY_OWNERLEAVE";
			case 3:
			case 5:
				return "CUSTOMMATCH_LOBBY_JOIN_ERROR_CONNECTIONEXCEPTION";
			case 4:
				return "CUSTOMMATCH_LOBBY_JOIN_ERROR_USERALREADYINU";
			case 6:
				return "CUSTOMMATCH_LOBBY_JOIN_ERROR_LOBBYFULL";
			default:
				if (errorType == 20)
				{
					return "CUSTOMMATCH_LOBBY_JOIN_ERROR_TIMEOUT";
				}
				if (errorType != 21)
				{
					return "CUSTOM_MATCH_HINT_NOT_POSSIBLE_UNEXPECTED_ERROR";
				}
				return "CUSTOMMATCH_LOBBY_JOIN_ERROR_USERNOTABLETOJOIN";
			}
		}

		private static string GetCustomMatchErrorDialogText(LobbyMatchmakingMessage.LobbyMessageErrorType errorType)
		{
			switch (errorType)
			{
			case 0:
			case 3:
			case 5:
				return "ERROR_CUSTOM_MATCH_WRONGTOKEN_OFFLINE_INMATCH";
			case 1:
				return "CUSTOMMATCH_LOBBY_KICKED";
			case 2:
				return "CUSTOMMATCH_LOBBY_OWNERLEAVE";
			case 4:
				return "CUSTOMMATCH_LOBBY_MATCHALLREADYSTART";
			case 6:
				return "CUSTOMMATCH_LOBBY_FULLROOM";
			default:
				switch (errorType)
				{
				case 20:
					return "CUSTOMMATCH_LOBBY_NOT_AVAILABLE";
				case 22:
					return "CUSTOMMATCH_LOBBY_NOT_AVAILABLE";
				}
				return "ERROR_CUSTOM_MATCH_WRONGTOKEN_OFFLINE_INMATCH";
			}
		}

		public static void ShowCustomMatchDialog(string message)
		{
			Guid confirmWindowGuid = Guid.NewGuid();
			ConfirmWindowProperties properties = new ConfirmWindowProperties
			{
				Guid = confirmWindowGuid,
				QuestionText = message,
				OkButtonText = Language.Get("Ok", TranslationContext.GUI),
				OnOk = delegate()
				{
					CustomMatchController.Hub.GuiScripts.ConfirmWindow.HideConfirmWindow(confirmWindowGuid);
				}
			};
			CustomMatchController.Hub.GuiScripts.ConfirmWindow.OpenConfirmWindow(properties);
			SingletonMonoBehaviour<PanelController>.Instance.SendSystemMessage(message, "SystemMessage", true, false, StackableHintKind.None, HintColorScheme.System);
		}

		private void ThrowNetworkErrorJoiningLobby(ConnectionException exception, bool isLobbyCreation)
		{
			this.ThrowErrorJoiningLobby(exception.Message, true, true);
		}

		private void ThrowErrorJoiningLobby(string exceptionMessage, bool isLobbyCreation, bool showGenericMessage)
		{
			CustomMatchController.Hub.GuiScripts.SharedPreGameWindow.HideWaitingWindow(base.GetType());
			this.LeaveLobby();
			SingletonMonoBehaviour<CustomMatchController>.Log.Warn(string.Format("[{0}-LOBBY] Exception entering lobby: {1}", (!isLobbyCreation) ? "JOIN" : "CREATE", exceptionMessage));
			string text = (!showGenericMessage) ? exceptionMessage : this._localizeKey.GetFormatted("CUSTOMMATCH_LOBBY_JOIN_ERROR_CONNECTIONEXCEPTION", TranslationContext.CustomMatch, new object[0]);
			this.ThrowLobbyCreationOrJoinError(false, text);
			CustomMatchController.ShowCustomMatchDialog(text);
		}

		public void CreateLobby(int maxSpectators = 2)
		{
			SingletonMonoBehaviour<CustomMatchController>.Log.Debug("Creating Lobby");
			if (this._joiningCustomMatch)
			{
				SingletonMonoBehaviour<CustomMatchController>.Log.Debug("Already joining lobby. Ignoring CreateLobby request");
				return;
			}
			Lobby currentLobby = CustomMatchController.Hub.ClientApi.lobby.GetCurrentLobby();
			if (currentLobby != null && !currentLobby.Id.Id().Equals(Guid.Empty.ToString()))
			{
				string formatted = this._localizeKey.GetFormatted("CUSTOMMATCH_LOBBY_JOIN_ERROR_USERALREADYIN", TranslationContext.CustomMatch, new object[0]);
				this.ThrowLobbyCreationOrJoinError(true, formatted);
				return;
			}
			SingletonMonoBehaviour<RegionController>.Instance.UpdateCurrentRegionOnSFServer(false);
			SingletonMonoBehaviour<CustomMatchController>.Log.Debug("Create Custom Match");
			bool isLobbyCreation = true;
			base.StartCoroutine(this.CreateOrJoinLobbyCoRoutine(isLobbyCreation, null, maxSpectators));
		}

		public void JoinLobby(string accessCode, int maxSpectators = 2)
		{
			if (this._joiningCustomMatch)
			{
				SingletonMonoBehaviour<CustomMatchController>.Log.Debug("Already joining lobby. Ignoring JoinLobby request");
				return;
			}
			base.StartCoroutine(this.CreateOrJoinLobbyCoRoutine(false, accessCode, maxSpectators));
		}

		private IEnumerator CreateOrJoinLobbyCoRoutine(bool isLobbyCreation, string accessCode = null, int maxSpectators = 2)
		{
			if (this._joiningCustomMatch)
			{
				yield break;
			}
			SingletonMonoBehaviour<CustomMatchController>.Log.Debug(string.Format("CreateOrJoinLobbyCoRoutine - Is Lobby Creation: {0}", isLobbyCreation));
			this._joiningCustomMatch = true;
			this.ThrowLobbyCreationOrJoinError(false, null);
			CustomMatchController.Hub.GuiScripts.SharedPreGameWindow.ShowWaitingWindow(false, base.GetType());
			yield return null;
			GroupManager groupManager = ManagerController.Get<GroupManager>();
			if (groupManager.IsUserInGroupOrPendingInvite)
			{
				Future<bool> leaveGroupFuture = new Future<bool>();
				this.CheckIfPlayerLeaveGroupBeforeJoiningLobby(leaveGroupFuture);
				while (!leaveGroupFuture.IsDone)
				{
					yield return null;
				}
				if (!leaveGroupFuture.Result)
				{
					this._joiningCustomMatch = false;
					CustomMatchController.Hub.GuiScripts.SharedPreGameWindow.HideWaitingWindow(base.GetType());
					yield break;
				}
				groupManager.LeaveGroup(false);
				if (groupManager.IsUserInGroupOrPendingInvite)
				{
					CustomMatchController.Hub.Swordfish.Log.BILogClient(77, true);
				}
				float leaveGroupTimeout = Time.unscaledTime + 5f;
				while (groupManager.IsUserInGroupOrPendingInvite)
				{
					yield return null;
					if (Time.unscaledTime >= leaveGroupTimeout)
					{
						SingletonMonoBehaviour<CustomMatchController>.Log.Warn("CUSTOMMATCH LOBBY STILL IN GROUP TIMEOUT");
						string exceptionMessage = this._localizeKey.GetFormatted("CUSTOMMATCH_LOBBY_STILL_IN_GROUP", TranslationContext.CustomMatch, new object[0]);
						this.ThrowErrorJoiningLobby(exceptionMessage, isLobbyCreation, false);
						CustomMatchController.Hub.GuiScripts.SharedPreGameWindow.HideWaitingWindow(base.GetType());
						yield break;
					}
				}
			}
			if (CustomMatchController.OnJoinLobby != null)
			{
				CustomMatchController.OnJoinLobby();
			}
			this.TryRegisterMatchmakingInstabilityCallback();
			if (isLobbyCreation)
			{
				this.CreateLobbyAndJoin(maxSpectators);
				yield break;
			}
			CustomMatchController.Hub.ClientApi.lobby.Join(null, accessCode, delegate(object state, ConnectionException exception)
			{
				this.ThrowNetworkErrorJoiningLobby(exception, false);
			});
			yield break;
		}

		private void CreateLobbyAndJoin(int maxSpectators)
		{
			SerializableCustomMatchLobby serializableCustomMatchLobby = CustomMatchLobbyConversions.ToSerializable(new CustomMatchLobby
			{
				Subtype = this._customMatchLobbyService.GetCustomMatchSubtypeFromLocalPlayer()
			});
			CustomMatchController.Hub.ClientApi.lobby.CreateLobby(null, maxSpectators, serializableCustomMatchLobby.Serialize(), delegate(object state, ConnectionException exception)
			{
				this.ThrowNetworkErrorJoiningLobby(exception, true);
			});
		}

		private void CheckIfPlayerLeaveGroupBeforeJoiningLobby(Future<bool> leaveGroupFuture)
		{
			this._leaveGroupConfirmWindowGuid = Guid.NewGuid();
			this._leaveGroupFuture = leaveGroupFuture;
			ConfirmWindowProperties properties = new ConfirmWindowProperties
			{
				Guid = this._leaveGroupConfirmWindowGuid,
				QuestionText = this._localizeKey.GetFormatted("CUSTOMMATCH_LOBBY_LEAVE_GROUP", TranslationContext.CustomMatch, new object[0]),
				ConfirmButtonText = this._localizeKey.GetFormatted("Yes", TranslationContext.GUI, new object[0]),
				RefuseButtonText = this._localizeKey.GetFormatted("No", TranslationContext.GUI, new object[0]),
				OnConfirm = delegate()
				{
					this._leaveGroupFuture.Result = true;
					this.HideLeaveGroupConfirmWindow();
				},
				OnRefuse = new Action(this.TryToCancelPlayerLeaveGroupBeforeJoiningLobby)
			};
			CustomMatchController.Hub.GuiScripts.ConfirmWindow.OpenConfirmWindow(properties);
		}

		private void HideLeaveGroupConfirmWindow()
		{
			CustomMatchController.Hub.GuiScripts.ConfirmWindow.HideConfirmWindow(this._leaveGroupConfirmWindowGuid);
			this._leaveGroupConfirmWindowGuid = Guid.Empty;
		}

		public void TryToCancelPlayerLeaveGroupBeforeJoiningLobby()
		{
			if (this._leaveGroupConfirmWindowGuid != Guid.Empty)
			{
				this._leaveGroupFuture.Result = false;
				this.HideLeaveGroupConfirmWindow();
			}
		}

		public void ConnectSpectatorToCustomMatch(string accessCode)
		{
			if (this._joiningCustomMatch)
			{
				SingletonMonoBehaviour<CustomMatchController>.Log.Warn("[CustomMatchController] Already Logging In to Custom Match!");
				return;
			}
			this._joiningCustomMatch = true;
			CustomMatchController._hub.ClientApi.lobby.JoinStoryteller(null, accessCode, new SwordfishClientApi.NetworkErrorCallback(this.OnJoinStorytellerError));
		}

		private void OnJoinStorytellerError(object state, ConnectionException exception)
		{
			this._joiningCustomMatch = false;
			SingletonMonoBehaviour<CustomMatchController>.Log.Fatal("Failed to join Storyteller", exception);
		}

		public void LeaveLobby()
		{
			this._joiningCustomMatch = false;
			if (CustomMatchController.OnLeaveLobby != null)
			{
				CustomMatchController.OnLeaveLobby();
			}
		}

		public void OnCleanup(CleanupMessage msg)
		{
			if (this._joiningCustomMatch)
			{
				this.LeaveLobby();
			}
		}

		private void TryRegisterMatchmakingInstabilityCallback()
		{
			if (this._matchmakingInstabilityCallbackRegistered)
			{
				return;
			}
			if (CustomMatchController._hub != null)
			{
				CustomMatchController._hub.State.ListenToStateChanged += this.ListenToStateChanged;
				CustomMatchController._hub.ClientApi.hubClient.ConnectionInstability += new EventHandlerEx<ConnectionInstabilityMessage>(this.HubClientOnConnectionInstabilityCreateOrJoinLobby);
			}
			this._matchmakingInstabilityCallbackRegistered = true;
		}

		private void TryUnregisterMatchmakingInstabilityCallback()
		{
			if (!this._matchmakingInstabilityCallbackRegistered)
			{
				return;
			}
			if (CustomMatchController._hub != null)
			{
				CustomMatchController._hub.State.ListenToStateChanged -= this.ListenToStateChanged;
				CustomMatchController._hub.ClientApi.hubClient.ConnectionInstability -= new EventHandlerEx<ConnectionInstabilityMessage>(this.HubClientOnConnectionInstabilityCreateOrJoinLobby);
			}
			this._matchmakingInstabilityCallbackRegistered = false;
		}

		private void HubClientOnConnectionInstabilityCreateOrJoinLobby(object sender, ConnectionInstabilityMessage eventArgs)
		{
			this.TryUnregisterMatchmakingInstabilityCallback();
			ObservableExtensions.Subscribe<Unit>(this.ShowLostConnectionWindow("CustomMatchController ConnectionInstability"));
		}

		private IObservable<Unit> ShowLostConnectionWindow(string eventName)
		{
			this._joiningCustomMatch = false;
			DialogConfiguration dialogConfiguration = new DialogConfiguration
			{
				Message = this._localizeKey.Get("LostMessageHubConnection", TranslationContext.GUI)
			};
			return Observable.Do<Unit>(this._genericConfirmWindowPresenter.Show(dialogConfiguration), delegate(Unit _)
			{
				CustomMatchController.Hub.EndSession(eventName);
			});
		}

		private void ListenToStateChanged(GameState ChangedState)
		{
			if (ChangedState.StateKind != GameState.GameStateKind.MainMenu)
			{
				this._joiningCustomMatch = false;
				this.TryUnregisterMatchmakingInstabilityCallback();
			}
		}

		private static HMMHub _hub;

		private bool _joiningCustomMatch;

		private bool _matchmakingInstabilityCallbackRegistered;

		private Guid _leaveGroupConfirmWindowGuid = Guid.Empty;

		private Future<bool> _leaveGroupFuture;

		[Inject]
		private ICustomMatchLobbyService _customMatchLobbyService;

		[Inject]
		private IGenericConfirmWindowPresenter _genericConfirmWindowPresenter;

		[Inject]
		private ILocalizeKey _localizeKey;

		[Inject]
		private IGetLocalPlayer _getLocalPlayer;

		[Inject]
		private IVoiceChatLogin _voiceChatLogin;

		[Inject]
		private DiContainer _diContainer;

		private const string CustomMatchWrongTokenOrInMatchErrorDraft = "ERROR_CUSTOM_MATCH_WRONGTOKEN_OFFLINE_INMATCH";

		private const string CustomMatchJoinErrorDraft = "CUSTOMMATCH_LOBBY_JOIN_ERROR_CONNECTIONEXCEPTION";

		private bool _subscribedToLobbyEvents;

		public delegate void LobbyCreateOrJoinErrorDelegate(bool showErrorText, string errorMsg);
	}
}
