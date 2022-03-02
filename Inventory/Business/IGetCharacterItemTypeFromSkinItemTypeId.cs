using System;
using Assets.ClientApiObjects;

namespace HeavyMetalMachines.Inventory.Business
{
	public interface IGetCharacterItemTypeFromSkinItemTypeId
	{
		IItemType GetFromSkinId(Guid skinItemTypeId);
	}
}
