using System;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Social;
using HeavyMetalMachines.Core.Extensions;
using Hoplon.Assertions;

namespace HeavyMetalMachines.Social.Groups.Business
{
	public class GetGroupInviteExpiration : IGetGroupInviteExpiration
	{
		public GetGroupInviteExpiration(IGroupManager groupManager)
		{
			Assert.ConstructorParametersAreNotNull(new object[]
			{
				groupManager
			});
			this._groupManager = groupManager;
		}

		public TimeSpan Get(IPlayer player)
		{
			return this._groupManager.GetPendingInviteRemainingTime(player.UniversalId).ToTimeSpan();
		}

		private readonly IGroupManager _groupManager;
	}
}
