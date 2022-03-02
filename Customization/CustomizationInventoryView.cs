using System;
using System.Collections;
using System.Collections.Generic;
using HeavyMetalMachines.Customization.Infra;
using HeavyMetalMachines.Customization.Presenter;
using HeavyMetalMachines.DataTransferObjects.Util;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Input;
using HeavyMetalMachines.Input.ControllerInput;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using HeavyMetalMachines.RadialMenu.View;
using HeavyMetalMachines.UnityUI;
using HeavyMetalMachines.Utils;
using Hoplon.Input;
using Pocketverse;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Customization
{
	public class CustomizationInventoryView : MonoBehaviour, ICustomizationInventoryView
	{
		public ICustomizationInventoryRadialPresenter RadialMenu
		{
			get
			{
				return this._radialPresenter;
			}
		}

		public UnityUIInventoryArtPreview ArtPreview
		{
			get
			{
				return this._artPreview;
			}
		}

		private void Awake()
		{
			this._setupObservation = new Subject<Unit>();
		}

		public IGetCustomizationChange GetCustomizationInventory()
		{
			return this._inventoryComponent;
		}

		private void OnEnable()
		{
			GUIUtils.ResetAnimation(this._mainWindowAnimation);
			this._inventoryComponent.RegisterView(this);
			this.InitializeShortcupFeedback();
			this._radialPresenter = new CustomizationInventoryRadialPresenter(this._radialPreview, this._customizationService, this._inventoryComponent, this._inputActiveDeviceChangeNotifier, this._inputGetActiveDevicePoller, this._inputTranslation);
			this._radialPresenter.Initialize();
			this._equipItemFailedAnimation.Rewind();
			this._inventoryComponent.OnSkinTabHasNewItemsStateChanged += this.InventoryComponentOnSkinTabHasNewItemsStateChanged;
		}

		private void OnDisable()
		{
			this._inventoryComponent.OnSkinTabHasNewItemsStateChanged -= this.InventoryComponentOnSkinTabHasNewItemsStateChanged;
			this._radialPresenter.Dispose();
		}

		protected void OnDestroy()
		{
			this._inventoryComponent.CustomizationWindowUnloaded();
		}

		private void InitializeShortcupFeedback()
		{
			this._shortcutFeedbackGroup.SetActive(false);
			ISprite sprite;
			string text;
			this._inputTranslation.TryToGetInputActionKeyboardMouseAssetOrFallbackToTranslation(16, ref sprite, ref text);
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
			this.ItemSelectedSetup(categoryId, itemTypeId);
		}

		private void ItemSelectedSetup(Guid categoryId, Guid itemTypeId)
		{
			if (this._selectedItemTypeId == itemTypeId)
			{
				return;
			}
			CustomizationInventoryCellItemData item = this._inventoryComponent.GetItem(itemTypeId);
			this.DeselectCurrentItem();
			if (!this.IsSelectable(categoryId))
			{
				this.RefreshEquipButton(item);
				this.ShowPreview(categoryId);
				return;
			}
			this._selectedItemTypeId = itemTypeId;
			item.IsSelected = true;
			this._inventoryComponent.MarkItemAsSeen(item);
			UnityUIInventoryArtPreview.ArtPreviewLoreData loreData = new UnityUIInventoryArtPreview.ArtPreviewLoreData
			{
				IsLocked = false,
				DescriptionText = item.LoreDescriptionDraft,
				SubtitleText = item.LoreSubtitleDraft,
				TitleText = item.LoreTitleDraft
			};
			UnityUIInventoryArtPreview.ArtPreviewData artPreviewData = new UnityUIInventoryArtPreview.ArtPreviewData
			{
				RewardAssetKind = item.PreviewKind,
				RewardAssetName = item.PreviewName,
				TitleText = item.ItemName,
				DescriptionText = item.ItemDescription,
				LoreData = loreData,
				ArtPreviewBackGroundAssetName = item.ArtPreviewBackGroundAssetName,
				SkinPrefabComponent = item.SkinPrefabComponent
			};
			this.RefreshEquipButton(item);
			this._artPreview.SetupAsset(artPreviewData, true);
			this.ShowPreview(categoryId);
		}

		private void DisposePreview()
		{
			if (this._previewDisposable != null)
			{
				this._previewDisposable.Dispose();
				this._previewDisposable = null;
			}
		}

		private void ShowPreview(Guid categoryId)
		{
			this.DisposePreview();
			if (categoryId != InventoryMapper.EmoteCategoryGuid)
			{
				this._previewDisposable = ObservableExtensions.Subscribe<Unit>(Observable.Merge<Unit>(new IObservable<Unit>[]
				{
					this._artPreview.Show(),
					this._radialPresenter.Hide()
				}));
			}
			else
			{
				this._previewDisposable = ObservableExtensions.Subscribe<Unit>(Observable.Merge<Unit>(new IObservable<Unit>[]
				{
					this._artPreview.Hide(),
					this._radialPresenter.Show()
				}));
			}
		}

		private void HidePreview(Guid categoryId)
		{
			this.DisposePreview();
			if (categoryId != InventoryMapper.EmoteCategoryGuid)
			{
				this._previewDisposable = ObservableExtensions.Subscribe<Unit>(Observable.Merge<Unit>(new IObservable<Unit>[]
				{
					this._artPreview.Hide(),
					this._radialPresenter.Hide()
				}));
			}
			else
			{
				this._previewDisposable = ObservableExtensions.Subscribe<Unit>(Observable.Merge<Unit>(new IObservable<Unit>[]
				{
					this._artPreview.Hide(),
					this._radialPresenter.Show()
				}));
			}
		}

		private void RefreshEquipButton(CustomizationInventoryCellItemData itemData)
		{
			this._equipItemButton.gameObject.SetActive(this.IsEquipableByButton(itemData));
			this._equipItemButton.interactable = this._inventoryComponent.IsInteractable(itemData);
			if (!this._inventoryComponent.GetIsItemEquiped(itemData))
			{
				this._equippedLabel.text = Language.Get("BATTLEPASS_INVENTORY_EQUIP", TranslationContext.Inventory);
			}
			else if (this._inventoryComponent.IsUnequipable(itemData))
			{
				this._equippedLabel.text = Language.Get("BATTLEPASS_INVENTORY_UNEQUIP", TranslationContext.Inventory);
			}
			else
			{
				this._equippedLabel.text = Language.Get("BATTLEPASS_INVENTORY_EQUIPPED", TranslationContext.Inventory);
			}
		}

		private bool IsEquipableByButton(CustomizationInventoryCellItemData itemData)
		{
			return this._inventoryComponent.IsEquipable(itemData) && itemData.ItemCategoryId != InventoryMapper.EmoteCategoryGuid;
		}

		private bool IsSelectable(Guid itemCategoryId)
		{
			return itemCategoryId != InventoryMapper.EmoteCategoryGuid;
		}

		private void DeselectCurrentItem()
		{
			if (this._selectedItemTypeId == Guid.Empty)
			{
				return;
			}
			CustomizationInventoryCellItemData item = this._inventoryComponent.GetItem(this._selectedItemTypeId);
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

		[Obsolete("Use MainMenuTree.InventoryNode")]
		public void SetVisibility(bool isVisible, bool imediate)
		{
			CustomizationInventoryView.Log.WarnStackTrace("Obsolete SetVisibility. Use MainMenuTree.InventoryNode");
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
			string titleText = Language.Get("INVENTORY_TITLE_NAME", TranslationContext.Inventory);
			this._title.Setup(titleText, HmmUiText.TextStyles.UpperCase, string.Empty, HmmUiText.TextStyles.Default, string.Empty, HmmUiText.TextStyles.Default, false);
			this._scroller.Setup(data, new CustomizationInventoryScroller.OnCustomizationItemSelectedDelegate(this.OnCustomizationItemSelected));
			this.EmptyWarningSetup(data);
			int index = 0;
			if (data.Items.Count > 0)
			{
				Guid itemTypeId = data.Items[0].ItemTypeId;
				for (int i = 0; i < data.Items.Count; i++)
				{
					CustomizationInventoryCellItemData customizationInventoryCellItemData = data.Items[i];
					if (this._inventoryComponent.GetIsItemEquiped(customizationInventoryCellItemData))
					{
						index = i;
						itemTypeId = customizationInventoryCellItemData.ItemTypeId;
						break;
					}
				}
				this.ItemSelectedSetup(data.CategoryId, itemTypeId);
			}
			else
			{
				this._equipItemButton.gameObject.SetActive(false);
				this.HidePreview(data.CategoryId);
			}
			this._scroller.JumpToCellIndex(index);
			this._scroller.Refresh();
		}

		private void EmptyWarningSetup(CustomizationInventoryCategoryData data)
		{
			if (data.Items.Count > 0)
			{
				this._emptyWarningGameObject.SetActive(false);
				return;
			}
			bool flag = data.CategoryId == InventoryMapper.SkinsCategoryGuid || data.IsLore;
			this._emptyWarningGameObject.SetActive(flag);
			if (flag)
			{
				string key = (!(data.CategoryId == InventoryMapper.SkinsCategoryGuid)) ? "EMPTY_LORE_DRAFT" : "EMPTY_SKINS_DRAFT";
				this._emptyWarningText.text = Language.Get(key, TranslationContext.Inventory);
			}
		}

		private void ConfigureShortcutFeedback()
		{
			bool active = this._selectedCategoryId == InventoryMapper.SprayCategoryGuid || this._selectedCategoryId == InventoryMapper.EmoteCategoryGuid;
			this._shortcutFeedbackGroup.SetActive(active);
			string empty = string.Empty;
			string key = string.Empty;
			string text = this._selectedCategoryId.ToString();
			if (text != null)
			{
				ControllerInputActions controllerInputActions;
				if (!(text == "100e5ce6-f2d2-1894-18c2-37c9b507a1a6"))
				{
					if (!(text == "0f3b8ebe-73e6-4ea6-90f0-3dd0da96771d"))
					{
						return;
					}
					controllerInputActions = 16;
					key = "INVENTORY_SPRAY_USE_HINT";
				}
				else
				{
					controllerInputActions = 28;
					key = "INVENTORY_EMOTE_USE_HINT";
				}
				ISprite sprite;
				this._inputTranslation.TryToGetInputActionKeyboardMouseAssetOrFallbackToTranslation(controllerInputActions, ref sprite, ref empty);
				this._shortcutFeedbackText.text = Language.GetFormatted(key, TranslationContext.Inventory, new object[]
				{
					empty
				});
				return;
			}
		}

		[UnityUiComponentCall]
		[Obsolete("Leave Navigation to presenter")]
		public void OnBackButtonClick()
		{
			CustomizationInventoryView.Log.WarnStackTrace("Obsolete OnBackButtonClick. Leave Navigation to presenter");
		}

		public void OnEquipButtonClick()
		{
			this._equipItemButton.interactable = false;
			CustomizationInventoryCellItemData item = this._inventoryComponent.GetItem(this._selectedItemTypeId);
			if (!this._inventoryComponent.GetIsItemEquiped(item))
			{
				this._inventoryComponent.EquipItem(this._selectedItemTypeId);
			}
			else if (this._inventoryComponent.IsUnequipable(item))
			{
				this._inventoryComponent.UnequipItem(this._selectedItemTypeId);
			}
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
			this.RefreshItemData();
		}

		private void RefreshItemData()
		{
			CustomizationInventoryCategoryData categoryData = this._inventoryComponent.GetCategoryData(this._selectedCategoryId);
			List<CustomizationInventoryCellItemData> items = categoryData.Items;
			for (int i = 0; i < items.Count; i++)
			{
				CustomizationInventoryCellItemData customizationInventoryCellItemData = items[i];
				if (customizationInventoryCellItemData.ItemTypeId == this._selectedItemTypeId)
				{
					this.RefreshEquipButton(customizationInventoryCellItemData);
					break;
				}
			}
		}

		private void InventoryComponentOnSkinTabHasNewItemsStateChanged()
		{
			this._scroller.Refresh();
		}

		public IObservable<Unit> AnimateShow()
		{
			UnityAnimation unityAnimation = new UnityAnimation(this._mainWindowAnimation, "InventoryIn");
			return Observable.Do<Unit>(Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				this.BeforeAnimationShow();
			}), unityAnimation.Play()), delegate(Unit _)
			{
				this.AfterAnimationShow();
			});
		}

		public IObservable<Unit> AnimateHide()
		{
			UnityAnimation unityAnimation = new UnityAnimation(this._mainWindowAnimation, "InventoryOut");
			return Observable.Do<Unit>(Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				this.BeforeAnimationHide();
			}), unityAnimation.Play()), delegate(Unit _)
			{
				this.AfterAnimationHide();
			});
		}

		public IObservable<Unit> AnimateCategoryShow()
		{
			UnityAnimation categoryInAnimation = new UnityAnimation(this._categoryAnimation, "TabAnimationIn");
			return Observable.Defer<Unit>(delegate()
			{
				this.BeforeCategoryAnimationShow();
				return categoryInAnimation.Play();
			});
		}

		public void SetCategory(Guid categoryGuid)
		{
			this._selectedCategoryId = categoryGuid;
		}

		public void BeforeCategoryAnimationShow()
		{
			this.DeselectCurrentItem();
			this._inventoryComponent.GetCategoryItems(this._selectedCategoryId);
		}

		public IObservable<Unit> AnimateCategoryHide()
		{
			UnityAnimation unityAnimation = new UnityAnimation(this._categoryAnimation, "TabAnimationOut");
			return unityAnimation.Play();
		}

		public void BeforeAnimationShow()
		{
			this._isAnimating = true;
			this._isVisible = true;
			this._mainWindowCanvas.enabled = true;
			this._mainWindowCanvasGroup.interactable = true;
		}

		public void AfterAnimationShow()
		{
			this._isAnimating = false;
		}

		public void BeforeAnimationHide()
		{
			this._isAnimating = true;
		}

		public void AfterAnimationHide()
		{
			this._mainWindowCanvas.gameObject.SetActive(false);
			this._mainWindowCanvas.enabled = false;
			this._mainWindowCanvasGroup.interactable = false;
			this._isVisible = false;
			this._isAnimating = false;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(CustomizationInventoryView));

		private const string CATEGORY_IN_ANIMATION_NAME = "TabAnimationIn";

		private const string CATEGORY_OUT_ANIMATION_NAME = "TabAnimationOut";

		private ICustomizationInventoryRadialPresenter _radialPresenter;

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
		private Text _equippedLabel;

		[SerializeField]
		private Animation _equipItemFailedAnimation;

		[SerializeField]
		private UnityUiTitleInfo _title;

		[Header("Preview")]
		[SerializeField]
		private UnityUIInventoryArtPreview _artPreview;

		[SerializeField]
		private UnityUiRadialCustomizationView _radialPreview;

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

		[InjectOnClient]
		private IInputTranslation _inputTranslation;

		[InjectOnClient]
		private ICustomizationService _customizationService;

		[InjectOnClient]
		private IInputActiveDeviceChangeNotifier _inputActiveDeviceChangeNotifier;

		[InjectOnClient]
		private IInputGetActiveDevicePoller _inputGetActiveDevicePoller;

		private Subject<Unit> _setupObservation;

		private IDisposable _previewDisposable;

		private Guid _previousPreviewCategory;
	}
}
