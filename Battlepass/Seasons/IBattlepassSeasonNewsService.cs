using System;
using UniRx;

namespace HeavyMetalMachines.Battlepass.Seasons
{
	public interface IBattlepassSeasonNewsService
	{
		IObservable<bool> TryConsume(long seasonId);
	}
}
