using System;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.Players
{
	public interface IFetchPlayerCompetitiveState
	{
		IObservable<PlayerCompetitiveState> FetchMine();

		IObservable<PlayerCompetitiveState> Fetch(long playerId);
	}
}
