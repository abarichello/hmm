using System;
using HeavyMetalMachines.Presenting;
using UniRx;

namespace HeavyMetalMachines.Inventory.Tab.View
{
	public interface IInventoryTabView
	{
		string NavigationName { get; }

		IToggle ToggleButton { get; }

		IObservable<Unit> Show();

		IObservable<Unit> Hide();
	}
}
