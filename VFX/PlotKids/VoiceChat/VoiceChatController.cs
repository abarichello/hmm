using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.CustomMatch;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Infra;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Social;
using ClientAPI.Matchmaking.Lobby;
using ClientAPI.Objects;
using ClientAPI.Objects.Partial;
using ClientAPI.Service;
using ClientAPI.Service.Interfaces;
using ClientAPI.SteamP2PMessages;
using FMod;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Options;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX.PlotKids.VoiceChat
{
	public class VoiceChatController : SingletonMonoBehaviour<VoiceChatController>
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<VoiceChatTeamStatus> OnVoiceChatStatusChanged;

		public VoiceChatTeamStatus VoiceChatTeamStatus
		{
			get
			{
				return this._voiceChatTeamStatus;
			}
			set
			{
				if (this._voiceChatTeamStatus == value)
				{
					return;
				}
				this._voiceChatTeamStatus = value;
				this.hub.PlayerPrefs.SetInt("VoiceChatTeamStatus", (int)this._voiceChatTeamStatus);
				this.hub.PlayerPrefs.Save();
				this.RefreshVoiceChatAudioSources();
				if (this.OnVoiceChatStatusChanged != null)
				{
					this.OnVoiceChatStatusChanged(value);
				}
			}
		}

		public float VoiceChatVolume
		{
			get
			{
				return this.hub.Options.Audio.VoiceChatVolume * this.hub.Options.Audio.MasterVolume;
			}
		}

		public bool IsMicEnabled
		{
			get
			{
				return this._isMicEnabled;
			}
			set
			{
				this._isMicEnabled = value;
			}
		}

		private HMMHub hub
		{
			get
			{
				if (this._hub == null)
				{
					this._hub = GameHubBehaviour.Hub;
				}
				return this._hub;
			}
		}

		public void MuteMatchUserIfNeeded(MatchData matchData, PlayerData data)
		{
			if (data.IsBot)
			{
				return;
			}
			this.SetMuteUserStatus(true, data.UserId);
		}

		public void ToggleMuteUser(string universalID)
		{
			bool flag = this._mutedUserIDs.Contains(universalID);
			this.SetMuteUserStatus(!flag, universalID);
		}

		public void SetMuteUserStatus(bool targetMuteStatus, string universalID)
		{
			if (!targetMuteStatus)
			{
				if (this._mutedUserIDs.Contains(universalID))
				{
					this._mutedUserIDs.Remove(universalID);
				}
				return;
			}
			if (this._mutedUserIDs.Contains(universalID))
			{
				return;
			}
			this._mutedUserIDs.Add(universalID);
		}

		public bool IsUserMuted(string universalID)
		{
			return this._mutedUserIDs.Contains(universalID);
		}

		public bool IsUserSpeaking(string universalID)
		{
			VoiceChatAudioSource voiceChatAudioSource;
			return !string.IsNullOrEmpty(universalID) && this._activeAudioSourcesByUserID.TryGetValue(universalID, out voiceChatAudioSource) && voiceChatAudioSource.IsSpeaking();
		}

		public bool IsRecording()
		{
			return this._initialized && this.hub.ClientApi.voice.IsVoiceRecording();
		}

		private void Start()
		{
			AudioSettings.outputSampleRate = 11025;
			AudioSettings.speakerMode = AudioSpeakerMode.Mono;
			this.Init();
		}

		public void LoadPrefs()
		{
			this._voiceChatInputType = (VoiceChatInputType)this.hub.PlayerPrefs.GetInt("VoiceChatInputMode", 0);
			this._voiceChatTeamStatus = (VoiceChatTeamStatus)this.hub.PlayerPrefs.GetInt("VoiceChatTeamStatus", 1);
		}

		private void Init()
		{
			GroupManager groupManager = ManagerController.Get<GroupManager>();
			groupManager.OnGroupUpdate += this.OnGroupUpdated;
			groupManager.EvtGroupMemberLeft += this.OnGroupMemberLeft;
			groupManager.EvtGroupInviteAccepted += this.OnInviteAccepted;
			MatchManager matchManager = ManagerController.Get<MatchManager>();
			matchManager.EvtLobbyJoined += this.OnLobbyJoined;
			matchManager.EvtLobbyFinished += this.OnLobbyFinished;
			matchManager.EvtLobbyMemberAdded += this.OnLobbyMemberAdded;
			matchManager.EvtLobbyMemberRemoved += this.OnLobbyMemberRemoved;
			VoiceService.OnVoiceReceived += this.onVoiceReceived;
			HudWindowManager.Instance.OnGuiStateChange += this.UpdateTeamPlayersList;
			base.StartCoroutine(this.InitCoRoutine());
		}

		private IEnumerator InitCoRoutine()
		{
			while (this.hub.Swordfish.Connection == null)
			{
				yield return null;
			}
			this.hub.Swordfish.Connection.ListenToSwordfishConnected += this.OnSwordfishConnected;
			yield break;
		}

		private void UpdateTeamPlayersList(HudWindowManager.GuiGameState currentGuiGameState)
		{
			this.RefreshVoiceChatAudioSources();
		}

		private bool CanUseVoiceChat
		{
			get
			{
				return this.VoiceChatTeamStatus != VoiceChatTeamStatus.Disable && !SpectatorController.IsSpectating;
			}
		}

		public void RefreshVoiceChatAudioSources()
		{
			if (this.CanUseVoiceChat)
			{
				HudWindowManager.GuiGameState state = HudWindowManager.Instance.State;
				if (state != HudWindowManager.GuiGameState.Game && state != HudWindowManager.GuiGameState.PickScreen)
				{
					if (state == HudWindowManager.GuiGameState.MainMenu)
					{
						if (ManagerController.Get<MatchManager>().IsUserInLobby)
						{
							this.RefreshLobbyValidPlayerList();
						}
						else
						{
							this.RefreshGroupValidPlayerList();
						}
					}
				}
				else
				{
					this.RefreshInGameValidPlayerList();
				}
			}
			foreach (KeyValuePair<string, VoiceChatAudioSource> keyValuePair in this._activeAudioSourcesByUserID)
			{
				string key = keyValuePair.Key;
				if (!this.CanUseVoiceChat || !this._validTargetUserIdsHashSet.Contains(key))
				{
					this._auxIdsToRemove.Add(key);
				}
			}
			for (int i = 0; i < this._auxIdsToRemove.Count; i++)
			{
				this.RemoveVoiceChatAudioSourceIfNeeded(this._auxIdsToRemove[i]);
			}
			foreach (string universalIdFrom in this._validTargetUserIdsHashSet)
			{
				this.AddVoiceChatAudioSourceIfNeeded(universalIdFrom);
			}
			this._validTargetUserIdsHashSet.Clear();
		}

		private void RefreshGroupValidPlayerList()
		{
			if (!this.CanUseVoiceChat)
			{
				return;
			}
			Dictionary<string, GroupMember> groupMembersByID = ManagerController.Get<GroupManager>().GroupMembersByID;
			if (groupMembersByID.Count == 0)
			{
				return;
			}
			foreach (KeyValuePair<string, GroupMember> keyValuePair in groupMembersByID)
			{
				if (!keyValuePair.Value.IsPendingInviteToGroup())
				{
					HashSet<string> validTargetUserIdsHashSet = this._validTargetUserIdsHashSet;
					Dictionary<string, GroupMember>.Enumerator enumerator;
					KeyValuePair<string, GroupMember> keyValuePair2 = enumerator.Current;
					validTargetUserIdsHashSet.Add(keyValuePair2.Key);
				}
			}
		}

		private void RefreshInGameValidPlayerList()
		{
			if (!this.CanUseVoiceChat)
			{
				return;
			}
			List<PlayerData> players = this.hub.Players.Players;
			PlayerData currentPlayerData = this.hub.Players.CurrentPlayerData;
			for (int i = 0; i < players.Count; i++)
			{
				string userId = players[i].UserId;
				if (!players[i].IsNarrator)
				{
					if (currentPlayerData.Team == players[i].Team)
					{
						if (!(userId == currentPlayerData.UserId))
						{
							this._validTargetUserIdsHashSet.Add(players[i].UserId);
						}
					}
				}
			}
		}

		private void RefreshLobbyValidPlayerList()
		{
			if (!this.CanUseVoiceChat)
			{
				return;
			}
			MatchManager matchManager = ManagerController.Get<MatchManager>();
			if (!matchManager.IsUserInLobby)
			{
				SingletonMonoBehaviour<VoiceChatController>.Log.Error("RefreshLobbyValidPlayerList with IsInLobby false");
				return;
			}
			List<LobbyMember> allMembers = matchManager.CurrentLobby.GetAllMembers();
			for (int i = 0; i < allMembers.Count; i++)
			{
				this._validTargetUserIdsHashSet.Add(allMembers[i].UniversalID);
			}
		}

		private void OnSwordfishConnected()
		{
			this.hub.ClientApi.voice.Initialize(11025u);
			if (this._VoiceChatThread == null)
			{
				this._voice = this.hub.ClientApi.voice;
				this._VoiceChatThread = new Thread(new ThreadStart(this.VoiceChatThreadMethod))
				{
					IsBackground = true,
					Name = "SwordfishSoundReceiver"
				};
				this._VoiceChatThread.Start();
			}
			this._initialized = true;
		}

		private void OnInviteAccepted(GroupMessageEventArgs groupMessageEventArgs)
		{
			this.RefreshVoiceChatAudioSources();
		}

		private void OnGroupUpdated()
		{
			UnityEngine.Debug.LogWarning("[OnGroupUpdated] Running Callback");
			this.RefreshVoiceChatAudioSources();
		}

		private void OnLobbyFinished(Lobby lobby, LobbyMatchmakingMessage.LobbyMessageErrorType lobbyErrorType)
		{
			this.RefreshVoiceChatAudioSources();
		}

		private void OnLobbyMemberRemoved(MatchmakingLobbyMemberRemovedEventArgs matchmakingLobbyMemberRemovedEventArgs)
		{
			if (string.Equals(matchmakingLobbyMemberRemovedEventArgs.RemovedUniversalId, this.hub.User.UniversalId))
			{
				this.RefreshVoiceChatAudioSources();
				return;
			}
			this.RefreshVoiceChatAudioSources();
		}

		private void OnLobbyMemberAdded(MatchmakingLobbyMemberAddedEventArgs matchmakingLobbyMemberAddedEventArgs)
		{
			this.RefreshVoiceChatAudioSources();
		}

		private void OnLobbyJoined(Lobby lobby)
		{
			this.RefreshVoiceChatAudioSources();
		}

		private void AddVoiceChatAudioSourceIfNeeded(string universalIdFrom)
		{
			if (universalIdFrom == this.hub.User.UniversalId || this._activeAudioSourcesByUserID.ContainsKey(universalIdFrom))
			{
				return;
			}
			VoiceChatAudioSource voiceChatAudioSource = UnityEngine.Object.Instantiate<VoiceChatAudioSource>(this._baseVoiceChatAudioSource);
			voiceChatAudioSource.transform.parent = base.transform;
			voiceChatAudioSource.Init(universalIdFrom);
			this._activeAudioSourcesByUserID.Add(universalIdFrom, voiceChatAudioSource);
			this._cacheTargetUsers.Add(universalIdFrom);
		}

		private void RemoveVoiceChatAudioSourceIfNeeded(string universalIdFrom)
		{
			if (!this._activeAudioSourcesByUserID.ContainsKey(universalIdFrom))
			{
				return;
			}
			VoiceChatAudioSource voiceChatAudioSource = this._activeAudioSourcesByUserID[universalIdFrom];
			UnityEngine.Object.Destroy(voiceChatAudioSource.gameObject);
			this._activeAudioSourcesByUserID.Remove(universalIdFrom);
			this._cacheTargetUsers.Remove(universalIdFrom);
			if (this._mutedUserIDs.Contains(universalIdFrom))
			{
				this._mutedUserIDs.Remove(universalIdFrom);
			}
		}

		private void OnDisable()
		{
			lock (this)
			{
				this._voiceChatThreadShutDown = true;
				if (this._VoiceChatThread != null)
				{
					this._VoiceChatThread.Join();
				}
			}
			if (SingletonMonoBehaviour<ManagerController>.DoesInstanceExist())
			{
				GroupManager groupManager = ManagerController.Get<GroupManager>();
				groupManager.EvtGroupMemberLeft -= this.OnGroupMemberLeft;
				groupManager.EvtGroupInviteAccepted -= this.OnInviteAccepted;
				MatchManager matchManager = ManagerController.Get<MatchManager>();
				matchManager.EvtLobbyJoined -= this.OnLobbyJoined;
				matchManager.EvtLobbyFinished -= this.OnLobbyFinished;
				matchManager.EvtLobbyMemberAdded -= this.OnLobbyMemberAdded;
				matchManager.EvtLobbyMemberRemoved -= this.OnLobbyMemberRemoved;
			}
			if (this.hub != null && this.hub.ClientApi != null)
			{
				this.hub.Swordfish.Connection.ListenToSwordfishConnected -= this.OnSwordfishConnected;
				HudWindowManager.Instance.OnGuiStateChange -= this.UpdateTeamPlayersList;
			}
			ManagerController.Get<GroupManager>().OnGroupUpdate -= this.OnGroupUpdated;
			VoiceService.OnVoiceReceived -= this.onVoiceReceived;
		}

		private void OnGroupMemberLeft(Guid guid, GroupMember groupMember)
		{
			this.RefreshVoiceChatAudioSources();
		}

		public void Update()
		{
			if (!this._initialized)
			{
				return;
			}
			this.UpdateVoiceChatInputMode();
		}

		public VoiceChatInputType VoiceChatInputType
		{
			get
			{
				return this._voiceChatInputType;
			}
			set
			{
				if (this._voiceChatInputType == value)
				{
					return;
				}
				this._voiceChatInputType = value;
				if (value == VoiceChatInputType.Toggle)
				{
					this._isVoiceChatToggleEnabled = false;
				}
				this.hub.PlayerPrefs.SetInt("VoiceChatInputMode", (int)this._voiceChatInputType);
				this.hub.PlayerPrefs.Save();
			}
		}

		public void UpdateVoiceChatInputMode()
		{
			bool flag = this.VoiceChatTeamStatus == VoiceChatTeamStatus.Enable && this.GetMustRecordVoiceChat();
			if (flag)
			{
				this.TryStartRecording();
				return;
			}
			this.TryStopRecording();
		}

		private bool GetMustRecordVoiceChat()
		{
			bool flag = false;
			GameState.GameStateKind stateKind = this.hub.State.Current.StateKind;
			if (stateKind != GameState.GameStateKind.MainMenu)
			{
				if (stateKind == GameState.GameStateKind.Pick || stateKind == GameState.GameStateKind.Game)
				{
					if (SpectatorController.IsSpectating)
					{
						return false;
					}
					flag = (HudChatController.ChatMsgInputVisible || SocialModalGUI.Current.IsChatInputFieldSelected());
				}
			}
			else
			{
				flag = (HudChatController.ChatMsgInputVisible || SocialModalGUI.Current.IsChatInputFieldSelected());
			}
			if (!this._isMicEnabled || this._cacheTargetUsers.Count == 0 || this.hub.GuiScripts.AfkControllerGui.IsWindowVisible())
			{
				return false;
			}
			VoiceChatInputType voiceChatInputType = this.VoiceChatInputType;
			if (voiceChatInputType != VoiceChatInputType.Toggle)
			{
				return voiceChatInputType != VoiceChatInputType.Pressed || (!flag && ControlOptions.GetButton(ControlAction.PushToTalk));
			}
			if (!flag && ControlOptions.GetButtonDown(ControlAction.PushToTalk))
			{
				this._isVoiceChatToggleEnabled = !this._isVoiceChatToggleEnabled;
			}
			return this._isVoiceChatToggleEnabled;
		}

		public void TryStartRecording()
		{
			if (this.IsRecording())
			{
				return;
			}
			FMODAudioManager.PlayOneShotAt(this.sfx_ui_voicechat_pressed, base.transform.position, 0);
			this.hub.ClientApi.voice.StartRecording();
			VoiceFeedBackGUI.Instance.OnVoiceReceived(this.hub.User.UniversalId);
		}

		public void TryStopRecording()
		{
			if (!this.IsRecording())
			{
				return;
			}
			FMODAudioManager.PlayOneShotAt(this.sfx_ui_voicechat_released, base.transform.position, 0);
			this.hub.ClientApi.voice.StopRecording();
			if (!string.IsNullOrEmpty(this.hub.User.UniversalId))
			{
				VoiceFeedBackGUI.Instance.OnVoiceStopped(this.hub.User.UniversalId);
			}
		}

		private void VoiceChatThreadMethod()
		{
			while (!this._voiceChatThreadShutDown)
			{
				while (this._voice.GetVoiceMessageCount() > 0)
				{
					this._voice.ProcessVoiceMessage();
				}
				if (this._voice.IsVoiceRecording())
				{
					if (this._cacheTargetUsers.Count > 0)
					{
						this._voice.SendAudio(this._cacheTargetUsers, true);
					}
					else
					{
						this._voice.ClearVoiceBuffer();
					}
				}
				Thread.Sleep(10);
			}
		}

		private void onVoiceReceived(string universalIdFrom, byte[] audiodata, DateTime datesent, DateTime datereceived)
		{
			if (!this._activeAudioSourcesByUserID.ContainsKey(universalIdFrom))
			{
				return;
			}
			this._activeAudioSourcesByUserID[universalIdFrom].onVoiceReceived(universalIdFrom, audiodata, datesent, datereceived);
		}

		private const string VoiceChatVolumePlayerPrefsKey = "VoiceChatVolume";

		private const string VoiceChatInputModePlayerPrefsKey = "VoiceChatInputMode";

		private const string VoiceChatTeamStatusPlayerPrefsKey = "VoiceChatTeamStatus";

		private Thread _VoiceChatThread;

		private bool _isVoiceChatToggleEnabled;

		private bool _voiceChatThreadShutDown;

		private GameGui _gameGui;

		private VoiceChatTeamStatus _voiceChatTeamStatus = VoiceChatTeamStatus.Enable;

		public FMODAsset sfx_ui_voicechat_pressed;

		public FMODAsset sfx_ui_voicechat_released;

		private bool _isMicEnabled = true;

		private bool _initialized;

		public const int SampleRate = 11025;

		private HMMHub _hub;

		private IVoice _voice;

		[SerializeField]
		private VoiceChatAudioSource _baseVoiceChatAudioSource;

		private Dictionary<string, VoiceChatAudioSource> _activeAudioSourcesByUserID = new Dictionary<string, VoiceChatAudioSource>();

		private HashSet<string> _mutedUserIDs = new HashSet<string>();

		private List<string> _auxIdsToRemove = new List<string>();

		private HashSet<string> _validTargetUserIdsHashSet = new HashSet<string>();

		private VoiceChatInputType _voiceChatInputType;

		public const float VoiceUploadIntervalSecs = 0.125f;

		private float _nextVoiceSendUpdate;

		private readonly List<string> _cacheTargetUsers = new List<string>();
	}
}
