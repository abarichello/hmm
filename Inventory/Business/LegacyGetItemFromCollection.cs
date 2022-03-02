using System;
using Assets.ClientApiObjects;
using Pocketverse;

namespace HeavyMetalMachines.Inventory.Business
{
	public class LegacyGetItemFromCollection : GameHubObject, IGetItemFromCollection
	{
		public IItemType Get(Guid itemId)
		{
			return GameHubObject.Hub.InventoryColletion.AllItemTypes[itemId];
		}
	}
}
