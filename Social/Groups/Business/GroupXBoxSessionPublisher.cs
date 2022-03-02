using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.CustomMatch;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Social;
using Assets.Standard_Assets.Scripts.HMM.Swordfish.Services;
using ClientAPI;
using ClientAPI.Matchmaking.Lobby;
using ClientAPI.Objects;
using ClientAPI.Objects.Partial;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Matches;
using HeavyMetalMachines.MatchMaking;
using HeavyMetalMachines.MatchMakingQueue.Infra;
using HeavyMetalMachines.Players.Business;
using HeavyMetalMachines.Publishing.SessionService;
using Hoplon.Logging;
using Pocketverse;
using UniRx;
using Zenject;

namespace HeavyMetalMachines.Social.Groups.Business
{
	public class GroupXBoxSessionPublisher : IGroupXBoxSessionPublisher, IDisposable
	{
		public void Setup(SwordfishClientApi clientApi, IGroupService groupService, IGetThenObserveMatchmakingQueueState matchmakingQueueState)
		{
			this._clientApi = clientApi;
			this._matchmakingQueueState = matchmakingQueueState;
			this._groupService = groupService;
			this._currentState = new GameState
			{
				IsInitializing = true
			};
			this._soloGroupId = Guid.NewGuid();
			this._hudWindowManager.OnGuiStateChange += this.OnGuiStateChanged;
			this._groupService.GroupUndone += this.OnGroupUndone;
			this._groupService.MemberRemoved += this.OnMemberRemoved;
			this._groupService.MemberLeft += this.OnMemberLeft;
			this._groupService.NewMember += this.OnNewMember;
			this._groupService.GroupUpdated += this.OnGroupUpdated;
			this._groupService.InviteAccepted += this.OnInviteAccepted;
			this._groupService.GroupIsFull += this.OnGroupIsFull;
			this._groupManager.OnGroupUpdate += this.CheckCurrentState;
			this._customMatchManager.EvtLobbyJoined += this.OnLobbyJoined;
			this._customMatchManager.EvtLobbyFinished += this.OnLobbyFinished;
			this._disposables.Add(ObservableExtensions.Subscribe<MatchmakingQueueState>(Observable.Do<MatchmakingQueueState>(Observable.Skip<MatchmakingQueueState>(this._matchmakingQueueState.GetThenObserve(), 1), delegate(MatchmakingQueueState _)
			{
				this.CheckCurrentState();
			})));
			this._disposables.Add(ObservableExtensions.Subscribe<GameState.GameStateKind>(Observable.Do<GameState.GameStateKind>(this._stateMachine.StateChangedObservation(), delegate(GameState.GameStateKind _)
			{
				this.CheckCurrentState();
			})));
		}

		private void OnMatchStateChanged(MatchData.MatchState newmatchstate)
		{
			this._log.InfoFormat("MatchState changed={0}", new object[]
			{
				newmatchstate
			});
			this.CheckCurrentState();
		}

		private void OnGroupIsFull(object sender, GroupEventArgs groupEventArgs)
		{
			this._log.InfoFormat("OnGroupIsFull::{0}", new object[]
			{
				groupEventArgs.GroupId
			});
			this.CheckCurrentState();
		}

		private void OnGroupUndone(object sender, GroupEventArgs groupEventArgs)
		{
			this._log.InfoFormat("OnGroupUndone::{0}", new object[]
			{
				groupEventArgs.GroupId
			});
			this.CheckCurrentState();
		}

		private void OnMemberRemoved(object sender, GroupEventArgs groupEventArgs)
		{
			this._log.InfoFormat("OnMemberRemoved::{0}", new object[]
			{
				groupEventArgs.GroupId
			});
			this.CheckCurrentState();
		}

		private void OnMemberLeft(object sender, GroupEventArgs groupEventArgs)
		{
			this._log.InfoFormat("OnMemberLeft::{0}", new object[]
			{
				groupEventArgs.GroupId
			});
			this.CheckCurrentState();
		}

		private void OnNewMember(object sender, GroupEventArgs groupEventArgs)
		{
			this._log.InfoFormat("OnNewMember::{0}", new object[]
			{
				groupEventArgs.GroupId
			});
			this.CheckCurrentState();
		}

		private void OnGroupUpdated(object sender, GroupEventArgs groupEventArgs)
		{
			this._log.InfoFormat("OnGroupUpdated::{0}", new object[]
			{
				groupEventArgs.GroupId
			});
			this.CheckCurrentState();
		}

		private void OnInviteAccepted(object sender, GroupEventArgs groupEventArgs)
		{
			this._log.InfoFormat("OnInviteAccepted::{0}", new object[]
			{
				groupEventArgs.GroupId
			});
			this.CheckCurrentState();
		}

		public void Dispose()
		{
			this._hudWindowManager.OnGuiStateChange -= this.OnGuiStateChanged;
			this._groupService.GroupUndone -= this.OnGroupUndone;
			this._groupService.MemberRemoved -= this.OnMemberRemoved;
			this._groupService.MemberLeft -= this.OnMemberLeft;
			this._groupService.NewMember -= this.OnNewMember;
			this._groupService.GroupUpdated -= this.OnGroupUpdated;
			this._groupService.InviteAccepted -= this.OnInviteAccepted;
			this._groupService.GroupIsFull -= this.OnGroupIsFull;
			this._groupManager.OnGroupUpdate -= this.CheckCurrentState;
			this._customMatchManager.EvtLobbyJoined -= this.OnLobbyJoined;
			this._customMatchManager.EvtLobbyFinished -= this.OnLobbyFinished;
			foreach (IDisposable disposable in this._disposables)
			{
				disposable.Dispose();
			}
			this._clientApi = null;
		}

		public Guid CurrentSoloSessionId
		{
			get
			{
				return this._soloGroupId;
			}
		}

		private void OnLobbyJoined(Lobby lobby)
		{
			this.CheckCurrentState();
		}

		private void OnLobbyFinished(Lobby lobby, LobbyMatchmakingMessage.LobbyMessageErrorType lobbyerrortype)
		{
			this.CheckCurrentState();
		}

		private void OnGuiStateChanged(GuiGameState currentguigamestate)
		{
			this.CheckCurrentState();
		}

		private void CheckCurrentState()
		{
			this.RefreshCurrentState();
			this._sessionService.OnStateChanged(this._currentState);
		}

		private void RefreshCurrentState()
		{
			GameState currentState = this._currentState;
			if (currentState.InGroup && this._groupManager.CurrentGroupID == Guid.Empty)
			{
				this._soloGroupId = Guid.NewGuid();
			}
			this._currentState = new GameState();
			GameState.GameStateKind currentStateKind = this._stateMachine.CurrentStateKind;
			switch (currentStateKind)
			{
			case GameState.GameStateKind.None:
			case GameState.GameStateKind.Stater:
			case GameState.GameStateKind.Login:
				break;
			default:
			{
				switch (currentStateKind)
				{
				case GameState.GameStateKind.Splash:
				case GameState.GameStateKind.Reconnect:
				case GameState.GameStateKind.Welcome:
					goto IL_86;
				}
				this._currentState.IsInitializing = false;
				Match? ifExisting = this._currentMatch.GetIfExisting();
				this._currentState.InMatch = (ifExisting != null);
				this._currentState.MatchId = ((ifExisting == null) ? Guid.Empty : new Guid(ifExisting.Value.MatchId));
				this._currentState.InLobby = this._customMatchManager.IsUserInLobby;
				this._currentState.LobbyId = ((!this._customMatchManager.IsUserInLobby) ? string.Empty : this._customMatchManager.CurrentLobby.Id.Id());
				this._currentState.IsLobbyJoinable = this._customMatchManager.IsUserInLobby;
				if (this._groupManager.CurrentGroupID == Guid.Empty)
				{
					this._currentState.GroupId = this._soloGroupId;
					this._currentState.InGroup = false;
				}
				else
				{
					this._currentState.GroupId = this._groupManager.CurrentGroupID;
					this._currentState.InGroup = true;
				}
				this._currentState.IsGroupJoinable = this.CheckGroupJoinable();
				return;
			}
			}
			IL_86:
			this._currentState.IsInitializing = true;
			this._currentState.InGroup = false;
			this._currentState.InMatch = false;
			this._currentState.InLobby = false;
			this._currentState.GroupId = Guid.Empty;
			this._currentState.MatchId = Guid.Empty;
			this._currentState.LobbyId = string.Empty;
			this._currentState.IsGroupJoinable = false;
			this._currentState.IsLobbyJoinable = false;
		}

		private bool CheckGroupJoinable()
		{
			return this._hudWindowManager.State == GuiGameState.MainMenu && !this._customMatchManager.IsUserInLobby && !this._matchmakingService.IsWaitingInQueue() && !this.IsJoinRestrictedByGroupRules();
		}

		private bool IsJoinRestrictedByGroupRules()
		{
			Group[] currentGroups = this._groupService.GetCurrentGroups();
			if (currentGroups.Length == 0)
			{
				this._log.Debug("IsPlayerRestrictedByGroupRules::Player is not in a group");
				return false;
			}
			List<GroupMember> members = currentGroups[0].Members;
			if (members.Count >= 4)
			{
				this._log.Debug("IsPlayerRestrictedByGroupRules::Player group is at max capacity");
				return true;
			}
			bool flag = members.Any((GroupMember m) => !m.IsOwner && m.PlayerId == this._localPlayer.Player.PlayerId);
			if (flag)
			{
				this._log.Debug("IsPlayerRestrictedByGroupRules::Player is not owner of the group");
			}
			return flag;
		}

		[Inject]
		private IHudWindowManager _hudWindowManager;

		[Inject]
		private IMatchManager _customMatchManager;

		[Inject]
		private IMatchmakingService _matchmakingService;

		[Inject]
		private ILogger<GroupXBoxSessionPublisher> _log;

		[Inject]
		private ILocalPlayerStorage _localPlayer;

		[Inject]
		private IGroupManager _groupManager;

		[Inject]
		private IPublisherSessionService _sessionService;

		[Inject]
		private IGetCurrentMatch _currentMatch;

		[Inject]
		private IStateMachine _stateMachine;

		private IGetThenObserveMatchmakingQueueState _matchmakingQueueState;

		private SwordfishClientApi _clientApi;

		private IGroupService _groupService;

		private GameState _currentState;

		private Guid _soloGroupId;

		private List<IDisposable> _disposables = new List<IDisposable>();
	}
}
