using System;
using HeavyMetalMachines.Presenting;

namespace HeavyMetalMachines.Inventory.View
{
	public interface IInventoryCharacterSelectionView
	{
		IActivatable CharactersSelectionGroupActivatable { get; }

		IDropdown<int> CharactersDropdown { get; }

		IButton LeftButton { get; }

		IButton RightButton { get; }
	}
}
