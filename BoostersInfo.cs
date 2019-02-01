using System;
using System.Collections.Generic;
using Assets.ClientApiObjects;
using Pocketverse;

namespace HeavyMetalMachines
{
	public class BoostersInfo : GameHubScriptableObject
	{
		public ItemTypeScriptableObject GetBoostItemByItemType(Guid itemTypeId)
		{
			for (int i = 0; i < this.BoostItems.Count; i++)
			{
				ItemTypeScriptableObject itemTypeScriptableObject = this.BoostItems[i];
				if (itemTypeId == itemTypeScriptableObject.Id)
				{
					return itemTypeScriptableObject;
				}
			}
			return null;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(BoostersInfo));

		public List<ItemTypeScriptableObject> BoostItems;
	}
}
