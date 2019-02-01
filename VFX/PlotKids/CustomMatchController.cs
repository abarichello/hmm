using System;
using System.Collections;
using System.Diagnostics;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Infra;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Social;
using ClientAPI;
using ClientAPI.Exceptions;
using ClientAPI.Matchmaking.Lobby;
using ClientAPI.MessageHub;
using ClientAPI.Objects.Partial;
using HeavyMetalMachines.Frontend;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX.PlotKids
{
	public class CustomMatchController : SingletonMonoBehaviour<CustomMatchController>
	{
		public static HMMHub Hub
		{
			get
			{
				if (CustomMatchController._hub == null)
				{
					CustomMatchController._hub = GameHubBehaviour.Hub;
				}
				return CustomMatchController._hub;
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public static event CustomMatchController.LobbyCreateOrJoinErrorDelegate EvtCreateOrJoinLobbyError;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public static event System.Action OnLeaveLobby;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public static event System.Action OnJoinLobby;

		private void ThrowLobbyCreationOrJoinError(bool showErrorText, string errorMsg = null)
		{
			string format = string.Format("[CustomMatchController] ThrowLobbyCreationOrJoinError. Show Error Text: {0} Error Message: {1}", showErrorText, (!string.IsNullOrEmpty(errorMsg)) ? errorMsg : "null");
			if (showErrorText)
			{
				SingletonMonoBehaviour<CustomMatchController>.Log.ErrorFormat(format, new object[0]);
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
			this.TrySubscribeLobbyEvents(false);
		}

		private IEnumerator Init()
		{
			while (CustomMatchController.Hub.Swordfish.Connection == null)
			{
				yield return null;
			}
			CustomMatchController.Hub.Swordfish.Connection.ListenToSwordfishConnected += this.OnSwordfishConnected;
			CustomMatchController.Hub.ClientApi.lobby.LobbyReady += this.LobbyOnLobbyReady;
			CustomMatchController.Hub.ClientApi.lobby.JoinedLobby += this.LobbyOnJoinedLobby;
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
				CustomMatchController.Hub.ClientApi.lobby.LobbyError += this.OnLobbyError;
				return;
			}
			CustomMatchController.Hub.ClientApi.lobby.LobbyError -= this.OnLobbyError;
		}

		private void OnLobbyError(object sender, MatchmakingLobbyErrorEventArgs eventargs)
		{
			SingletonMonoBehaviour<CustomMatchController>.Log.Warn(string.Format("Lobby Error, cause: {0}. Is Joining Custom Match: {1}", eventargs.ErrorType, this._joiningCustomMatch));
			CustomMatchController.Hub.GuiScripts.SharedPreGameWindow.HideWaitinWindow(base.GetType());
			this.VerifyCustomMatchTypeError(eventargs);
			this._joiningCustomMatch = false;
		}

		private void VerifyCustomMatchTypeError(MatchmakingLobbyErrorEventArgs eventargs)
		{
			if (this._joiningCustomMatch)
			{
				string exceptionMessage;
				if (eventargs.ErrorType == LobbyMatchmakingMessage.LobbyMessageErrorType.WrongToken || eventargs.ErrorType == LobbyMatchmakingMessage.LobbyMessageErrorType.Offline || eventargs.ErrorType == LobbyMatchmakingMessage.LobbyMessageErrorType.InMatch)
				{
					exceptionMessage = Language.Get("ERROR_CUSTOM_MATCH_WrongToken_Offline_InMatch", TranslationSheets.Error);
				}
				else
				{
					exceptionMessage = Language.Get(string.Format("CUSTOMMATCH_LOBBY_JOIN_ERROR_{0}", eventargs.ErrorType), TranslationSheets.MainMenuGui);
				}
				this.ThrowErrorJoiningLobby(exceptionMessage, false, false);
				return;
			}
			string customMatchErrorDialogText = CustomMatchController.GetCustomMatchErrorDialogText(eventargs.ErrorType);
			CustomMatchController.ShowCustomMatchDialog(customMatchErrorDialogText);
		}

		private static string GetCustomMatchErrorDialogText(LobbyMatchmakingMessage.LobbyMessageErrorType errorType)
		{
			string result = null;
			switch (errorType)
			{
			case LobbyMatchmakingMessage.LobbyMessageErrorType.WrongToken:
			case LobbyMatchmakingMessage.LobbyMessageErrorType.Offline:
			case LobbyMatchmakingMessage.LobbyMessageErrorType.InMatch:
				result = Language.Get("ERROR_CUSTOM_MATCH_WrongToken_Offline_InMatch", TranslationSheets.Error);
				break;
			case LobbyMatchmakingMessage.LobbyMessageErrorType.BlockedUser:
				result = Language.Get("CUSTOMMATCH_LOBBY_KICKED", TranslationSheets.MainMenuGui);
				break;
			case LobbyMatchmakingMessage.LobbyMessageErrorType.OwnerLeaved:
				result = Language.Get("CUSTOMMATCH_LOBBY_OWNERLEAVE", TranslationSheets.MainMenuGui);
				break;
			case LobbyMatchmakingMessage.LobbyMessageErrorType.UserAlreadyIn:
				result = Language.Get("CUSTOMMATCH_LOBBY_MATCHALLREADYSTART", TranslationSheets.MainMenuGui);
				break;
			case LobbyMatchmakingMessage.LobbyMessageErrorType.LobbyFull:
				result = Language.Get("CUSTOMMATCH_LOBBY_FULL", TranslationSheets.MainMenuGui);
				break;
			default:
				if (errorType != LobbyMatchmakingMessage.LobbyMessageErrorType.NoServerAvailable)
				{
					if (errorType == LobbyMatchmakingMessage.LobbyMessageErrorType.None)
					{
						result = Language.Get("CUSTOMMATCH_LOBBY_NOT_AVAILABLE", TranslationSheets.MainMenuGui);
					}
				}
				else
				{
					result = Language.Get("CUSTOMMATCH_LOBBY_NOT_AVAILABLE", TranslationSheets.MainMenuGui);
				}
				break;
			}
			return result;
		}

		public static void ShowCustomMatchDialog(string message)
		{
			Guid confirmWindowGuid = Guid.NewGuid();
			ConfirmWindowProperties properties = new ConfirmWindowProperties
			{
				Guid = confirmWindowGuid,
				QuestionText = message,
				OkButtonText = Language.Get("Ok", TranslationSheets.GUI),
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
			CustomMatchController.Hub.GuiScripts.SharedPreGameWindow.HideWaitinWindow(base.GetType());
			this.LeaveLobby();
			SingletonMonoBehaviour<CustomMatchController>.Log.Warn(string.Format("[{0}-LOBBY] Exception entering lobby: {1}", (!isLobbyCreation) ? "JOIN" : "CREATE", exceptionMessage));
			string text = (!showGenericMessage) ? exceptionMessage : Language.Get("CUSTOMMATCH_LOBBY_JOIN_ERROR", TranslationSheets.MainMenuGui);
			this.ThrowLobbyCreationOrJoinError(true, text);
			CustomMatchController.ShowCustomMatchDialog(text);
		}

		public void CreateLobby(int maxSpectators = 2)
		{
			if (this._joiningCustomMatch)
			{
				return;
			}
			Lobby currentLobby = CustomMatchController.Hub.ClientApi.lobby.GetCurrentLobby();
			if (currentLobby != null && !currentLobby.Id.Id().Equals(Guid.Empty.ToString()))
			{
				this.ThrowLobbyCreationOrJoinError(true, Language.Get("CUSTOMMATCH_LOBBY_JOIN_ERROR_UserAlreadyIn", TranslationSheets.MainMenuGui));
				return;
			}
			bool isLobbyCreation = true;
			base.StartCoroutine(this.CreateOrJoinLobbyCoRoutine(isLobbyCreation, null, maxSpectators));
		}

		public void CreateOrJoinLobby(string accessCode, int maxSpectators = 2)
		{
			base.StartCoroutine(this.CreateOrJoinLobbyCoRoutine(false, accessCode, maxSpectators));
		}

		private IEnumerator CreateOrJoinLobbyCoRoutine(bool isLobbyCreation, string accessCode = null, int maxSpectators = 2)
		{
			if (this._joiningCustomMatch)
			{
				yield break;
			}
			this._joiningCustomMatch = true;
			this.ThrowLobbyCreationOrJoinError(false, null);
			CustomMatchController.Hub.GuiScripts.SharedPreGameWindow.ShowWaitingWindow(false, base.GetType());
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
					CustomMatchController.Hub.GuiScripts.SharedPreGameWindow.HideWaitinWindow(base.GetType());
					yield break;
				}
				groupManager.LeaveGroup(false);
				float leaveGroupTimeout = Time.unscaledTime + 5f;
				while (groupManager.IsUserInGroupOrPendingInvite)
				{
					yield return null;
					if (Time.unscaledTime >= leaveGroupTimeout)
					{
						this.ThrowErrorJoiningLobby(Language.Get("CUSTOMMATCH_LOBBY_STILL_IN_GROUP", TranslationSheets.MainMenuGui), isLobbyCreation, false);
						CustomMatchController.Hub.GuiScripts.SharedPreGameWindow.HideWaitinWindow(base.GetType());
						yield break;
					}
				}
			}
			if (CustomMatchController.OnJoinLobby != null)
			{
				CustomMatchController.OnJoinLobby();
			}
			CustomMatchController.Hub.ClientApi.hubClient.ConnectionInstability += this.HubClientOnConnectionInstabilityCreateOrJoinLobby;
			if (isLobbyCreation)
			{
				CustomMatchController.Hub.ClientApi.lobby.CreateLobby(null, maxSpectators, delegate(object state, ConnectionException exception)
				{
					this.ThrowNetworkErrorJoiningLobby(exception, true);
				});
				yield break;
			}
			CustomMatchController.Hub.ClientApi.lobby.Join(null, accessCode, delegate(object state, ConnectionException exception)
			{
				this.ThrowNetworkErrorJoiningLobby(exception, false);
			});
			yield break;
		}

		private void LobbyOnJoinedLobby(object sender, MatchmakingUpdateLobbyMembersEventArgs eventArgs)
		{
			this.JoinedLobby();
		}

		private void LobbyOnLobbyReady(object sender, MatchmakingLobbyCreatedEventArgs eventArgs)
		{
			this.JoinedLobby();
		}

		private void JoinedLobby()
		{
			CustomMatchController.Hub.ClientApi.hubClient.ConnectionInstability -= this.HubClientOnConnectionInstabilityCreateOrJoinLobby;
		}

		private void HubClientOnConnectionInstabilityCreateOrJoinLobby(object sender, ConnectionInstabilityMessage eventArgs)
		{
			this.ThrowNetworkErrorJoiningLobby(new ConnectionException("Connection instability detected."), true);
			CustomMatchController.Hub.ClientApi.hubClient.ConnectionInstability -= this.HubClientOnConnectionInstabilityCreateOrJoinLobby;
		}

		private void CheckIfPlayerLeaveGroupBeforeJoiningLobby(Future<bool> leaveGroupFuture)
		{
			Guid confirmWindowGuid = Guid.NewGuid();
			ConfirmWindowProperties properties = new ConfirmWindowProperties
			{
				Guid = confirmWindowGuid,
				QuestionText = Language.Get("CUSTOMMATCH_LOBBY_LEAVE_GROUP", TranslationSheets.MainMenuGui),
				ConfirmButtonText = Language.Get("Yes", TranslationSheets.GUI),
				RefuseButtonText = Language.Get("No", TranslationSheets.GUI),
				OnConfirm = delegate()
				{
					leaveGroupFuture.Result = true;
					CustomMatchController.Hub.GuiScripts.ConfirmWindow.HideConfirmWindow(confirmWindowGuid);
				},
				OnRefuse = delegate()
				{
					leaveGroupFuture.Result = false;
					CustomMatchController.Hub.GuiScripts.ConfirmWindow.HideConfirmWindow(confirmWindowGuid);
				}
			};
			CustomMatchController.Hub.GuiScripts.ConfirmWindow.OpenConfirmWindow(properties);
		}

		public void ConnectSpectatorToCustomMatch(string accessCode)
		{
			if (this._joiningCustomMatch)
			{
				UnityEngine.Debug.LogWarning("[CustomMatchController] Already Logging In to Custom Match!");
				return;
			}
			this._joiningCustomMatch = true;
			CustomMatchController._hub.ClientApi.lobby.JoinStoryteller(null, accessCode, new SwordfishClientApi.NetworkErrorCallback(this.OnJoinStorytellerError));
		}

		private void OnJoinStorytellerError(object state, ConnectionException exception)
		{
			this._joiningCustomMatch = false;
			UnityEngine.Debug.LogWarning("Failed to join Storyteller");
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

		private static HMMHub _hub;

		private bool _subscribedToLobbyEvents;

		private bool _joiningCustomMatch;

		public delegate void LobbyCreateOrJoinErrorDelegate(bool showErrorText, string errorMsg);
	}
}
