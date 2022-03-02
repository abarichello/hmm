using System;
using HeavyMetalMachines.Matches.DataTransferObjects;

namespace HeavyMetalMachines.MatchMakingQueue.Infra
{
	public class MatchmakingStartMatchSearchResult
	{
		public MatchKind MatchKind;

		public long EstimatedWaitTimeMinutes;
	}
}
