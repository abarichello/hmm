using System;
using Assets.ClientApiObjects;
using HeavyMetalMachines.Presenting;
using UniRx;

namespace HeavyMetalMachines.Store.Details.View
{
	public interface IStoreItemDetailView
	{
		IButton BackButton { get; }

		IObservable<Unit> ShowDriver(IItemType driverItemType);

		IObservable<Unit> ShowSkin(IItemType driverItemType);

		IObservable<Unit> ShowGeneric(IItemType itemType);

		IObservable<Unit> Hide();
	}
}
