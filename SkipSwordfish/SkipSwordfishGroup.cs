using System;
using System.Collections.Generic;
using System.Diagnostics;
using ClientAPI;
using ClientAPI.Matchmaking.Automatch;
using ClientAPI.Objects;
using ClientAPI.Service.API.Interfaces.Custom;
using ClientAPI.Service.Interfaces;
using ClientAPI.SteamP2PMessages;

namespace HeavyMetalMachines.SkipSwordfish
{
	public class SkipSwordfishGroup : IGroup
	{
		public Group CreateGroup(int maxMembers)
		{
			throw new NotImplementedException();
		}

		public Group CreateGroup(Guid groupId, int maxMembers)
		{
			throw new NotImplementedException();
		}

		public Group[] GetCurrentGroups()
		{
			throw new NotImplementedException();
		}

		public Group GetCurrentGroup()
		{
			throw new NotImplementedException();
		}

		public GroupMember GetOwner(Guid groupId)
		{
			throw new NotImplementedException();
		}

		public string GetUniversalUserId()
		{
			throw new NotImplementedException();
		}

		public Dictionary<Guid, string> GetCurrentInvites()
		{
			throw new NotImplementedException();
		}

		public GroupMember[] GetMembers(Guid groupId)
		{
			throw new NotImplementedException();
		}

		public GroupMember GetCurrentMember(Guid groupId)
		{
			throw new NotImplementedException();
		}

		public IGroupForMatchmakingAdapter GetMatchmakingAdapter()
		{
			throw new NotImplementedException();
		}

		public GroupMember GetMember(Guid groupId, string universalUserId)
		{
			throw new NotImplementedException();
		}

		public void Initialize()
		{
			throw new NotImplementedException();
		}

		public SwordfishImage GetMemberAvatarImage(string universalUserId, int width, int height)
		{
			throw new NotImplementedException();
		}

		public void InviteMember(Guid groupId, string universalUserId)
		{
			throw new NotImplementedException();
		}

		public void InviteMember(Guid groupId, string universalUserId, int millisecondsBeforeExpiring)
		{
			throw new NotImplementedException();
		}

		public void InviteMember(Guid groupId, string universalUserId, int millisecondsBeforeExpiring, string bag)
		{
			throw new NotImplementedException();
		}

		public void CancelInvite(Guid groupId, string universalUserId)
		{
			throw new NotImplementedException();
		}

		public void SuggestMember(Guid groupId, string universalUserId)
		{
			throw new NotImplementedException();
		}

		public void SuggestMember(Guid groupId, string universalUserId, string bag)
		{
			throw new NotImplementedException();
		}

		public void AcceptInvite(Guid groupId)
		{
			throw new NotImplementedException();
		}

		public void AcceptInvite(Guid groupId, string bag)
		{
			throw new NotImplementedException();
		}

		public void OnInviteReceived(string data)
		{
			throw new NotImplementedException();
		}

		public void RejectInvite(Guid groupId, string reason)
		{
			throw new NotImplementedException();
		}

		public void Remove(Guid groupId, string universalUserId)
		{
			throw new NotImplementedException();
		}

		public void SendMessage(Guid groupId, string text)
		{
			throw new NotImplementedException();
		}

		public void Leave(Guid groupId)
		{
			throw new NotImplementedException();
		}

		public void SendMessage(Guid groupId, string text, string bag)
		{
			throw new NotImplementedException();
		}

		public void TransferOwnership(Guid groupId, string transferOwnerUniversalId)
		{
			throw new NotImplementedException();
		}

		public void UpdateBag(Guid groupId, string bag)
		{
			throw new NotImplementedException();
		}

		public void RequestJoinGroup(string groupOwnerUniversalId, Guid groupId, int millisecondsBeforeExpiring, string bag)
		{
		}

		public void AcceptRequestJoinGroup(string newMemberUniversalId, Guid groupId, string bag)
		{
		}

		public void RejectRequestJoinGroup(string rejectedMemberUniversalId, Guid groupId, string reason, string bag)
		{
		}

		public void PublishCurrentSession()
		{
		}

		public void UnpublishCurrentSession()
		{
		}

		public void Clear()
		{
			throw new NotImplementedException();
		}

		public void Stop()
		{
			throw new NotImplementedException();
		}

		public void Terminate()
		{
			throw new NotImplementedException();
		}

		public void CreateGroupFromAutomatch(GroupCreationAutomatchEventArgs creationAutomatch, SwordfishClientApi.ParameterizedCallback<Group> successCallback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public Group GetGroup(Guid groupId)
		{
			throw new NotImplementedException();
		}

		public void LeaveAllGroups()
		{
			throw new NotImplementedException();
		}

		public List<string> GetCurrentInvitesSent(Guid groupId)
		{
			throw new NotImplementedException();
		}

		public bool IsCurrentInviteSent(Guid groupId, string universalId)
		{
			throw new NotImplementedException();
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandler<GroupMessageEventArgs> MessageReceived;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandler<GroupMessageEventArgs> InviteReceived;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandler<GroupMessageEventArgs> InviteCancelled;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandler<GroupMessageEventArgs> SuggestionReceived;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandler<GroupMessageEventArgs> JoinGroupRequestReceived;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandler<GroupMessageEventArgs> JoinGroupRequestAccepted;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandler<GroupMessageRejectJoinGroupEventArgs> JoinGroupRequestRejected;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandler<GroupMessageEventArgs> JoinGroupRequestCanceled;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandler<GroupErrorEventArgs> GroupError;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandler<GroupMessageEventArgs> GroupIsFull;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandler<GroupMessageEventArgs> InviteAccepted;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandler<GroupMessageEventArgs> InviteRejected;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandler<GroupMessageEventArgs> GroupUpdated;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandler<GroupMessageEventArgs> NewMember;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandler<GroupMessageEventArgs> MemberLeft;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandler<GroupMessageEventArgs> MemberRemoved;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandler<GroupMessageEventArgs> NewOwner;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandler<GroupMessageEventArgs> GroupUndone;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandler<GroupMessageEventArgs> BagUpdated;
	}
}
