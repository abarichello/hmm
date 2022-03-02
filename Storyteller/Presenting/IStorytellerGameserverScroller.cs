using System;
using HeavyMetalMachines.Presenting;

namespace HeavyMetalMachines.Storyteller.Presenting
{
	public interface IStorytellerGameserverScroller : IScroller<IStorytellerMatchInfo>
	{
		void UpdateIsLocalPlayerInQueueState(bool state);

		void UpdateIsLocalPlayerInGroupState(bool state);
	}
}
