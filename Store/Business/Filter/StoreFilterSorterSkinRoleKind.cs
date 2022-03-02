using System;
using System.Collections.Generic;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using Assets.ClientApiObjects.Specializations;
using HeavyMetalMachines.Characters;
using HeavyMetalMachines.Inventory.Business;

namespace HeavyMetalMachines.Store.Business.Filter
{
	public class StoreFilterSorterSkinRoleKind : IStoreFilterSorter
	{
		public StoreFilterSorterSkinRoleKind(DriverRoleKind roleKind, IStoreFilterSorter tiebreakerSorter, IGetCharacterItemTypeFromSkinItemTypeId getCharacterItemTypeFromSkinItemTypeId)
		{
			this._roleKind = roleKind;
			this._tiebreakerSorter = tiebreakerSorter;
			this._getCharacterItemTypeFromSkinItemTypeId = getCharacterItemTypeFromSkinItemTypeId;
		}

		public void Sort(List<IItemType> itemTypes)
		{
			List<IItemType> list = new List<IItemType>(itemTypes.Count);
			foreach (IItemType itemType in itemTypes)
			{
				ShopItemTypeComponent component = itemType.GetComponent<ShopItemTypeComponent>();
				if (component == null || component.PreviewKind != ItemPreviewKind.Model3D)
				{
					list.Add(itemType);
				}
				else
				{
					IItemType fromSkinId = this._getCharacterItemTypeFromSkinItemTypeId.GetFromSkinId(itemType.Id);
					ItemTypeComponent itemTypeComponent;
					if (!fromSkinId.GetComponentByEnum(ItemTypeComponent.Type.Character, out itemTypeComponent))
					{
						list.Add(itemType);
					}
					else
					{
						CharacterItemTypeComponent characterItemTypeComponent = itemTypeComponent as CharacterItemTypeComponent;
						if (characterItemTypeComponent == null || characterItemTypeComponent.Role != this._roleKind)
						{
							list.Add(itemType);
						}
					}
				}
			}
			foreach (IItemType item in list)
			{
				itemTypes.Remove(item);
			}
			itemTypes.Sort(new Comparison<IItemType>(this.SortElements));
		}

		public int SortElements(IItemType x, IItemType y)
		{
			return this._tiebreakerSorter.SortElements(x, y);
		}

		private readonly DriverRoleKind _roleKind;

		private readonly IStoreFilterSorter _tiebreakerSorter;

		private readonly IGetCharacterItemTypeFromSkinItemTypeId _getCharacterItemTypeFromSkinItemTypeId;
	}
}
