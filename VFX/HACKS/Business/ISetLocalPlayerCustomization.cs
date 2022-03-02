using System;
using HeavyMetalMachines.DataTransferObjects.Battlepass;

namespace HeavyMetalMachines.VFX.HACKS.Business
{
	public interface ISetLocalPlayerCustomization
	{
		void Set(PlayerCustomizationSlot slot, Guid itemTypeId);
	}
}
