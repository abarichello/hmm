using System;
using Assets.ClientApiObjects;

namespace HeavyMetalMachines.Inventory.Business
{
	public interface IGetItemFromCollection
	{
		IItemType Get(Guid itemId);
	}
}
