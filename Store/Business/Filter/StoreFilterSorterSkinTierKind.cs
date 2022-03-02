using System;
using System.Collections.Generic;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using Assets.ClientApiObjects.Specializations;
using HeavyMetalMachines.Customizations.Skins;

namespace HeavyMetalMachines.Store.Business.Filter
{
	public class StoreFilterSorterSkinTierKind : IStoreFilterSorter
	{
		public StoreFilterSorterSkinTierKind(TierKind tierKind, IStoreFilterSorter tiebreakerSorter)
		{
			this._tierKind = tierKind;
			this._tiebreakerSorter = tiebreakerSorter;
		}

		public void Sort(List<IItemType> itemTypes)
		{
			List<IItemType> list = new List<IItemType>(itemTypes.Count);
			foreach (IItemType itemType in itemTypes)
			{
				ShopItemTypeComponent component = itemType.GetComponent<ShopItemTypeComponent>();
				ItemTypeComponent itemTypeComponent;
				if (component == null || component.PreviewKind != ItemPreviewKind.Model3D)
				{
					list.Add(itemType);
				}
				else if (!itemType.GetComponentByEnum(ItemTypeComponent.Type.SkinPrefab, out itemTypeComponent))
				{
					list.Add(itemType);
				}
				else
				{
					SkinPrefabItemTypeComponent skinPrefabItemTypeComponent = itemTypeComponent as SkinPrefabItemTypeComponent;
					if (skinPrefabItemTypeComponent == null || skinPrefabItemTypeComponent.Tier != this._tierKind)
					{
						list.Add(itemType);
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

		private readonly TierKind _tierKind;

		private readonly IStoreFilterSorter _tiebreakerSorter;
	}
}
