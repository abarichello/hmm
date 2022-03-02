using System;
using System.Diagnostics;
using ClientAPI;
using ClientAPI.Chat;
using ClientAPI.Matchmaking.Lobby;
using ClientAPI.Objects.Partial;
using ClientAPI.Service.API.Interfaces.Custom;
using Pocketverse;

namespace HeavyMetalMachines.CustomMatch
{
	public class SwordfishLobbyProxy : GameHubObject, ILobby
	{
		public Lobby GetCurrentLobby()
		{
			return GameHubObject.Hub.ClientApi.lobby.GetCurrentLobby();
		}

		public bool IsInLobby()
		{
			return GameHubObject.Hub.ClientApi.lobby.IsInLobby();
		}

		public void SetUsesRematch(bool isUsesRematch)
		{
			throw new NotImplementedException("SwordfishLobbyProxy.SetUsesRematch");
		}

		public bool IsUsesRematch()
		{
			throw new NotImplementedException("SwordfishLobbyProxy.IsUsesRematch");
		}

		public void CreateLobby(object state, int numberOfSpectators, string customMatchBag, SwordfishClientApi.NetworkErrorCallback errorCallback)
		{
			throw new NotImplementedException("SwordfishLobbyProxy.CreateLobby");
		}

		public void CreateLobby(object state, SwordfishClientApi.NetworkErrorCallback errorCallback)
		{
			throw new NotImplementedException("SwordfishLobbyProxy.CreateLobby");
		}

		public void CreateLobby(object state, int numberOfSpectators, SwordfishClientApi.NetworkErrorCallback errorCallback)
		{
			throw new NotImplementedException("SwordfishLobbyProxy.CreateLobby");
		}

		public void ChangeTeam(object state, string universalId, LobbyTeam lobbyTeam, SwordfishClientApi.NetworkErrorCallback errorCallback)
		{
			throw new NotImplementedException("SwordfishLobbyProxy.ChangeTeam");
		}

		public void SwapMembers(object state, string swappingUniversalId, string swapByUniversalId, SwordfishClientApi.NetworkErrorCallback errorCallback)
		{
			throw new NotImplementedException("SwordfishLobbyProxy.SwapMembers");
		}

		public void Join(object state, string lobbyToken, SwordfishClientApi.NetworkErrorCallback errorCallback)
		{
			throw new NotImplementedException("SwordfishLobbyProxy.Join");
		}

		public void JoinStoryteller(object state, string lobbyToken, SwordfishClientApi.NetworkErrorCallback errorCallback)
		{
			throw new NotImplementedException("SwordfishLobbyProxy.JoinStoryteller");
		}

		public void Leave(object state, SwordfishClientApi.NetworkErrorCallback errorCallback)
		{
			throw new NotImplementedException("SwordfishLobbyProxy.Leave");
		}

		public void RemoveAndBlock(object state, string lobbyToken, string universalId, SwordfishClientApi.NetworkErrorCallback errorCallback)
		{
			throw new NotImplementedException("SwordfishLobbyProxy.RemoveAndBlock");
		}

		public void SendChatMessage(object state, string message, SwordfishClientApi.NetworkErrorCallback errorCallback)
		{
			throw new NotImplementedException("SwordfishLobbyProxy.SendChatMessage");
		}

		public void SendChatMessage(object state, string message, string bag, SwordfishClientApi.NetworkErrorCallback errorCallback)
		{
			throw new NotImplementedException("SwordfishLobbyProxy.SendChatMessage");
		}

		public void RefreshLobby(object state, SwordfishClientApi.NetworkErrorCallback errorCallback)
		{
			throw new NotImplementedException("SwordfishLobbyProxy.RefreshLobby");
		}

		public string GetLobbyToken()
		{
			throw new NotImplementedException("SwordfishLobbyProxy.GetLobbyToken");
		}

		public void ChangeArena(string arena)
		{
			throw new NotImplementedException("SwordfishLobbyProxy.ChangeArena");
		}

		public string GetArena()
		{
			throw new NotImplementedException("SwordfishLobbyProxy.GetArena");
		}

		public void UpdateCustomMatchInfo(object state, string bagInfo, SwordfishClientApi.NetworkErrorCallback errorCallback)
		{
			throw new NotImplementedException("SwordfishLobbyProxy.UpdateCustomMatchInfo");
		}

		public void TransferLeadership(object state, string universalId, SwordfishClientApi.NetworkErrorCallback errorCallback)
		{
			throw new NotImplementedException("SwordfishLobbyProxy.TransferLeadership");
		}

		public void InviteMember(object state, string universalId, SwordfishClientApi.NetworkErrorCallback errorCallback)
		{
			throw new NotImplementedException("SwordfishLobbyProxy.InviteMember");
		}

		public void CancelInvite(object state, Guid inviteId, SwordfishClientApi.NetworkErrorCallback errorCallback)
		{
			throw new NotImplementedException("SwordfishLobbyProxy.CancelInvite");
		}

		public void SuggestInvite(object state, string universalId, SwordfishClientApi.NetworkErrorCallback errorCallback)
		{
			throw new NotImplementedException("SwordfishLobbyProxy.SuggestInvite");
		}

		public void AcceptInvite(object state, Guid inviteId, SwordfishClientApi.NetworkErrorCallback errorCallback)
		{
			throw new NotImplementedException("SwordfishLobbyProxy.AcceptInvite");
		}

		public void RejectInvite(object state, Guid inviteId, SwordfishClientApi.NetworkErrorCallback errorCallback)
		{
			throw new NotImplementedException("SwordfishLobbyProxy.RejectInvite");
		}

		public void GetInvitationPending()
		{
			throw new NotImplementedException("SwordfishLobbyProxy.GetInvitationPending");
		}

		public void GetSuggestionPending()
		{
			throw new NotImplementedException("SwordfishLobbyProxy.GetSuggestionPending");
		}

		public void AcceptJoin(string universalId)
		{
			throw new NotImplementedException("SwordfishLobbyProxy.AcceptJoin");
		}

		public void RejectJoin(string universalId, string reason)
		{
			throw new NotImplementedException("SwordfishLobbyProxy.RejectJoin");
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandlerEx<MatchmakingLobbyCreatedEventArgs> LobbyReady;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandlerEx<MatchmakingUpdateLobbyMembersEventArgs> JoinedLobby;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandlerEx<MatchmakingChangeLobbyMembersEventArgs> ChangeLobbyMember;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandlerEx<MatchmakingSwapLobbyMembersEventArgs> SwapLobbyMembers;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandlerEx<MatchmakingLobbyMemberAddedEventArgs> LobbyMemberAdded;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandlerEx<MatchmakingLobbyMemberRemovedEventArgs> LobbyMemberRemoved;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandlerEx<MatchmakingLobbyFinishedEventArgs> LobbyFinished;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandlerEx<MatchmakingLobbyErrorEventArgs> LobbyError;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandlerEx<MatchmakingLobbyCreatedEventArgs> LobbyRefreshed;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandlerEx<MatchmakingLobbyChangedArenaEventArgs> ChangedArena;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandlerEx<ChatMessage> ChatMessageReceived;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandlerEx<MatchmakingLobbyLeadershipChangedEventArgs> LeadershipChanged;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandlerEx<MatchmakingLobbyCustomMatchInfoUpdatedEventArgs> CustomMatchInfoUpdated;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandlerEx<MatchmakingLobbyInviteAcceptedEventArgs> InviteAccepted;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandlerEx<MatchmakingLobbyInvitedRejectedEventArgs> InviteRejected;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandlerEx<MatchmakingLobbySuggestReceiveEventArgs> SuggestReceived;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandlerEx<MatchmakingLobbySuggestAcceptedEventArgs> SuggestAccepted;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandlerEx<MatchmakingLobbySuggestRejectedEventArgs> SuggestRejected;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandlerEx<MatchmakingJoinIntentionEventArgs> JoinIntention;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandlerEx<MatchmakingLobbyJoinRejectedEventArgs> JoinRejected;
	}
}
