using System;
using System.Diagnostics;
using System.Linq;
using Assets.Standard_Assets.Scripts.HMM.SkipSwordfish;
using ClientAPI;
using ClientAPI.Enum;
using ClientAPI.Objects;
using ClientAPI.Service.Interfaces;
using ClientAPI.Social.Friend.FriendEventArgs;
using ClientAPI.Social.Player.EventArgs;
using HeavyMetalMachines.Publishing;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.SkipSwordfish
{
	public class SkipSwordfishFriendService : IFriend
	{
		public void UpdateFriendBag(string bag)
		{
			SkipSwordfishFriendService.Log.Info("UpdateFriendBag");
		}

		public string GetMyBag()
		{
			SkipSwordfishFriendService.Log.Info("GetMyBag");
			return string.Empty;
		}

		public void GetAllFriends(object state, Guid userId, SwordfishClientApi.ParameterizedCallback<UserFriend[]> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			callback.Invoke(state, MockedSocialFriendsServices.AllFriends.ToArray());
		}

		public UserFriend[] GetAllFriendsSync(Guid userId)
		{
			SkipSwordfishFriendService.Log.Info("GetAllFriendsSync");
			return new UserFriend[0];
		}

		public void AcceptInvite(object state, long inviteId, SwordfishClientApi.Callback callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishFriendService.Log.Info("AcceptInviteSync");
			if (this.FriendListChanged != null)
			{
				this.FriendListChanged(this, EventArgs.Empty);
			}
		}

		public void AcceptInviteSync(long inviteId)
		{
			SkipSwordfishFriendService.Log.Info("AcceptInviteSync");
		}

		public void SendInvite(object state, long toPlayerId, SwordfishClientApi.Callback callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishFriendService.Log.Info("SendInvite");
			Debug.LogError("SendInvite");
			if (MockedSocialFriendsServices.PlayerFriendSuggestionSF.Exists((PlayerFriendSuggestion p) => p.PlayerId == toPlayerId))
			{
				PlayerFriendSuggestion playerFriendSuggestion = MockedSocialFriendsServices.PlayerFriendSuggestionSF.FirstOrDefault((PlayerFriendSuggestion p) => p.PlayerId == toPlayerId);
				callback.Invoke(this);
				if (this.FriendListChanged != null)
				{
					MockedSocialFriendsServices.AllFriends.Add(new UserFriend
					{
						PlayerId = playerFriendSuggestion.PlayerId,
						PlayerName = playerFriendSuggestion.PlayerName,
						Publisher = Publishers.Psn.SwordfishUniqueName,
						UniversalId = playerFriendSuggestion.UniversalId,
						PlayerNameTag = playerFriendSuggestion.PlayerNameTag
					});
					this.FriendListChanged(this, EventArgs.Empty);
				}
			}
		}

		public void SendInviteSync(long toPlayerId)
		{
			SkipSwordfishFriendService.Log.Info("SendInviteSync");
		}

		public void SendInvite(object state, long[] toPlayerIds, SwordfishClientApi.Callback callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishFriendService.Log.Info("SendInvite");
		}

		public void SendInviteSync(long[] toPlayerIds)
		{
			SkipSwordfishFriendService.Log.Info("SendInviteSync");
		}

		public void GetFriends(object state, SwordfishClientApi.ParameterizedCallback<PlayerFriend[]> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishFriendService.Log.Info("GetFriends");
		}

		public PlayerFriend[] GetFriendsSync()
		{
			SkipSwordfishFriendService.Log.Info("GetFriendsSync");
			return new PlayerFriend[0];
		}

		public void GetFriendsPaged(object state, int page, int recordset, FriendRelationshipEnum orderfield, bool sortorder, SwordfishClientApi.ParameterizedCallback<PlayerFriend[]> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishFriendService.Log.Info("GetFriendsPaged");
		}

		public PlayerFriend[] GetFriendsPagedSync(int page, int recordset, FriendRelationshipEnum orderfield, bool sortorder, out int pagecount)
		{
			SkipSwordfishFriendService.Log.Info("GetFriendsPagedSync");
			pagecount = 0;
			return new PlayerFriend[0];
		}

		public void GetFriends(object state, long playerId, SwordfishClientApi.ParameterizedCallback<PlayerFriend[]> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishFriendService.Log.Info("GetFriends");
		}

		public PlayerFriend[] GetFriendsSync(long playerId)
		{
			SkipSwordfishFriendService.Log.Info("GetFriendsSync");
			return new PlayerFriend[0];
		}

		public void GetFriendsPaged(object state, long playerId, int page, int recordset, FriendRelationshipEnum orderfield, bool sortorder, SwordfishClientApi.ParameterizedCallback<PlayerFriend[]> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishFriendService.Log.Info("GetFriendsPaged");
		}

		public PlayerFriend[] GetFriendsPagedSync(long playerId, int page, int recordset, FriendRelationshipEnum orderfield, bool sortorder, out int pagecount)
		{
			SkipSwordfishFriendService.Log.Info("GetFriendsPagedSync");
			pagecount = 0;
			return new PlayerFriend[0];
		}

		public void GetFriendsWithPublisherSocialBlockedAndRestrictionDetail(object state, FriendsWithDetailFilter filter, SwordfishClientApi.ParameterizedCallback<PlayerFriendsWithBlockedAndRestrictionDetail[]> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishFriendService.Log.Info("GetFriendsWithPublisherSocialBlockedAndRestrictionDetail");
		}

		public PlayerFriendsWithBlockedAndRestrictionDetail[] GetFriendsWithPublisherSocialBlockedAndRestrictionDetailSync(FriendsWithDetailFilter filter)
		{
			SkipSwordfishFriendService.Log.Info("GetFriendsWithPublisherSocialBlockedAndRestrictionDetailSync");
			return new PlayerFriendsWithBlockedAndRestrictionDetail[0];
		}

		public void GetFriendsWithPublisherSocialBlockedAndRestrictionDetailPaged(object state, FriendsWithDetailFilter filter, int page, int recordset, PlayerFriendsWithBlockedAndRestrictionDetailEnum orderfield, bool sortorder, SwordfishClientApi.ParameterizedCallback<PlayerFriendsWithBlockedAndRestrictionDetail[]> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishFriendService.Log.Info("GetFriendsWithPublisherSocialBlockedAndRestrictionDetailPaged");
		}

		public PlayerFriendsWithBlockedAndRestrictionDetail[] GetFriendsWithPublisherSocialBlockedAndRestrictionDetailPagedSync(FriendsWithDetailFilter filter, int page, int recordset, PlayerFriendsWithBlockedAndRestrictionDetailEnum orderfield, bool sortorder, out int pagecount)
		{
			SkipSwordfishFriendService.Log.Info("GetFriendsWithPublisherSocialBlockedAndRestrictionDetailPagedSync");
			pagecount = 0;
			return new PlayerFriendsWithBlockedAndRestrictionDetail[0];
		}

		public void AddFriend(object state, long playerId, long friendPlayerId, SwordfishClientApi.Callback callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishFriendService.Log.Info("AddFriend");
		}

		public void AddFriendSync(long playerId, long friendPlayerId)
		{
			SkipSwordfishFriendService.Log.Info("AddFriendSync");
		}

		public void GetSentInvites(object state, SwordfishClientApi.ParameterizedCallback<FriendInvite[]> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishFriendService.Log.Info("GetSentInvites");
		}

		public FriendInvite[] GetSentInvitesSync()
		{
			SkipSwordfishFriendService.Log.Info("GetSentInvitesSync");
			return new FriendInvite[0];
		}

		public void GetSentInvites(object state, long playerId, SwordfishClientApi.ParameterizedCallback<FriendInvite[]> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishFriendService.Log.Info("GetSentInvites");
		}

		public FriendInvite[] GetSentInvitesSync(long playerId)
		{
			SkipSwordfishFriendService.Log.Info("GetSentInvitesSync");
			return new FriendInvite[0];
		}

		public void GetPendingInvites(object state, SwordfishClientApi.ParameterizedCallback<FriendInvite[]> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishFriendService.Log.Info("GetPendingInvites");
		}

		public FriendInvite[] GetPendingInvitesSync()
		{
			SkipSwordfishFriendService.Log.Info("GetPendingInvitesSync");
			return new FriendInvite[0];
		}

		public void GetPendingInvites(object state, long playerId, SwordfishClientApi.ParameterizedCallback<FriendInvite[]> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishFriendService.Log.Info("GetPendingInvites");
		}

		public FriendInvite[] GetPendingInvitesSync(long playerId)
		{
			SkipSwordfishFriendService.Log.Info("GetPendingInvitesSync");
			return new FriendInvite[0];
		}

		public void GetInviteByInviteId(object state, long inviteId, SwordfishClientApi.ParameterizedCallback<FriendInvite> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishFriendService.Log.Info("GetInviteByInviteId");
		}

		public FriendInvite GetInviteByInviteIdSync(long inviteId)
		{
			SkipSwordfishFriendService.Log.Info("GetInviteByInviteIdSync");
			return new FriendInvite
			{
				CreatedAt = DateTime.MinValue,
				Id = 0L,
				ToPlayerId = 0L,
				FromPlayerId = 1L,
				Bag = string.Empty
			};
		}

		public void DeclineInvite(object state, long inviteId, SwordfishClientApi.Callback callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishFriendService.Log.Info("DeclineInvite");
		}

		public void DeclineInviteSync(long inviteId)
		{
			SkipSwordfishFriendService.Log.Info("DeclineInviteSync");
		}

		public void CancelInvite(object state, long inviteId, SwordfishClientApi.Callback callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishFriendService.Log.Info("CancelInvite");
		}

		public void CancelInviteSync(long inviteId)
		{
			SkipSwordfishFriendService.Log.Info("CancelInviteSync");
		}

		event EventHandler<UserFriendEventArgs> IFriend.FriendBagUpdated
		{
			add
			{
			}
			remove
			{
			}
		}

		event EventHandler<UserFriendEventArgs> IFriend.FriendGotOnline
		{
			add
			{
			}
			remove
			{
			}
		}

		event EventHandler<UserFriendEventArgs> IFriend.FriendWentOffline
		{
			add
			{
			}
			remove
			{
			}
		}

		event EventHandler<UserFriendEventArgs> IFriend.InviteReceived
		{
			add
			{
			}
			remove
			{
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandler<UserFriendEventArgs> InviteDeclined;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandler<UserFriendEventArgs> InviteCanceled;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandler<BlockedPlayerEventArgs> PlayerBlocked;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandler<BlockedPlayerEventArgs> PlayerUnblocked;

		public void RemoveFriend(object state, long friendPlayerId, SwordfishClientApi.Callback callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			SkipSwordfishFriendService.Log.Info("RemoveFriend");
			UserFriend item = MockedSocialFriendsServices.AllFriends.FirstOrDefault((UserFriend user) => user.PlayerId == friendPlayerId);
			MockedSocialFriendsServices.AllFriends.Remove(item);
			if (this.FriendListChanged != null)
			{
				this.FriendListChanged(this, EventArgs.Empty);
			}
		}

		public void RemoveFriendSync(long friendPlayerId)
		{
			SkipSwordfishFriendService.Log.Info("RemoveFriendSync");
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandler FriendListChanged;

		private static readonly BitLogger Log = new BitLogger(typeof(SkipSwordfishFriendService));
	}
}
