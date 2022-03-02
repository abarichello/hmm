using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Store.Tabs.View;
using UniRx;
using Zenject;

namespace HeavyMetalMachines.Store.Tabs.Presenter
{
	public class LegacyNguiStoreTabPresenterWrapper<T> : ICashStoreTabPresenter, IDriversStoreTabPresenter, IBoostersStoreTabPresenter, IEffectsStoreTabPresenter, IEmotesStoreTabPresenter, ISkinsStoreTabPresenter, ISpraysStoreTabPresenter, IPresenter where T : class, IStoreTabView
	{
		public IObservable<Unit> Initialize()
		{
			return Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				this.InitializeView();
			});
		}

		private void InitializeView()
		{
			this._view = this._viewProvider.Provide<T>(null);
		}

		public IObservable<Unit> Show()
		{
			return this._view.AnimateShow();
		}

		public IObservable<Unit> Hide()
		{
			return this._view.AnimateHide();
		}

		public IObservable<Unit> Dispose()
		{
			return Observable.ReturnUnit();
		}

		public IObservable<Unit> ObserveHide()
		{
			throw new NotImplementedException();
		}

		[Inject]
		private IViewProvider _viewProvider;

		private T _view;
	}
}
