using System;
using UniRx;

namespace HeavyMetalMachines.Store.Balance
{
	public class ObserveLocalBalanceChange : IObserveLocalBalanceChange
	{
		public ObserveLocalBalanceChange(ILocalBalanceStorage localBalanceStorage)
		{
			this._localBalanceStorage = localBalanceStorage;
		}

		public IObservable<int> ObserveSoftCurrencyChanged
		{
			get
			{
				return this._localBalanceStorage.OnSoftCurrencyChange;
			}
		}

		public IObservable<long> ObserveHardCurrencyChanged
		{
			get
			{
				return this._localBalanceStorage.OnHardCurrencyChange;
			}
		}

		public IObservable<int> GetAndObserveSoftCurrencyChanged
		{
			get
			{
				return Observable.Concat<int>(Observable.Return(this._localBalanceStorage.SoftCurrency), new IObservable<int>[]
				{
					this._localBalanceStorage.OnSoftCurrencyChange
				});
			}
		}

		public IObservable<long> GetAndObserveHardCurrencyChanged
		{
			get
			{
				return Observable.Concat<long>(Observable.Return<long>(this._localBalanceStorage.HardCurrency), new IObservable<long>[]
				{
					this._localBalanceStorage.OnHardCurrencyChange
				});
			}
		}

		private readonly ILocalBalanceStorage _localBalanceStorage;
	}
}
