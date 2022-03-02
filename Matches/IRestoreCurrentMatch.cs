using System;
using UniRx;

namespace HeavyMetalMachines.Matches
{
	public interface IRestoreCurrentMatch
	{
		IObservable<bool> TryRestore();
	}
}
