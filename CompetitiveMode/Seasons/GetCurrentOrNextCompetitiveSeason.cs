using System;
using HeavyMetalMachines.CompetitiveMode.Seasons.Exceptions;

namespace HeavyMetalMachines.CompetitiveMode.Seasons
{
	public class GetCurrentOrNextCompetitiveSeason : IGetCurrentOrNextCompetitiveSeason
	{
		public GetCurrentOrNextCompetitiveSeason(ISeasonsStorage seasonsStorage)
		{
			this._seasonsStorage = seasonsStorage;
		}

		public CompetitiveSeason Get()
		{
			if (this._seasonsStorage.CurrentSeason != null)
			{
				return this._seasonsStorage.CurrentSeason;
			}
			if (this._seasonsStorage.NextSeason != null)
			{
				return this._seasonsStorage.NextSeason;
			}
			throw new NoCurrentNorNextCompetitiveSeasonException();
		}

		private readonly ISeasonsStorage _seasonsStorage;
	}
}
