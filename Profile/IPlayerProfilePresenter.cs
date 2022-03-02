using System;
using UniRx;

namespace HeavyMetalMachines.Profile
{
	public interface IPlayerProfilePresenter
	{
		IObservable<Unit> Initialize();
	}
}
