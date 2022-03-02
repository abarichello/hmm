using System;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.Matchmaking
{
	public interface IInitializeAndWatchMyPlayerCompetitiveStateProgress
	{
		IObservable<Unit> InitializeAndWatch();
	}
}
