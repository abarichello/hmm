using System;
using UniRx;

namespace HeavyMetalMachines.Store.Balance
{
	public class LocalBalanceStorage : ILocalBalanceStorage
	{
		public LocalBalanceStorage()
		{
			this._softCurrency = new ReactiveProperty<int>();
			this._hardCurrency = new ReactiveProperty<long>();
		}

		public int SoftCurrency
		{
			get
			{
				return this._softCurrency.Value;
			}
			set
			{
				this._softCurrency.Value = value;
			}
		}

		public long HardCurrency
		{
			get
			{
				return this._hardCurrency.Value;
			}
			set
			{
				this._hardCurrency.Value = value;
			}
		}

		public IObservable<int> OnSoftCurrencyChange
		{
			get
			{
				return this._softCurrency;
			}
		}

		public IObservable<long> OnHardCurrencyChange
		{
			get
			{
				return this._hardCurrency;
			}
		}

		private readonly ReactiveProperty<int> _softCurrency;

		private readonly ReactiveProperty<long> _hardCurrency;
	}
}
