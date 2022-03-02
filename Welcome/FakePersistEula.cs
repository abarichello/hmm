using System;
using HeavyMetalMachines.Login;
using UniRx;

namespace HeavyMetalMachines.Welcome
{
	public class FakePersistEula : IPersistCurrentEulaVersion
	{
		public IObservable<Unit> Persist()
		{
			return Observable.AsUnitObservable<long>(Observable.Timer(TimeSpan.FromSeconds(3.0)));
		}
	}
}
