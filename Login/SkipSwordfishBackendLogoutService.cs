using System;
using UniRx;

namespace HeavyMetalMachines.Login
{
	public class SkipSwordfishBackendLogoutService : IBackendLogoutService
	{
		public IObservable<Unit> Logout()
		{
			return Observable.ReturnUnit();
		}
	}
}
