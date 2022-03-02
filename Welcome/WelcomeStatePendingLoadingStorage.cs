using System;
using Hoplon;
using UniRx;

namespace HeavyMetalMachines.Welcome
{
	public class WelcomeStatePendingLoadingStorage
	{
		public Maybe<IObservable<Unit>> PendingLoading { get; set; }
	}
}
