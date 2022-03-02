using System;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.Players
{
	public interface IPlayerCompetitiveStateProvider
	{
		IObservable<PlayerCompetitiveState> GetPlayerCompetitiveState(long playerId, long seasonId);

		IObservable<PlayerCompetitiveState[]> GetPlayersCompetitiveState(long[] playersIds, long seasonId);
	}
}
