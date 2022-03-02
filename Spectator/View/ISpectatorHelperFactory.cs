using System;
using UniRx;

namespace HeavyMetalMachines.Spectator.View
{
	public interface ISpectatorHelperFactory
	{
		IObservable<Unit> LoadSpectatorHelper();
	}
}
