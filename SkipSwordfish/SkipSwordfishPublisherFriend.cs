using System;
using Assets.Standard_Assets.Scripts.HMM.SkipSwordfish;
using ClientAPI.Objects;
using ClientAPI.Publisher3rdp.Contracts;
using ClientAPI.Service.Interfaces;

namespace HeavyMetalMachines.SkipSwordfish
{
	public class SkipSwordfishPublisherFriend : IPublisherFriend
	{
		public long[] GetAllFriendsPlayerId(IUser user, DateTime? lastLoginDateFilter = null)
		{
			throw new NotImplementedException();
		}

		public void GetAllFriendsPlayerIdAsync(Action<long[]> callback, Action<Exception> errorCallback, IUser user, DateTime? lastLoginDateFilter = null)
		{
			throw new NotImplementedException();
		}

		public string[] GetAllFriendsUniversalId()
		{
			throw new NotImplementedException();
		}

		public void GetAllFriendsUniversalIdAsync(Action<string[]> callback, Action<Exception> errorCallback)
		{
			throw new NotImplementedException();
		}

		public PlayerFriendSuggestion[] GetRecommendedFriendList()
		{
			throw new NotImplementedException();
		}

		public void GetRecommendedFriendListAsync(Action<PlayerFriendSuggestion[]> callback, Action<Exception> errorCallback)
		{
			callback(MockedSocialFriendsServices.PlayerFriendSuggestionSF.ToArray());
		}

		public void ShowProfileCardForUser(string toUniversalId)
		{
			throw new NotImplementedException();
		}

		public string[] GetPublisherBlockedUniversalId(string currentUserUniversalId)
		{
			throw new NotImplementedException();
		}
	}
}
