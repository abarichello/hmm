using System;
using Pocketverse;

namespace HeavyMetalMachines.Store.Business.PlayerInventory
{
	public class GetLocalUserHasItemWrapper : GameHubObject, IGetLocalUserHasItem
	{
		public bool HasItemOfType(Guid itemTypeId)
		{
			return GameHubObject.Hub.User.Inventory.HasItemOfType(itemTypeId);
		}
	}
}
