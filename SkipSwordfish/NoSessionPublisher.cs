using System;
using Assets.Standard_Assets.Scripts.HMM.Swordfish.Services;
using ClientAPI;
using HeavyMetalMachines.MatchMaking;
using HeavyMetalMachines.Social.Groups.Business;

namespace HeavyMetalMachines.SkipSwordfish
{
	public class NoSessionPublisher : IGroupXBoxSessionPublisher
	{
		public void Setup(SwordfishClientApi clientApi, IGroupService groupService, IGetThenObserveMatchmakingQueueState matchmakingQueueState)
		{
		}

		public void Dispose()
		{
		}

		public Guid CurrentSoloSessionId { get; private set; }
	}
}
