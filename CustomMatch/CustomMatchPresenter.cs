using System;
using HeavyMetalMachines.Presenting;
using UniRx;

namespace HeavyMetalMachines.CustomMatch
{
	public class CustomMatchPresenter : ICustomMatchPresenter, IPresenter
	{
		public IObservable<Unit> Initialize()
		{
			return Observable.ReturnUnit();
		}

		public IObservable<Unit> Show()
		{
			return Observable.ReturnUnit();
		}

		public IObservable<Unit> Hide()
		{
			return Observable.ReturnUnit();
		}

		public IObservable<Unit> Dispose()
		{
			return Observable.ReturnUnit();
		}

		public IObservable<Unit> ObserveHide()
		{
			throw new NotImplementedException();
		}
	}
}
