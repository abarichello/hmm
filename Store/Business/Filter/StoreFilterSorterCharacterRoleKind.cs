using System;
using System.Collections.Generic;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using HeavyMetalMachines.Characters;

namespace HeavyMetalMachines.Store.Business.Filter
{
	public class StoreFilterSorterCharacterRoleKind : IStoreFilterSorter
	{
		public StoreFilterSorterCharacterRoleKind(DriverRoleKind roleKind, IStoreFilterSorter tiebreakerSorter)
		{
			this._roleKind = roleKind;
			this._tiebreakerSorter = tiebreakerSorter;
		}

		public void Sort(List<IItemType> itemTypes)
		{
			List<IItemType> list = new List<IItemType>(itemTypes.Count);
			foreach (IItemType itemType in itemTypes)
			{
				CharacterItemTypeComponent component = itemType.GetComponent<CharacterItemTypeComponent>();
				if (component == null || component.Role != this._roleKind)
				{
					list.Add(itemType);
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
	}
}
