using System;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.Players
{
	public interface IGetOrFetchPlayerState
	{
		IObservable<PlayerCompetitiveState> GetFromPlayerId(long playerId);

		IObservable<PlayerCompetitiveState[]> GetFromPlayersIds(long[] playerId);
	}
}
