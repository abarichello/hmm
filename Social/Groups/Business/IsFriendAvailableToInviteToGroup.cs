using System;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Social;
using HeavyMetalMachines.Social.Friends.Models;
using Zenject;

namespace HeavyMetalMachines.Social.Groups.Business
{
	public class IsFriendAvailableToInviteToGroup : IIsFriendAvailableToInviteToGroup
	{
		public bool IsAvailable(Friend friend)
		{
			if (!friend.IsOnline)
			{
				return false;
			}
			if (friend.Status != 1)
			{
				return false;
			}
			IGroupManager groupManager = this._container.Resolve<IGroupManager>();
			if (groupManager == null)
			{
				return false;
			}
			bool flag = groupManager.GroupMembersByID.ContainsKey(friend.UniversalId);
			return !flag;
		}

		[Inject]
		private readonly DiContainer _container;
	}
}
