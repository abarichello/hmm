using System;
using EnhancedUI.EnhancedScroller;
using HeavyMetalMachines.Frontend;
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

		public void Setup(int index, CustomizationInventoryCellItemData data, CustomizationInventoryCell.OnCellItemSelected onItemSelectedCallback)
		{
			this._onItemSelectedCallback = onItemSelectedCallback;
			this._itemsData[index] = data;
			this.RefreshItem(index);
		}

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
				customizationInventoryItem.EquippedImages[j].gameObject.SetActive(customizationInventoryCellItemData.IsEquipped);
			}
			customizationInventoryItem.SelectedImage.gameObject.SetActive(customizationInventoryCellItemData.IsSelected);
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
			customizationInventoryItem.Button.interactable = !customizationInventoryCellItemData.IsSelected;
			this._items[index].ItemTypeId = customizationInventoryCellItemData.ItemTypeId;
		}

		[SerializeField]
		protected CustomizationInventoryCell.CustomizationInventoryItem[] _items;

		protected CustomizationInventoryCellItemData[] _itemsData;

		private CustomizationInventoryCell.OnCellItemSelected _onItemSelectedCallback;

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
		}

		public delegate void OnCellItemSelected(Guid itemTypeId);
	}
}
