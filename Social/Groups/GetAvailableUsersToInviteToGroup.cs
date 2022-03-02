using System;
using System.Linq;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Social;
using HeavyMetalMachines.Social.Friends.Business;
using HeavyMetalMachines.Social.Friends.Business.BlockedPlayers;
using HeavyMetalMachines.Social.Friends.Models;
using HeavyMetalMachines.Social.Groups.Business;
using HeavyMetalMachines.Social.Teams.Business;
using HeavyMetalMachines.Social.Teams.Models;
using Pocketverse;
using Zenject;

namespace HeavyMetalMachines.Social.Groups
{
	public class GetAvailableUsersToInviteToGroup : IGetAvailableFriendsToInviteToGroup
	{
		public GetAvailableUsersToInviteToGroup(IConfigLoader configLoader, IGetLocalPlayerTeam getLocalPlayerTeam, IFriendsStorage friendsStorage, IIsPlayerBlocked isPlayerBlocked)
		{
			this._configLoader = configLoader;
			this._getLocalPlayerTeam = getLocalPlayerTeam;
			this._friendsStorage = friendsStorage;
			this._isPlayerBlocked = isPlayerBlocked;
		}

		public Friend[] Get()
		{
			return this._friendsStorage.Friends.Where(new Func<Friend, bool>(this.IsFriendAvailableToBeInvitedToGroup)).ToArray<Friend>();
		}

		public bool Any()
		{
			return this._friendsStorage.Friends.Where(new Func<Friend, bool>(this.IsFriendAvailableToBeInvitedToGroup)).Any<Friend>();
		}

		private bool IsFriendAvailableToBeInvitedToGroup(Friend friend)
		{
			if (this._isPlayerBlocked.IsBlocked(friend.PlayerId))
			{
				return false;
			}
			if (!friend.IsOnline)
			{
				return false;
			}
			if (friend.Status != 1)
			{
				return false;
			}
			GroupManager groupManager = this._container.Resolve<GroupManager>();
			if (groupManager == null)
			{
				return false;
			}
			bool flag = groupManager.GroupMembersByID.ContainsKey(friend.UniversalId);
			return !flag;
		}

		private bool IsFriendTeamMember(Friend friend)
		{
			long playerId = friend.PlayerId;
			Team team = this._getLocalPlayerTeam.GetTeam();
			return team != null && team.Members != null && team.Members.Any((ITeamMember member) => member != null && member.PlayerId == playerId);
		}

		private readonly IConfigLoader _configLoader;

		private readonly IGetLocalPlayerTeam _getLocalPlayerTeam;

		private readonly IFriendsStorage _friendsStorage;

		private readonly IIsPlayerBlocked _isPlayerBlocked;

		[Inject]
		private readonly DiContainer _container;
	}
}
