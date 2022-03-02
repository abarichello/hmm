using System;
using HeavyMetalMachines.DataTransferObjects.Battlepass;

namespace HeavyMetalMachines.Customization
{
	public class ItemChangeRequestState
	{
		public bool IsEquip;

		public Guid CategoryId;

		public Guid ItemTypeId;

		public PlayerCustomizationSlot Slot;
	}
}
