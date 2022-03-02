using System;
using HeavyMetalMachines.Store.Business.Filter;

namespace HeavyMetalMachines.Store.Presenter
{
	public interface IStoreFilterPresenter
	{
		void DisableFilter();

		void InitializeFilter(IStoreFilterContract storeFilterContract, IStoreFilterChangeReceiver storeFilterChangeReceiver);
	}
}
