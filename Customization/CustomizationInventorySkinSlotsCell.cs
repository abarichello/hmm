using System;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Localization;
using UnityEngine;

namespace HeavyMetalMachines.Customization
{
	public class CustomizationInventorySkinSlotsCell : CustomizationInventoryCell
	{
		protected override void RefreshItem(int index)
		{
			base.RefreshItem(index);
			CustomizationInventoryCellItemData customizationInventoryCellItemData = this._itemsData[index];
			this._items[index].NameText.text = ((customizationInventoryCellItemData == null) ? string.Empty : Language.Get(customizationInventoryCellItemData.ItemDescription, TranslationContext.Items));
			this._items[index].BgImage.SetAlpha((customizationInventoryCellItemData == null) ? this._disabledBgAlpha : 1f);
			this._items[index].Image.gameObject.SetActive(this._items[index].Image.enabled);
		}

		[SerializeField]
		private float _disabledBgAlpha = 0.3f;
	}
}
