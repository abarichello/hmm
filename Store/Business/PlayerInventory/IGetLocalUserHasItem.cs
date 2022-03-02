using System;

namespace HeavyMetalMachines.Store.Business.PlayerInventory
{
	public interface IGetLocalUserHasItem
	{
		bool HasItemOfType(Guid itemTypeId);
	}
}
