using System;
using System.Collections;
using System.Collections.Generic;
using Assets.ClientApiObjects.Specializations;
using Commons.Swordfish.Battlepass;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Options;
using HeavyMetalMachines.UnityUI;
using HeavyMetalMachines.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Customization
{
	public class CustomizationInventoryView : MonoBehaviour, ICustomizationInventoryView
	{
		private void OnEnable()
		{
			GUIUtils.ResetAnimation(this._mainWindowAnimation);
			this._inventoryComponent.RegisterView(this);
			this.InitializeShortcupFeedback();
			this._artPreview.gameObject.SetActive(false);
			this._equipItemFailedAnimation.Rewind();
			this._inventoryComponent.OnSkinTabHasNewItemsStateChanged += this.InventoryComponentOnSkinTabHasNewItemsStateChanged;
		}

		private void OnDisable()
		{
			this._inventoryComponent.OnSkinTabHasNewItemsStateChanged -= this.InventoryComponentOnSkinTabHasNewItemsStateChanged;
		}

		private void InitializeShortcupFeedback()
		{
			this._shortcutFeedbackGroup.SetActive(false);
			string textlocalized = ControlOptions.GetTextlocalized(ControlAction.Spray, ControlOptions.ControlActionInputType.Primary);
			this._shortcutFeedbackText.text = string.Format(Language.Get("INVENTORY_SPRAY_USE_HINT", TranslationSheets.Inventory), textlocalized);
		}

		public void RegisterCategoriesView(ICustomizationInventoryCategoriesView categoriesView)
		{
			this._categoriesView = categoriesView;
		}

		public int GetNewItemsCount(Guid categoryId)
		{
			return this._inventoryComponent.GetCategoryNewItemsCount(categoryId);
		}

		private void OnCustomizationItemSelected(Guid categoryId, Guid itemTypeId)
		{
			if (this._selectedItemTypeId == itemTypeId)
			{
				return;
			}
			this.DeselectCurrentItem();
			this._selectedItemTypeId = itemTypeId;
			CustomizationInventoryCellItemData item = this._inventoryComponent.GetItem(categoryId, itemTypeId);
			item.IsSelected = true;
			int newItemsCount = this._inventoryComponent.MarkItemAsSeen(item);
			this._categoriesView.UpdateNewItemsMarker(categoryId, newItemsCount);
			UnityUiBattlepassArtPreview.ArtPreviewLoreData loreData = new UnityUiBattlepassArtPreview.ArtPreviewLoreData
			{
				IsLocked = false,
				DescriptionText = item.LoreDescriptionDraft,
				SubtitleText = item.LoreSubtitleDraft,
				TitleText = item.LoreTitleDraft
			};
			UnityUiBattlepassArtPreview.ArtPreviewData artPreviewData = new UnityUiBattlepassArtPreview.ArtPreviewData
			{
				RewardAssetKind = item.PreviewKind,
				RewardAssetName = item.PreviewName,
				TitleText = item.ItemName,
				DescriptionText = item.ItemDescription,
				ShowCurrencyIcon = false,
				LoreData = loreData,
				ArtPreviewBackGroundAssetName = item.ArtPreviewBackGroundAssetName,
				SkinCustomizations = item.SkinCustomizations
			};
			this._equipItemButton.gameObject.SetActive(!item.IsEquipped && item.PreviewKind != ItemPreviewKind.Lore && this._selectedCustomizationSlot != PlayerCustomizationSlot.Skin);
			this._equipItemButton.interactable = !item.IsEquipped;
			this._equippedTextGameObject.SetActive(item.IsEquipped);
			this._artPreview.gameObject.SetActive(true);
			this._artPreview.ShowReward(artPreviewData, true);
		}

		private void DeselectCurrentItem()
		{
			CustomizationInventoryCellItemData item = this._inventoryComponent.GetItem(this._selectedCategoryId, this._selectedItemTypeId);
			if (item != null)
			{
				item.IsSelected = false;
			}
			this._selectedItemTypeId = Guid.Empty;
		}

		public void MarkAllItemsAsSeen(Guid[] categoriesWithNewItems)
		{
			if (!this._inventoryComponent.MarkAllItemsAsSeen(categoriesWithNewItems))
			{
				return;
			}
			for (int i = 0; i < categoriesWithNewItems.Length; i++)
			{
				this._categoriesView.UpdateNewItemsMarker(categoriesWithNewItems[i], 0);
			}
			this._scroller.Refresh();
		}

		public bool IsVisible()
		{
			return this._isVisible;
		}

		public void SetVisibility(bool isVisible, bool imediate)
		{
			if (isVisible == this._isVisible || this._isAnimating)
			{
				return;
			}
			if (!isVisible)
			{
				Dictionary<Guid, bool> expandSkinState = this._scroller.GetExpandSkinState();
				foreach (KeyValuePair<Guid, bool> charIdExpandPair in expandSkinState)
				{
					this._inventoryComponent.UpdateExpandSkinState(charIdExpandPair);
				}
			}
			if (imediate)
			{
				if (isVisible)
				{
					this._mainWindowCanvas.enabled = true;
					this._mainWindowCanvasGroup.interactable = true;
					this._isVisible = true;
				}
				else
				{
					this._mainWindowCanvas.gameObject.SetActive(false);
				}
			}
			else
			{
				base.StartCoroutine(this.SetVisibilityCoroutine(isVisible));
			}
		}

		private IEnumerator SetVisibilityCoroutine(bool isVisible)
		{
			this._isAnimating = true;
			if (isVisible)
			{
				this._mainWindowCanvas.enabled = true;
				this._mainWindowCanvasGroup.interactable = true;
				this._isVisible = true;
			}
			else
			{
				this._mainWindowCanvasGroup.interactable = false;
			}
			GUIUtils.PlayAnimation(this._mainWindowAnimation, !isVisible, 1f, string.Empty);
			yield return new WaitForSeconds(this._mainWindowAnimation.clip.length);
			if (!isVisible)
			{
				this._mainWindowCanvas.enabled = false;
				this._isVisible = false;
			}
			this._isAnimating = false;
			yield break;
		}

		public void Setup(CustomizationInventoryCategoryData data)
		{
			this._categoryName.text = data.CategoryName;
			string titleText = Language.Get("INVENTORY_TITLE_NAME", TranslationSheets.Inventory);
			this._title.Setup(titleText, HmmUiText.TextStyles.UpperCase, string.Empty, HmmUiText.TextStyles.Default);
			this._scroller.Setup(data, new CustomizationInventoryScroller.OnCustomizationItemSelectedDelegate(this.OnCustomizationItemSelected));
			this.EmptyWarningSetup(data);
			int index = 0;
			if (data.Items.Count > 0)
			{
				Guid itemTypeId = data.Items[0].ItemTypeId;
				for (int i = 0; i < data.Items.Count; i++)
				{
					if (data.Items[i].IsEquipped)
					{
						index = i;
						itemTypeId = data.Items[i].ItemTypeId;
						break;
					}
				}
				this.OnCustomizationItemSelected(data.CategoryId, itemTypeId);
			}
			else
			{
				this._artPreview.HideReward();
				this._equipItemButton.gameObject.SetActive(false);
				this._equippedTextGameObject.SetActive(false);
			}
			this._scroller.JumpToCellIndex(index);
			this._scroller.Refresh();
			this.AnimateCategoryIn();
		}

		private void EmptyWarningSetup(CustomizationInventoryCategoryData data)
		{
			if (data.Items.Count > 0)
			{
				this._emptyWarningGameObject.SetActive(false);
				return;
			}
			bool flag = data.CustomizationSlot == PlayerCustomizationSlot.Skin || data.IsLore;
			this._emptyWarningGameObject.SetActive(flag);
			if (flag)
			{
				string key = (data.CustomizationSlot != PlayerCustomizationSlot.Skin) ? "EMPTY_LORE_DRAFT" : "EMPTY_SKINS_DRAFT";
				this._emptyWarningText.text = Language.Get(key, TranslationSheets.Inventory);
			}
		}

		public void SelectCategory(Guid categoryId, PlayerCustomizationSlot customizationSlot)
		{
			if (categoryId == this._selectedCategoryId)
			{
				return;
			}
			this.DeselectCurrentItem();
			this._selectedCategoryId = categoryId;
			this._selectedCustomizationSlot = customizationSlot;
			if (!this._categoryAnimation.IsPlaying("TabAnimationOut"))
			{
				if (this._categoryAnimation.IsPlaying("TabAnimationIn"))
				{
					this._categoryAnimation.Stop();
					this._categoryAnimation.Rewind();
					this.OnAnimateCategoryOutComplete();
				}
				else
				{
					base.StartCoroutine(this.AnimateCategoryOut());
				}
			}
		}

		private IEnumerator AnimateCategoryOut()
		{
			this._categoryAnimation.Stop();
			this._categoryAnimation.Rewind();
			this._categoryAnimation.Play("TabAnimationOut");
			yield return new WaitForSeconds(this._categoryAnimation.clip.length);
			this.OnAnimateCategoryOutComplete();
			yield break;
		}

		private void OnAnimateCategoryOutComplete()
		{
			if (!this._isVisible)
			{
				return;
			}
			this._inventoryComponent.GetCategoryItems(this._selectedCategoryId);
		}

		private void AnimateCategoryIn()
		{
			this._categoryAnimation.Stop();
			this._categoryAnimation.Rewind();
			this._categoryAnimation.Play("TabAnimationIn");
			this._shortcutFeedbackGroup.SetActive(this._selectedCustomizationSlot == PlayerCustomizationSlot.Spray);
		}

		public void OnBackButtonClick()
		{
			this._inventoryComponent.HideCustomizationInventoryWindow(false);
		}

		public void OnEquipButtonClick()
		{
			this._equipItemButton.interactable = false;
			this._inventoryComponent.EquipItem(this._selectedCategoryId, this._selectedItemTypeId);
		}

		public void OnEquipItemResponse(bool success)
		{
			if (!this._isVisible)
			{
				return;
			}
			if (success)
			{
				this._scroller.Refresh();
			}
			else
			{
				this._equipItemFailedAnimation.Play();
			}
			CustomizationInventoryCellItemData item = this._inventoryComponent.GetItem(this._selectedCategoryId, this._selectedItemTypeId);
			if (item != null)
			{
				this._equipItemButton.interactable = !item.IsEquipped;
				this._equipItemButton.gameObject.SetActive(!item.IsEquipped);
				this._equippedTextGameObject.SetActive(item.IsEquipped);
			}
		}

		private void InventoryComponentOnSkinTabHasNewItemsStateChanged()
		{
			this._scroller.Refresh();
		}

		private const string CATEGORY_IN_ANIMATION_NAME = "TabAnimationIn";

		private const string CATEGORY_OUT_ANIMATION_NAME = "TabAnimationOut";

		[Header("Data")]
		[SerializeField]
		private CustomizationInventoryComponent _inventoryComponent;

		[Header("Main window")]
		[SerializeField]
		private Canvas _mainWindowCanvas;

		[SerializeField]
		private CanvasGroup _mainWindowCanvasGroup;

		[SerializeField]
		private Animation _mainWindowAnimation;

		[SerializeField]
		private Button _equipItemButton;

		[SerializeField]
		private GameObject _equippedTextGameObject;

		[SerializeField]
		private Animation _equipItemFailedAnimation;

		[SerializeField]
		private UnityUiTitleInfo _title;

		[Header("Preview")]
		[SerializeField]
		private UnityUiBattlepassArtPreview _artPreview;

		[Header("Categories")]
		[SerializeField]
		private Text _categoryName;

		[SerializeField]
		private Animation _categoryAnimation;

		[SerializeField]
		private GameObject _shortcutFeedbackGroup;

		[SerializeField]
		private Text _shortcutFeedbackText;

		[Header("Empty Warning")]
		[SerializeField]
		private GameObject _emptyWarningGameObject;

		[SerializeField]
		private Text _emptyWarningText;

		[Header("Scroller")]
		[SerializeField]
		private CustomizationInventoryScroller _scroller;

		private ICustomizationInventoryCategoriesView _categoriesView;

		private Guid _selectedCategoryId = Guid.Empty;

		private Guid _selectedItemTypeId = Guid.Empty;

		private bool _isVisible;

		private bool _isAnimating;

		private PlayerCustomizationSlot _selectedCustomizationSlot;
	}
}
