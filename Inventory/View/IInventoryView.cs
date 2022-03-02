using System;
using HeavyMetalMachines.Presenting;
using Hoplon.Input.UiNavigation;
using UniRx;

namespace HeavyMetalMachines.Inventory.View
{
	public interface IInventoryView
	{
		IButton BackButton { get; }

		IUiNavigationGroupHolder UiNavigationGroupHolder { get; }

		IObservable<Unit> PlayInAnimation();

		IObservable<Unit> PlayOutAnimation();
	}
}
