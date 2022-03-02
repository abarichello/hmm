using System;
using UniRx;

namespace HeavyMetalMachines.Frontend
{
	public interface IExecuteStarterState
	{
		IObservable<Unit> Execute();
	}
}
