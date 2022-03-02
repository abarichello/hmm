using System;
using Assets.ClientApiObjects;
using HeavyMetalMachines.DataTransferObjects.Battlepass;
using HeavyMetalMachines.Items.DataTransferObjects;

namespace HeavyMetalMachines.Customization.Business
{
	public class MultipleCustomizationSlotSelector : IGetCustomizationSlot
	{
		public MultipleCustomizationSlotSelector(ItemCategoryScriptableObject itemCategory, CustomizationContent content)
		{
			this._itemCategory = itemCategory;
			this._content = content;
		}

		public PlayerCustomizationSlot GetEquippingSlot()
		{
			for (int i = 0; i < this._itemCategory.CustomizationSlots.Count; i++)
			{
				PlayerCustomizationSlot playerCustomizationSlot = this._itemCategory.CustomizationSlots[i];
				if (this._content.GetGuidBySlot(playerCustomizationSlot) == Guid.Empty)
				{
					return playerCustomizationSlot;
				}
			}
			return 0;
		}

		public PlayerCustomizationSlot GetUnequippingSlot(Guid itemTypeId)
		{
			for (int i = this._itemCategory.CustomizationSlots.Count - 1; i >= 0; i--)
			{
				PlayerCustomizationSlot playerCustomizationSlot = this._itemCategory.CustomizationSlots[i];
				if (this._content.GetGuidBySlot(playerCustomizationSlot) == itemTypeId)
				{
					return playerCustomizationSlot;
				}
			}
			return 0;
		}

		private ItemCategoryScriptableObject _itemCategory;

		private CustomizationContent _content;
	}
}
