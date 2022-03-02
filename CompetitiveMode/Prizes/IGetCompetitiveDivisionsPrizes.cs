using System;

namespace HeavyMetalMachines.CompetitiveMode.Prizes
{
	public interface IGetCompetitiveDivisionsPrizes
	{
		CompetitiveReward[] GetForDivision(int divisionIndex);

		CompetitiveReward[] GetForTopPlayers();
	}
}
