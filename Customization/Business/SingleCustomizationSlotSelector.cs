using System;
using Assets.ClientApiObjects;
using HeavyMetalMachines.DataTransferObjects.Battlepass;

namespace HeavyMetalMachines.Customization.Business
{
	public class SingleCustomizationSlotSelector : IGetCustomizationSlot
	{
		public SingleCustomizationSlotSelector(ItemCategoryScriptableObject itemCategory)
		{
			this._itemCategory = itemCategory;
		}

		public PlayerCustomizationSlot GetEquippingSlot()
		{
			return this._itemCategory.CustomizationSlots[0];
		}

		public PlayerCustomizationSlot GetUnequippingSlot(Guid itemTypeId)
		{
			return this._itemCategory.CustomizationSlots[0];
		}

		private ItemCategoryScriptableObject _itemCategory;
	}
}
