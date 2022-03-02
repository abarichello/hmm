using System;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.Prizes
{
	public interface ITryCollectMyPlayerPendingCompetitivePrizes
	{
		IObservable<CompetitivePrizesCollection> TryCollect();
	}
}
