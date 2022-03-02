using System;
using UniRx;

namespace HeavyMetalMachines.MatchMakingQueue.Infra
{
	public interface INoviceTrialsAmountProvider
	{
		int GetNoviceTrialsAmount();

		IObservable<Unit> InitializeProvider();
	}
}
