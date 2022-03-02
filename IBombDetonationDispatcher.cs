using System;
using HeavyMetalMachines.Match;

namespace HeavyMetalMachines
{
	public interface IBombDetonationDispatcher
	{
		void Send(TeamKind damagedTeam, int pickupId, int lastFrameId);
	}
}
