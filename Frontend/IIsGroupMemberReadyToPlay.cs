using System;

namespace HeavyMetalMachines.Frontend
{
	public interface IIsGroupMemberReadyToPlay
	{
		bool IsReady(long playerId);
	}
}
