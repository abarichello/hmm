using System;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.Matchmaking
{
	public interface IWaitAndGetMyPlayerCompetitiveStateProgress
	{
		IObservable<PlayerCompetitiveProgress> WaitAndGet();
	}
}
