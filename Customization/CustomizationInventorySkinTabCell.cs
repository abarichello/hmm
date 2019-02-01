using System;
using EnhancedUI.EnhancedScroller;
using HeavyMetalMachines.Frontend;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Customization
{
	public class CustomizationInventorySkinTabCell : EnhancedScrollerCellView
	{
		public void Setup(CustomizationInventoryScroller scroller, int tabIndex, CustomizationInventoryCellItemSkinTabData skinTabData, CustomizationInventorySkinTabCell.OnTabCellToggle onTabCellToggleCallback)
		{
			this._scroller = scroller;
			this._tabIndex = tabIndex;
			this._onTabCellToggleCallback = onTabCellToggleCallback;
			this._characterNameText.text = skinTabData.CharacterName;
			this._iconImage.SetAlpha((!skinTabData.HasCharacter) ? this._disabledIconAlpha : 1f);
			this._iconImage.TryToLoadAsset(skinTabData.IconAssetName);
			this._newItemGameObject.SetActive(skinTabData.IsNew);
			this._isCollapsed = skinTabData.IsExpanded;
			this.SetArrowRotation(skinTabData.IsExpanded);
		}

		private void SetArrowRotation(bool isCollapsed)
		{
			this._arrowRectTransform.localRotation = Quaternion.Euler(0f, 0f, (!isCollapsed) ? 90f : 0f);
		}

		[UnityUiComponentCall]
		public void OnTabClick()
		{
			this._isCollapsed = !this._isCollapsed;
			this.SetArrowRotation(this._isCollapsed);
			if (this._onTabCellToggleCallback != null)
			{
				this._onTabCellToggleCallback(this._tabIndex, this._isCollapsed);
			}
		}

		public override void RefreshCellView()
		{
			base.RefreshCellView();
			CustomizationInventoryCellItemSkinTabData itemSkinTabData = this._scroller.GetItemSkinTabData(this._tabIndex);
			this._newItemGameObject.SetActive(itemSkinTabData.IsNew);
		}

		[SerializeField]
		private Text _characterNameText;

		[SerializeField]
		private HmmUiImage _iconImage;

		[SerializeField]
		private float _disabledIconAlpha = 0.3f;

		[SerializeField]
		private RectTransform _arrowRectTransform;

		[SerializeField]
		private GameObject _newItemGameObject;

		private CustomizationInventorySkinTabCell.OnTabCellToggle _onTabCellToggleCallback;

		private int _tabIndex;

		private bool _isCollapsed;

		private CustomizationInventoryScroller _scroller;

		public delegate void OnTabCellToggle(int tabIndex, bool isExpanded);
	}
}
