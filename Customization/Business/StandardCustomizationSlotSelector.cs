using System;
using HeavyMetalMachines.DataTransferObjects.Battlepass;

namespace HeavyMetalMachines.Customization.Business
{
	public class StandardCustomizationSlotSelector : IGetCustomizationSlot
	{
		public PlayerCustomizationSlot GetEquippingSlot()
		{
			return 0;
		}

		public PlayerCustomizationSlot GetUnequippingSlot(Guid itemTypeId)
		{
			return 0;
		}
	}
}
