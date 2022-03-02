using System;
using HeavyMetalMachines.Items.DataTransferObjects.Match;

namespace HeavyMetalMachines.Match.Infra
{
	public interface IMatchHistoryProvider
	{
		MatchHistoryInventoryBag GetInventoryBag();
	}
}
