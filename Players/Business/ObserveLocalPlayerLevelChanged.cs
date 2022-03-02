using System;
using UniRx;

namespace HeavyMetalMachines.Players.Business
{
	public class ObserveLocalPlayerLevelChanged : IObserveLocalPlayerLevelChanged
	{
		public ObserveLocalPlayerLevelChanged(ILocalPlayerTotalLevelStorage localPlayerTotalLevelStorage)
		{
			this._localPlayerTotalLevelStorage = localPlayerTotalLevelStorage;
		}

		public IObservable<int> Observe
		{
			get
			{
				return this._localPlayerTotalLevelStorage.OnLevelUpdated;
			}
		}

		public IObservable<int> GetAndObserve
		{
			get
			{
				return Observable.Concat<int>(Observable.Return(this._localPlayerTotalLevelStorage.Get()), new IObservable<int>[]
				{
					this._localPlayerTotalLevelStorage.OnLevelUpdated
				});
			}
		}

		private readonly ILocalPlayerTotalLevelStorage _localPlayerTotalLevelStorage;
	}
}
