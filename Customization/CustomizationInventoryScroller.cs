using System;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using HeavyMetalMachines.DataTransferObjects.Util;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Inventory.View;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Presenting;
using Hoplon.Input.UiNavigation.AxisSelector;
using Hoplon.Input.UiNavigation.ScrollSelector;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.Customization
{
	public class CustomizationInventoryScroller : MonoBehaviour, IEnhancedScrollerDelegate
	{
		private IUiNavigationAxisSelector UiNavigationAxisSelector
		{
			get
			{
				return this._uiNavigationAxisSelector;
			}
		}

		private IUiNavigationAxisSelectorTransformHandler UiNavigationAxisSelectorTransformHandler
		{
			get
			{
				return this._uiNavigationAxisSelector;
			}
		}

		private void Start()
		{
			GameObject gameObject = this._cellPrefab.gameObject;
			this._itemsPerCell = gameObject.transform.childCount;
			this._scroller.Delegate = this;
			this._cellViewSize = gameObject.GetComponent<RectTransform>().sizeDelta.y;
			this._skinsCellViewSize = this._skinsCellPrefab.GetComponent<RectTransform>().sizeDelta.y;
			this._numberOfVisibleCells = Mathf.RoundToInt((base.transform as RectTransform).rect.height / this._cellViewSize);
			this.InitializeCharacterSelection();
		}

		private void InitializeCharacterSelection()
		{
			ObservableExtensions.Subscribe<Unit>(this._inventoryCharacterSelectionView.RightButton.OnClick(), delegate(Unit _)
			{
				this.CycleCharacterRight();
			});
			ObservableExtensions.Subscribe<Unit>(this._inventoryCharacterSelectionView.LeftButton.OnClick(), delegate(Unit _)
			{
				this.CycleCharacterLeft();
			});
			ObservableExtensions.Subscribe<int>(Observable.Do<int>(this._inventoryCharacterSelectionView.CharactersDropdown.OnSelectionChanged(), new Action<int>(this.OnCharactersDropdownSelectionChanged)));
		}

		public int GetNumberOfCells(EnhancedScroller scroller)
		{
			if (this.IsCurrentCategorySkin())
			{
				int skinScrollerCellCount = this.GetSkinScrollerCellCount();
				return (skinScrollerCellCount != 0) ? skinScrollerCellCount : this._cellSkinEmptyQuantity;
			}
			return this._numberOfCells;
		}

		public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
		{
			if (this.IsCurrentCategorySkin())
			{
				return this._skinsCellViewSize;
			}
			return this._cellViewSize;
		}

		public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
		{
			if (this.IsCurrentCategoryEmote())
			{
				return this.GetCustomizationInventoryCellView(dataIndex, this._animatedCellPrefab);
			}
			if (!this.IsCurrentCategorySkin())
			{
				return this.GetCustomizationInventoryCellView(dataIndex, this._cellPrefab);
			}
			return this.GetCustomizationInventorySkinCellView(dataIndex);
		}

		private bool IsCurrentCategorySkin()
		{
			return this._currentCategoryData != null && this._currentCategoryData.CategoryId == InventoryMapper.SkinsCategoryGuid;
		}

		private bool HasCharacterSkin()
		{
			return this._currentCategoryData != null && this._currentCategoryData.SortedCharacterIds.Count > 0;
		}

		private bool IsCurrentCategoryEmote()
		{
			return this._currentCategoryData != null && this._currentCategoryData.CategoryId == InventoryMapper.EmoteCategoryGuid;
		}

		private EnhancedScrollerCellView GetCustomizationInventoryCellView(int dataIndex, CustomizationInventoryCell prefab)
		{
			EnhancedScrollerCellView cellView = this._scroller.GetCellView(prefab);
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
				customizationInventoryCell.Setup(i, data, new CustomizationInventoryCell.OnCellItemSelected(this.OnItemSelected), this._inventoryComponent, this.UiNavigationAxisSelectorTransformHandler);
			}
			return customizationInventoryCell;
		}

		private EnhancedScrollerCellView GetCustomizationInventorySkinCellView(int dataIndex)
		{
			if (this._currentCategoryData.Items.Count == 0)
			{
				CustomizationInventorySkinSlotsCell customizationInventorySkinSlotsCell = (CustomizationInventorySkinSlotsCell)this._scroller.GetCellView(this._skinsCellPrefab);
				for (int i = 0; i < 3; i++)
				{
					customizationInventorySkinSlotsCell.Setup(i, null, new CustomizationInventoryCell.OnCellItemSelected(this.OnItemSelected), this._inventoryComponent, this.UiNavigationAxisSelectorTransformHandler);
				}
				return customizationInventorySkinSlotsCell;
			}
			int num = dataIndex * 3;
			List<CustomizationInventoryCellItemData> currentCharacterSkinItems = this.GetCurrentCharacterSkinItems();
			CustomizationInventorySkinSlotsCell customizationInventorySkinSlotsCell2 = (CustomizationInventorySkinSlotsCell)this._scroller.GetCellView(this._skinsCellPrefab);
			for (int j = 0; j < 3; j++)
			{
				int num2 = num + j;
				CustomizationInventoryCellItemData data = (num2 >= currentCharacterSkinItems.Count) ? null : currentCharacterSkinItems[num2];
				customizationInventorySkinSlotsCell2.Setup(j, data, new CustomizationInventoryCell.OnCellItemSelected(this.OnItemSelected), this._inventoryComponent, this.UiNavigationAxisSelectorTransformHandler);
			}
			return customizationInventorySkinSlotsCell2;
		}

		public void Setup(CustomizationInventoryCategoryData data, CustomizationInventoryScroller.OnCustomizationItemSelectedDelegate onCustomizationIntemSelected)
		{
			this._currentCategoryData = data;
			this._onCustomizationItemSelected = onCustomizationIntemSelected;
			this._numberOfCells = Mathf.CeilToInt((float)this._currentCategoryData.Items.Count / (float)this._itemsPerCell);
			this._numberOfCells = Math.Max(this._numberOfCells, this._minNumberOfCells);
			this._scroller.SetSpacing((!(data.CategoryId == InventoryMapper.SkinsCategoryGuid)) ? this._cellSpacing : this._cellSkinSpacing);
			this._scroller.ScrollPosition = 0f;
			this._scroller.ReloadData(0f);
			if (this.IsCurrentCategoryEmote())
			{
				this.UiNavigationAxisSelector.RebuildAndSelect();
				this.UiNavigationAxisSelector.SetDoClickOnNavigationOff();
			}
			else
			{
				this.UiNavigationAxisSelector.Rebuild();
				this.UiNavigationAxisSelector.SetDoClickOnNavigationOn();
			}
			this._uiNavigationItemContextScroller.ForceSnap();
			this.SetupCharacterSelection();
		}

		private void SetupCharacterSelection()
		{
			if (!this.IsCurrentCategorySkin() || !this.HasCharacterSkin())
			{
				ActivatableExtensions.Deactivate(this._inventoryCharacterSelectionView.CharactersSelectionGroupActivatable);
				return;
			}
			ActivatableExtensions.Activate(this._inventoryCharacterSelectionView.CharactersSelectionGroupActivatable);
			this._inventoryCharacterSelectionView.CharactersDropdown.ClearOptions();
			List<Guid> sortedCharacterIds = this._currentCategoryData.SortedCharacterIds;
			List<int> list = new List<int>(sortedCharacterIds.Count);
			List<string> list2 = new List<string>();
			for (int i = 0; i < sortedCharacterIds.Count; i++)
			{
				list.Add(i);
				Guid key = sortedCharacterIds[i];
				CustomizationInventoryCellItemData customizationInventoryCellItemData = this._currentCategoryData.CharacterIdToItemsDictionary[key][0];
				list2.Add(Language.Get(customizationInventoryCellItemData.ItemName, TranslationContext.Items));
			}
			this._inventoryCharacterSelectionView.CharactersDropdown.AddOptions(list, list2);
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

		private void CycleCharacterRight()
		{
			if (++this._currentCharacterIndex >= this._currentCategoryData.SortedCharacterIds.Count)
			{
				this._currentCharacterIndex = 0;
			}
			this.UpdateCharactersDropdownSelectedOption();
			this.ReloadAndSelectFirstCharacterSkinItem();
			this.UiNavigationAxisSelector.Rebuild();
		}

		private void CycleCharacterLeft()
		{
			if (--this._currentCharacterIndex < 0)
			{
				this._currentCharacterIndex = this._currentCategoryData.SortedCharacterIds.Count - 1;
			}
			this.UpdateCharactersDropdownSelectedOption();
			this.ReloadAndSelectFirstCharacterSkinItem();
			this.UiNavigationAxisSelector.Rebuild();
		}

		private void OnCharactersDropdownSelectionChanged(int dropdownIndex)
		{
			if (this._currentCharacterIndex != dropdownIndex)
			{
				this._currentCharacterIndex = dropdownIndex;
				this.ReloadAndSelectFirstCharacterSkinItem();
				this.UiNavigationAxisSelector.Rebuild();
			}
		}

		private void UpdateCharactersDropdownSelectedOption()
		{
			this._inventoryCharacterSelectionView.CharactersDropdown.SelectedOption = this._currentCharacterIndex;
		}

		private void ReloadAndSelectFirstCharacterSkinItem()
		{
			this._scroller.ScrollPosition = 0f;
			this._scroller.ReloadData(0f);
			List<CustomizationInventoryCellItemData> currentCharacterSkinItems = this.GetCurrentCharacterSkinItems();
			if (currentCharacterSkinItems.Count > 0)
			{
				this.OnItemSelected(currentCharacterSkinItems[0].ItemTypeId);
			}
		}

		private List<CustomizationInventoryCellItemData> GetCurrentCharacterSkinItems()
		{
			Guid key = this._currentCategoryData.SortedCharacterIds[this._currentCharacterIndex];
			return this._currentCategoryData.CharacterIdToItemsDictionary[key];
		}

		private int GetSkinScrollerCellCount()
		{
			if (this._currentCategoryData.SortedCharacterIds.Count == 0)
			{
				return 0;
			}
			Guid key = this._currentCategoryData.SortedCharacterIds[this._currentCharacterIndex];
			int count = this._currentCategoryData.CharacterIdToItemsDictionary[key].Count;
			int num = count / 3;
			if (count % 3 > 0)
			{
				num++;
			}
			return num;
		}

		[SerializeField]
		private EnhancedScroller _scroller;

		[SerializeField]
		private CustomizationInventoryComponent _inventoryComponent;

		[SerializeField]
		private CustomizationInventoryCell _cellPrefab;

		[SerializeField]
		private CustomizationInventoryCell _animatedCellPrefab;

		[SerializeField]
		private CustomizationInventorySkinSlotsCell _skinsCellPrefab;

		[SerializeField]
		private UiNavigationAxisSelector _uiNavigationAxisSelector;

		[SerializeField]
		private InventoryCharacterSelectionView _inventoryCharacterSelectionView;

		[SerializeField]
		private UiNavigationItemContextScroller _uiNavigationItemContextScroller;

		[SerializeField]
		private int _minNumberOfCells = 3;

		[SerializeField]
		private int _cellSpacing = 25;

		[SerializeField]
		private int _cellSkinSpacing = 10;

		[SerializeField]
		private int _cellSkinEmptyQuantity = 4;

		private int _numberOfCells = 1;

		private int _numberOfVisibleCells = 3;

		private int _itemsPerCell;

		private float _cellViewSize;

		private float _skinsCellViewSize;

		private int _currentCharacterIndex;

		private CustomizationInventoryCategoryData _currentCategoryData;

		private CustomizationInventoryScroller.OnCustomizationItemSelectedDelegate _onCustomizationItemSelected;

		public delegate void OnCustomizationItemSelectedDelegate(Guid categoryId, Guid itemTypeId);
	}
}
