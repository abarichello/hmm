using System;
using HeavyMetalMachines.Matches;

namespace HeavyMetalMachines.CompetitiveMode.Matchmaking
{
	public class ShouldTrackPlayerCompetitiveStateProgress : IShouldTrackPlayerCompetitiveStateProgress
	{
		public ShouldTrackPlayerCompetitiveStateProgress(IGetCurrentMatch getCurrentMatch)
		{
			this._getCurrentMatch = getCurrentMatch;
		}

		public bool Check()
		{
			Match? ifExisting = this._getCurrentMatch.GetIfExisting();
			return ifExisting != null && ShouldTrackPlayerCompetitiveStateProgress.IsSupportedMatchMode(ifExisting.Value);
		}

		private static bool IsSupportedMatchMode(Match currentMatch)
		{
			return currentMatch.Mode == 1 || currentMatch.Mode == 2 || currentMatch.Mode == null || currentMatch.Mode == 6;
		}

		private readonly IGetCurrentMatch _getCurrentMatch;
	}
}
