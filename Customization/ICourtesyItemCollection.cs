using System;
using Assets.ClientApiObjects;
using HeavyMetalMachines.DataTransferObjects.Battlepass;

namespace HeavyMetalMachines.Customization
{
	public interface ICourtesyItemCollection
	{
		IItemType GetDefaultItem(PlayerCustomizationSlot slot);

		IItemType[] GetItems(PlayerCustomizationSlot slot);
	}
}
