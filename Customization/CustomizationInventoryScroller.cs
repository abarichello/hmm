using System;
using System.Collections.Generic;
using Commons.Swordfish.Battlepass;
using EnhancedUI.EnhancedScroller;
using HeavyMetalMachines.Frontend;
using UnityEngine;

namespace HeavyMetalMachines.Customization
{
	public class CustomizationInventoryScroller : MonoBehaviour, IEnhancedScrollerDelegate
	{
		private void Start()
		{
			GameObject gameObject = this._cellPrefab.gameObject;
			this._itemsPerCell = gameObject.transform.childCount;
			this._scroller.Delegate = this;
			this._cellViewSize = gameObject.GetComponent<RectTransform>().sizeDelta.y;
			this._skinsCellViewSize = this._skinsCellPrefab.GetComponent<RectTransform>().sizeDelta.y;
			this._skinTabCellViewSize = this._skinTabCellPrefab.GetComponent<RectTransform>().sizeDelta.y;
			this._numberOfVisibleCells = Mathf.RoundToInt((base.transform as RectTransform).rect.height / this._cellViewSize);
		}

		public int GetNumberOfCells(EnhancedScroller scroller)
		{
			if (this.IsCurrentCategorySkin())
			{
				int skinScrollerCellCount = this._currentCategoryData.GetSkinScrollerCellCount();
				return (skinScrollerCellCount != 0) ? skinScrollerCellCount : 3;
			}
			return this._numberOfCells;
		}

		public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
		{
			if (!this.IsCurrentCategorySkin())
			{
				return this._cellViewSize;
			}
			if (this._currentCategoryData.Items.Count == 0)
			{
				return this._skinsCellViewSize;
			}
			int num;
			int num2;
			if (this._currentCategoryData.SkinIndexIsTab(dataIndex, out num, out num2))
			{
				return this._skinTabCellViewSize;
			}
			float num3 = this._skinsCellViewSize;
			int num4 = this._currentCategoryData.SkinTabToCellCountDictionary[num];
			if (num < this._currentCategoryData.SkinTabDataItems.Count - 1 && num2 == num4 - 1)
			{
				num3 += (float)this._cellSkinTabSpacing;
			}
			return num3;
		}

		public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
		{
			if (!this.IsCurrentCategorySkin())
			{
				return this.GetCustomizationInventoryCellView(dataIndex);
			}
			return this.GetCustomizationInventorySkinCellView(dataIndex);
		}

		private bool IsCurrentCategorySkin()
		{
			return this._currentCategoryData != null && this._currentCategoryData.CustomizationSlot == PlayerCustomizationSlot.Skin;
		}

		private EnhancedScrollerCellView GetCustomizationInventoryCellView(int dataIndex)
		{
			EnhancedScrollerCellView cellView = this._scroller.GetCellView(this._cellPrefab);
			CustomizationInventoryCell customizationInventoryCell = cellView as CustomizationInventoryCell;
			if (this._currentCategoryData == null)
			{
				return customizationInventoryCell;
			}
			int num = dataIndex * this._itemsPerCell;
			List<CustomizationInventoryCellItemData> items = this._currentCategoryData.Items;
			for (int i = 0; i < this._itemsPerCell; i++)
			{
				int num2 = num + i;
				CustomizationInventoryCellItemData data = (num2 >= items.Count) ? null : items[num2];
				customizationInventoryCell.Setup(i, data, new CustomizationInventoryCell.OnCellItemSelected(this.OnItemSelected));
			}
			return customizationInventoryCell;
		}

		private EnhancedScrollerCellView GetCustomizationInventorySkinCellView(int dataIndex)
		{
			if (this._currentCategoryData.Items.Count == 0)
			{
				CustomizationInventorySkinSlotsCell customizationInventorySkinSlotsCell = this._scroller.GetCellView(this._skinsCellPrefab) as CustomizationInventorySkinSlotsCell;
				for (int i = 0; i < 3; i++)
				{
					customizationInventorySkinSlotsCell.Setup(i, null, new CustomizationInventoryCell.OnCellItemSelected(this.OnItemSelected));
				}
				return customizationInventorySkinSlotsCell;
			}
			int num;
			int num2;
			if (this._currentCategoryData.SkinIndexIsTab(dataIndex, out num, out num2))
			{
				CustomizationInventorySkinTabCell customizationInventorySkinTabCell = this._scroller.GetCellView(this._skinTabCellPrefab) as CustomizationInventorySkinTabCell;
				customizationInventorySkinTabCell.Setup(this, num, this._currentCategoryData.SkinTabDataItems[num], new CustomizationInventorySkinTabCell.OnTabCellToggle(this.OnTabCellToggleCallback));
				return customizationInventorySkinTabCell;
			}
			int num3 = num2 * 3;
			List<CustomizationInventoryCellItemData> list = this._currentCategoryData.SkinTabToItemsDictionary[num];
			CustomizationInventorySkinSlotsCell customizationInventorySkinSlotsCell2 = this._scroller.GetCellView(this._skinsCellPrefab) as CustomizationInventorySkinSlotsCell;
			for (int j = 0; j < 3; j++)
			{
				int num4 = num3 + j;
				CustomizationInventoryCellItemData data = (num4 >= list.Count) ? null : list[num4];
				customizationInventorySkinSlotsCell2.Setup(j, data, new CustomizationInventoryCell.OnCellItemSelected(this.OnItemSelected));
			}
			return customizationInventorySkinSlotsCell2;
		}

		private void OnTabCellToggleCallback(int tabIndex, bool isExpanded)
		{
			this._currentCategoryData.SkinTabDataItems[tabIndex].IsExpanded = isExpanded;
			this._scroller.ReloadData(this._scroller.NormalizedScrollPosition);
		}

		public void Setup(CustomizationInventoryCategoryData data, CustomizationInventoryScroller.OnCustomizationItemSelectedDelegate onCustomizationIntemSelected)
		{
			this._currentCategoryData = data;
			this._onCustomizationItemSelected = onCustomizationIntemSelected;
			this._numberOfCells = Mathf.CeilToInt((float)this._currentCategoryData.Items.Count / (float)this._itemsPerCell);
			this._numberOfCells = Math.Max(this._numberOfCells, this._minNumberOfCells);
			this._scroller.SetSpacing((data.CustomizationSlot != PlayerCustomizationSlot.Skin) ? this._cellSpacing : this._cellSkinSpacing);
			this._scroller.ReloadData(0f);
		}

		public void Refresh()
		{
			this._scroller.RefreshActiveCellViews();
		}

		public void JumpToCellIndex(int index)
		{
			int num = index / this._itemsPerCell;
			if (num < this._numberOfVisibleCells)
			{
				num = 0;
			}
			else
			{
				int num2 = this._numberOfCells - this._numberOfVisibleCells;
				if (num > num2)
				{
					num = num2;
				}
			}
			this._scroller.JumpToDataIndex(num, 0f, 0f, true, EnhancedScroller.TweenType.immediate, 0f, null, EnhancedScroller.LoopJumpDirectionEnum.Closest);
		}

		private void OnItemSelected(Guid itemTypeId)
		{
			Guid categoryId = this._currentCategoryData.CategoryId;
			this._onCustomizationItemSelected(categoryId, itemTypeId);
			this.Refresh();
		}

		public CustomizationInventoryCellItemSkinTabData GetItemSkinTabData(int tabIndex)
		{
			return this._currentCategoryData.SkinTabDataItems[tabIndex];
		}

		public Dictionary<Guid, bool> GetExpandSkinState()
		{
			Dictionary<Guid, bool> dictionary = new Dictionary<Guid, bool>(20);
			for (int i = 0; i < this._currentCategoryData.SkinTabDataItems.Count; i++)
			{
				CustomizationInventoryCellItemSkinTabData customizationInventoryCellItemSkinTabData = this._currentCategoryData.SkinTabDataItems[i];
				dictionary.Add(customizationInventoryCellItemSkinTabData.CharacterId, customizationInventoryCellItemSkinTabData.IsExpanded);
			}
			return dictionary;
		}

		[SerializeField]
		private EnhancedScroller _scroller;

		[SerializeField]
		private CustomizationInventoryCell _cellPrefab;

		[SerializeField]
		private CustomizationInventorySkinSlotsCell _skinsCellPrefab;

		[SerializeField]
		private CustomizationInventorySkinTabCell _skinTabCellPrefab;

		[SerializeField]
		private int _minNumberOfCells = 3;

		[SerializeField]
		private int _cellSpacing = 25;

		[SerializeField]
		private int _cellSkinSpacing = 10;

		[SerializeField]
		private int _cellSkinTabSpacing = 20;

		private int _numberOfCells = 1;

		private int _numberOfVisibleCells = 3;

		private int _itemsPerCell;

		private float _cellViewSize;

		private float _skinsCellViewSize;

		private float _skinTabCellViewSize;

		private CustomizationInventoryCategoryData _currentCategoryData;

		private CustomizationInventoryScroller.OnCustomizationItemSelectedDelegate _onCustomizationItemSelected;

		public delegate void OnCustomizationItemSelectedDelegate(Guid categoryId, Guid itemTypeId);
	}
}
