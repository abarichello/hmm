using System;
using Assets.Standard_Assets.Scripts.HMM.Swordfish.Services;
using ClientAPI;
using HeavyMetalMachines.MatchMaking;

namespace HeavyMetalMachines.Social.Groups.Business
{
	public interface IGroupXBoxSessionPublisher
	{
		void Setup(SwordfishClientApi clientApi, IGroupService groupService, IGetThenObserveMatchmakingQueueState matchmakingQueueState);

		void Dispose();

		Guid CurrentSoloSessionId { get; }
	}
}
