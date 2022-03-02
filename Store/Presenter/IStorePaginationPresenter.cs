using System;
using HeavyMetalMachines.Store.Tabs.Presenter;

namespace HeavyMetalMachines.Store.Presenter
{
	public interface IStorePaginationPresenter
	{
		void InitializeTabPagination(IStoreTabPresenter tabPresenter);

		void DisablePagination();
	}
}
