using System;
using UniRx;

namespace HeavyMetalMachines.Store.Business.RefreshStoreItemStorage
{
	public interface IRefreshStoreItemStorage
	{
		IObservable<Unit> RefreshAllItems();
	}
}
