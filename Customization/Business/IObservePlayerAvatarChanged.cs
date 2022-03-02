using System;
using UniRx;

namespace HeavyMetalMachines.Customization.Business
{
	public interface IObservePlayerAvatarChanged
	{
		IObservable<Unit> Observe();
	}
}
