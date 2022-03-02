using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HeavyMetalMachines.Customization.Business
{
	public class CustomizationItemHover : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler
	{
		public void Setup(CustomizationInventoryCellItemData itemData)
		{
			this._itemData = itemData;
		}

		public void OnSelect(BaseEventData eventData)
		{
			this.Select();
		}

		public void OnDeselect(BaseEventData eventData)
		{
			this.Deselect();
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			this.Select();
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			this.Deselect();
		}

		private void Select()
		{
			this._inventoryComponent.RaiseItemHoverChange(this._itemData);
		}

		private void Deselect()
		{
			this._inventoryComponent.RaiseItemHoverChange(null);
		}

		[SerializeField]
		private CustomizationInventoryComponent _inventoryComponent;

		private CustomizationInventoryCellItemData _itemData;
	}
}
