using System;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Social;
using ClientAPI.Objects;
using Hoplon.Assertions;

namespace HeavyMetalMachines.Social.Groups.Business
{
	public class CancelGroupInvite : ICancelGroupInvite
	{
		public CancelGroupInvite(IGroupManager groupManager)
		{
			Assert.ConstructorParametersAreNotNull(new object[]
			{
				groupManager
			});
			this._groupManager = groupManager;
		}

		public void Cancel(IPlayer player)
		{
			GroupMember groupMember = CancelGroupInvite.CreateGroupMemberFromPlayer(player);
			this._groupManager.TryKickMemberOrCancelInvite(groupMember);
		}

		private static GroupMember CreateGroupMemberFromPlayer(IPlayer player)
		{
			return new GroupMember
			{
				PlayerId = player.PlayerId,
				UniversalID = player.UniversalId,
				PlayerName = player.Nickname
			};
		}

		private readonly IGroupManager _groupManager;
	}
}
