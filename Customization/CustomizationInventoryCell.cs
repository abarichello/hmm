using System;
using EnhancedUI.EnhancedScroller;
using HeavyMetalMachines.Customization.Business;
using HeavyMetalMachines.Frontend;
using Hoplon.Input.UiNavigation.AxisSelector;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Customization
{
	public class CustomizationInventoryCell : EnhancedScrollerCellView
	{
		private void Awake()
		{
			this._itemsData = new CustomizationInventoryCellItemData[this._items.Length];
		}

		public void Setup(int index, CustomizationInventoryCellItemData data, CustomizationInventoryCell.OnCellItemSelected onItemSelectedCallback, CustomizationInventoryComponent inventoryComponent, IUiNavigationAxisSelectorTransformHandler uiNavigationAxisSelectorTransformHandler)
		{
			this._onItemSelectedCallback = onItemSelectedCallback;
			this._itemsData[index] = data;
			this._inventoryComponent = inventoryComponent;
			this._uiNavigationAxisSelectorTransformHandler = uiNavigationAxisSelectorTransformHandler;
			this.RefreshItem(index);
		}

		[UnityUiComponentCall]
		public void OnItemSelected(Transform root)
		{
			for (int i = 0; i < this._items.Length; i++)
			{
				CustomizationInventoryCell.CustomizationInventoryItem customizationInventoryItem = this._items[i];
				if (customizationInventoryItem.RootTransform == root)
				{
					this._onItemSelectedCallback(customizationInventoryItem.ItemTypeId);
					break;
				}
			}
		}

		public override void RefreshCellView()
		{
			base.RefreshCellView();
			for (int i = 0; i < this._items.Length; i++)
			{
				this.RefreshItem(i);
			}
		}

		protected virtual void RefreshItem(int index)
		{
			CustomizationInventoryCell.CustomizationInventoryItem customizationInventoryItem = this._items[index];
			CustomizationInventoryCellItemData customizationInventoryCellItemData = this._itemsData[index];
			if (customizationInventoryCellItemData == null)
			{
				for (int i = 0; i < customizationInventoryItem.EquippedImages.Length; i++)
				{
					customizationInventoryItem.EquippedImages[i].gameObject.SetActive(false);
				}
				customizationInventoryItem.SelectedImage.gameObject.SetActive(false);
				customizationInventoryItem.Image.enabled = false;
				customizationInventoryItem.NewItemGameObject.SetActive(false);
				customizationInventoryItem.Button.interactable = false;
				return;
			}
			customizationInventoryItem.RootTransform.gameObject.SetActive(true);
			for (int j = 0; j < customizationInventoryItem.EquippedImages.Length; j++)
			{
				customizationInventoryItem.EquippedImages[j].gameObject.SetActive(this._inventoryComponent.GetIsItemEquiped(customizationInventoryCellItemData));
			}
			bool isSelected = customizationInventoryCellItemData.IsSelected;
			customizationInventoryItem.SelectedImage.gameObject.SetActive(isSelected);
			customizationInventoryItem.Image.enabled = true;
			if (string.IsNullOrEmpty(customizationInventoryCellItemData.IconName))
			{
				customizationInventoryItem.Image.ClearAsset();
			}
			else
			{
				customizationInventoryItem.Image.TryToLoadAsset(customizationInventoryCellItemData.IconName);
			}
			customizationInventoryItem.NewItemGameObject.SetActive(customizationInventoryCellItemData.IsNew);
			customizationInventoryItem.Button.interactable = !isSelected;
			this._items[index].ItemTypeId = customizationInventoryCellItemData.ItemTypeId;
			if (customizationInventoryItem.OnClickItem != null)
			{
				customizationInventoryItem.OnClickItem.Setup(customizationInventoryCellItemData.ItemTypeId, customizationInventoryCellItemData.ItemCategoryId);
			}
			if (customizationInventoryItem.OnHoverItem != null)
			{
				customizationInventoryItem.OnHoverItem.Setup(customizationInventoryCellItemData);
			}
			if (isSelected)
			{
				this._uiNavigationAxisSelectorTransformHandler.TryForceSelection(customizationInventoryItem.RootTransform);
			}
		}

		[SerializeField]
		protected CustomizationInventoryCell.CustomizationInventoryItem[] _items;

		protected CustomizationInventoryCellItemData[] _itemsData;

		private CustomizationInventoryCell.OnCellItemSelected _onItemSelectedCallback;

		private CustomizationInventoryComponent _inventoryComponent;

		private IUiNavigationAxisSelectorTransformHandler _uiNavigationAxisSelectorTransformHandler;

		[Serializable]
		protected struct CustomizationInventoryItem
		{
			public Transform RootTransform;

			public HmmUiRawImage Image;

			public Image[] EquippedImages;

			public Image SelectedImage;

			public GameObject NewItemGameObject;

			public HmmUiButton Button;

			public Guid ItemTypeId;

			public Text NameText;

			public Image BgImage;

			public CustomizationItemClick OnClickItem;

			public CustomizationItemHover OnHoverItem;
		}

		public delegate void OnCellItemSelected(Guid itemTypeId);
	}
}
