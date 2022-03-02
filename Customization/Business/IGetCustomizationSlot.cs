using System;
using HeavyMetalMachines.DataTransferObjects.Battlepass;

namespace HeavyMetalMachines.Customization.Business
{
	public interface IGetCustomizationSlot
	{
		PlayerCustomizationSlot GetEquippingSlot();

		PlayerCustomizationSlot GetUnequippingSlot(Guid itemTypeId);
	}
}
