using System;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Social;

namespace HeavyMetalMachines.Social.Groups.Business
{
	public class IsUserPendingInviteToGroup : IIsUserPendingInviteToGroup
	{
		public IsUserPendingInviteToGroup(IGroupManager groupManager)
		{
			this._groupManager = groupManager;
		}

		public bool Get(string universalId)
		{
			return this._groupManager.PendingInvites.ContainsKey(universalId);
		}

		private readonly IGroupManager _groupManager;
	}
}
