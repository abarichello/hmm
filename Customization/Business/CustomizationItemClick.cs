using System;
using HeavyMetalMachines.Frontend;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HeavyMetalMachines.Customization.Business
{
	public class CustomizationItemClick : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
	{
		private void Awake()
		{
			this._button = base.GetComponent<HmmUiButton>();
		}

		private void OnEnable()
		{
			this._itemChangeDisposable = ObservableExtensions.Subscribe<ItemChangeRequestState>(Observable.Do<ItemChangeRequestState>(this._inventoryComponent.OnItemEquipChanged, new Action<ItemChangeRequestState>(this.InventoryComponentOnOnItemEquipChanged)));
			this._inventoryComponent.OnReceivedItemEquipChangedCallback += this.OnReceivedItemEquipChangedCallback;
		}

		private void OnDisable()
		{
			if (this._itemChangeDisposable != null)
			{
				this._itemChangeDisposable.Dispose();
				this._itemChangeDisposable = null;
			}
			this._inventoryComponent.OnReceivedItemEquipChangedCallback -= this.OnReceivedItemEquipChangedCallback;
		}

		private void InventoryComponentOnOnItemEquipChanged(ItemChangeRequestState e)
		{
			if (e.CategoryId == this._categoryId)
			{
				this._button.interactable = false;
			}
		}

		private void OnReceivedItemEquipChangedCallback(ItemChangeRequestState e)
		{
			if (e.CategoryId == this._categoryId)
			{
				this._button.interactable = true;
			}
		}

		public void Setup(Guid itemId, Guid categoryId)
		{
			this._categoryId = categoryId;
			this._itemId = itemId;
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (!this._button.IsInteractable())
			{
				return;
			}
			PointerEventData.InputButton button = eventData.button;
			if (button == null || button == 1)
			{
				this._customizationInventoryCell.OnItemSelected(base.transform);
				CustomizationInventoryCellItemData item = this._inventoryComponent.GetItem(this._itemId);
				this._inventoryComponent.MarkItemAsSeen(item);
				if (!this._inventoryComponent.GetIsItemEquiped(item))
				{
					this._inventoryComponent.EquipItem(this._itemId);
				}
				else if (this._inventoryComponent.IsUnequipable(item))
				{
					this._inventoryComponent.UnequipItem(this._itemId);
				}
			}
		}

		[SerializeField]
		private CustomizationInventoryCell _customizationInventoryCell;

		[SerializeField]
		private CustomizationInventoryComponent _inventoryComponent;

		private IDisposable _itemChangeDisposable;

		private Guid _itemId;

		private HmmUiButton _button;

		private Guid _categoryId;
	}
}
