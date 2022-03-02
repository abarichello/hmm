using System;
using HeavyMetalMachines.CompetitiveMode.Divisions;
using HeavyMetalMachines.CompetitiveMode.Prizes.Exceptions;
using HeavyMetalMachines.CompetitiveMode.Seasons;
using HeavyMetalMachines.CompetitiveMode.Seasons.Exceptions;

namespace HeavyMetalMachines.CompetitiveMode.Prizes
{
	public class GetCompetitiveDivisionsPrizes : IGetCompetitiveDivisionsPrizes
	{
		public GetCompetitiveDivisionsPrizes(ISeasonsStorage seasonsStorage, ICompetitiveRewardsProvider rewardsProvider)
		{
			this._seasonsStorage = seasonsStorage;
			this._rewardsProvider = rewardsProvider;
		}

		public CompetitiveReward[] GetForDivision(int divisionIndex)
		{
			Division[] divisions = this._seasonsStorage.CurrentSeason.Divisions;
			if (divisionIndex >= divisions.Length)
			{
				throw new DivisionNotFoundException(divisionIndex);
			}
			Division division = divisions[divisionIndex];
			return this._rewardsProvider.GetRewards(division.PrizesItemTypeIds);
		}

		public CompetitiveReward[] GetForTopPlayers()
		{
			Guid[] topPlayersRewardItemTypeIds = this.GetTopPlayersRewardItemTypeIds();
			return this._rewardsProvider.GetRewards(topPlayersRewardItemTypeIds);
		}

		private Guid[] GetTopPlayersRewardItemTypeIds()
		{
			if (this._seasonsStorage.CurrentSeason != null)
			{
				return this._seasonsStorage.CurrentSeason.TopPlayersPrizesItemTypesIds;
			}
			if (this._seasonsStorage.NextSeason != null)
			{
				return this._seasonsStorage.NextSeason.TopPlayersPrizesItemTypesIds;
			}
			throw new NoCurrentNorNextCompetitiveSeasonException();
		}

		private readonly ISeasonsStorage _seasonsStorage;

		private readonly ICompetitiveRewardsProvider _rewardsProvider;
	}
}
