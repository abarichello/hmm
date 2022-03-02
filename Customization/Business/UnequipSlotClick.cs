using System;
using Assets.ClientApiObjects;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HeavyMetalMachines.Customization.Business
{
	public class UnequipSlotClick : MonoBehaviour, IPointerDownHandler, IUnequipItem, IEventSystemHandler
	{
		public void OnPointerDown(PointerEventData eventData)
		{
			if (this._itemType != null)
			{
				this._inventoryComponent.UnequipItem(this._itemType.Id);
			}
		}

		public void Setup(IItemType itemType)
		{
			this._itemType = itemType;
		}

		[SerializeField]
		private CustomizationInventoryComponent _inventoryComponent;

		private IItemType _itemType;
	}
}
