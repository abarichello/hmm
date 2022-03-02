using System;
using UniRx;

namespace HeavyMetalMachines.Frontend
{
	public interface IShowSystemCheckDialogs
	{
		IObservable<Unit> Show();
	}
}
