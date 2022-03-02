using System;
using HeavyMetalMachines.CompetitiveMode.Seasons;

namespace HeavyMetalMachines.CompetitiveMode.View.Matches
{
	public class FakeGetCurrentOrNextCompetitiveSeason : IGetCurrentOrNextCompetitiveSeason
	{
		public CompetitiveSeason Get()
		{
			return new CompetitiveSeason
			{
				Id = 17L
			};
		}
	}
}
