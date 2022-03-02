using System;
using UniRx;

namespace HeavyMetalMachines.Welcome
{
	public interface IExecuteWelcomeState
	{
		IObservable<Unit> Initialize();

		IObservable<Unit> Execute();
	}
}
