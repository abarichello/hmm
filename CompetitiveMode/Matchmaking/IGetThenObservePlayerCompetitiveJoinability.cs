using System;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.Matchmaking
{
	public interface IGetThenObservePlayerCompetitiveJoinability
	{
		IObservable<CompetitiveQueueJoinabilityResult> GetThenObserve();
	}
}
