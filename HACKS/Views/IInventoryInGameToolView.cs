using System;
using HeavyMetalMachines.Presenting;

namespace HeavyMetalMachines.HACKS.Views
{
	public interface IInventoryInGameToolView
	{
		ICanvas Canvas { get; }

		IButton EquipButton { get; }

		ILabel CategoryLabel { get; }

		ILabel ItemLabel { get; }

		ToggleInventoryToolView InstantiateToggleArmoryToolView();

		ItemInventoryToolView InstantiateItemArmoryToolView();

		void Rebuild();

		void ResetItemToggles();

		void ResetCategoryToggles();
	}
}
