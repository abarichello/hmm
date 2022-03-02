using System;
using UniRx;

namespace HeavyMetalMachines.Battlepass.Seasons
{
	public interface IConsumeCurrentBattlepassSeasonNews
	{
		IObservable<bool> TryConsume();
	}
}
