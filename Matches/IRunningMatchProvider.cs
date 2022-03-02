using System;
using UniRx;

namespace HeavyMetalMachines.Matches
{
	public interface IRunningMatchProvider
	{
		IObservable<GetRunningMatchResult> GetRunningMatch(string playerId);
	}
}
