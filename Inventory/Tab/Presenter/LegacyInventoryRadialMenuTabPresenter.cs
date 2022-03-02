using System;
using HeavyMetalMachines.Inventory.Tab.View;
using HeavyMetalMachines.Presenting;
using UniRx;
using Zenject;

namespace HeavyMetalMachines.Inventory.Tab.Presenter
{
	public class LegacyInventoryRadialMenuTabPresenter<T> : IEmotesInventoryTabPresenter, IPresenter where T : class, IInventoryTabView
	{
		public IObservable<Unit> Initialize()
		{
			return Observable.ReturnUnit();
		}

		private void GetView()
		{
			this._view = this._viewProvider.Provide<T>(null);
		}

		public IObservable<Unit> Show()
		{
			return Observable.Defer<Unit>(delegate()
			{
				this.GetView();
				return this._view.Show();
			});
		}

		public IObservable<Unit> Hide()
		{
			return this._view.Hide();
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
		private readonly IViewProvider _viewProvider;

		private T _view;
	}
}
