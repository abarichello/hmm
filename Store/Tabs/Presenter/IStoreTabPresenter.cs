using System;
using Assets.ClientApiObjects;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Store.Business.Filter;
using Hoplon.Input.UiNavigation.AxisSelector;
using UniRx;

namespace HeavyMetalMachines.Store.Tabs.Presenter
{
	public interface IStoreTabPresenter : IPresenter, IStoreFilterChangeReceiver
	{
		void ShowPage(int page);

		IObservable<IItemType> OnClick { get; }

		void GoToNextPage();

		void GoToPreviousPage();

		void GoToNextPageByEdgeReached();

		void GoToPreviousPageByEdgeReached();

		int GetPageCount();

		int CurrentPage { get; }

		IObservable<AxisSelectorEdge> ObserveOnAxisSelectorEdgeReached();
	}
}
