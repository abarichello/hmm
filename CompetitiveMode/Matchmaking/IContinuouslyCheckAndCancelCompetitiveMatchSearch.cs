using System;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.Matchmaking
{
	public interface IContinuouslyCheckAndCancelCompetitiveMatchSearch
	{
		IObservable<Unit> CheckAndCancel();
	}
}
