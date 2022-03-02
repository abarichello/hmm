using System;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using Assets.ClientApiObjects.Specializations;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Presenting.Unity;
using HeavyMetalMachines.Store.Business;
using HeavyMetalMachines.Store.Business.GetStoreItem;
using HeavyMetalMachines.Store.Business.ObserveStoreItem;
using HeavyMetalMachines.VFX;
using HeavyMetalMachines.Video;
using Hoplon.Input.UiNavigation;
using ModelViewer;
using Pocketverse;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class ItemTypeShopDetails : GameHubBehaviour
	{
		private IUiNavigationGroupHolder UiNavigationGroupHolder
		{
			get
			{
				return this._uiNavigationGroupHolder;
			}
		}

		private void Start()
		{
			ObservableExtensions.Subscribe<Unit>(this.UiNavigationGroupHolder.ObserveInputCancelDown(), delegate(Unit _)
			{
				this._shopGui.Details.Hide();
			});
		}

		public void Show(IItemType itemType)
		{
			this._itemType = (ItemTypeScriptableObject)itemType;
			base.gameObject.SetActive(true);
			this._rootGameObject.SetActive(true);
			this._topTabsGroup.SetActive(false);
			this.UiNavigationGroupHolder.AddGroup();
			ShopItemTypeComponent component = this._itemType.GetComponent<ShopItemTypeComponent>();
			if (null == component)
			{
				return;
			}
			ItemCategoryScriptableObject itemCategory = this._itemType.GetItemCategory();
			this._subtitleLabel.text = itemCategory.LocalizedName;
			this._titleLabel.text = Language.Get(component.TitleDraft, TranslationContext.Items);
			this._rarityLabel.text = itemCategory.LocalizedName;
			this._descriptionLabel.text = Language.Get(component.DescriptionDraft, TranslationContext.Items);
			IObserveStoreItem observeStoreItem = this._storeBusinessFactory.CreateObserveStoreItem();
			IObservable<StoreItem> observable = observeStoreItem.CreateObservable(this._itemType.Id);
			this._storeItemObservation = ObservableExtensions.Subscribe<StoreItem>(observable, delegate(StoreItem changedStoreItem)
			{
				this._softValueLabel.text = changedStoreItem.SoftPrice.ToString("0");
				this._hardValueLabel.text = changedStoreItem.HardPrice.ToString("0");
			});
			this.UpdateBoughtStatus();
			this.ShowPreview(component.PreviewKind, component.ArtAssetName);
		}

		private void UpdateBoughtStatus()
		{
			bool flag = GameHubBehaviour.Hub.User.Inventory.HasItemOfType(this._itemType.ItemType.Id);
			IGetStoreItem getStoreItem = this._storeBusinessFactory.CreateGetStoreItem();
			StoreItem storeItem = getStoreItem.Get(this._itemType.Id);
			this._boughtGroup.SetActive(flag);
			this._buyButton.gameObject.SetActive(!flag && (storeItem.IsSoftPurchasable || storeItem.IsHardPurchasable));
			this._softBalanceGroup.SetActive(!flag && storeItem.IsSoftPurchasable);
			this._hardBalanceGroup.SetActive(!flag && storeItem.IsHardPurchasable);
			if (flag)
			{
				return;
			}
			this._softValueLabel.text = storeItem.SoftPrice.ToString("0");
			this._hardValueLabel.text = storeItem.HardPrice.ToString("0");
		}

		public void Hide()
		{
			if (this._storeItemObservation != null)
			{
				this._storeItemObservation.Dispose();
				this._storeItemObservation = null;
			}
			this.ShowPreview(ItemPreviewKind.None, null);
			this._itemType = null;
			base.gameObject.SetActive(false);
			this.UiNavigationGroupHolder.RemoveGroup();
		}

		private void DisposeLoadPreview()
		{
			if (this._loadDisposable != null)
			{
				this._loadDisposable.Dispose();
				this._loadDisposable = null;
			}
		}

		private void ShowPreview(ItemPreviewKind previewKind, string assetName)
		{
			bool active = previewKind == ItemPreviewKind.Sprite || previewKind == ItemPreviewKind.Lore;
			this._previewDynamicSprite.gameObject.SetActive(active);
			this._previewSmallDynamicSprite.gameObject.SetActive(previewKind == ItemPreviewKind.SmallSprite);
			this._previewVideoPlayer.gameObject.SetActive(previewKind == ItemPreviewKind.Video);
			this._previewModelViewerTexture.gameObject.SetActive(previewKind == ItemPreviewKind.Model3D);
			this._previewAnimatedSprite.gameObject.SetActive(previewKind == ItemPreviewKind.SpriteSheet);
			this._loadingView.Hide();
			this._previewVideoPlayer.Stop();
			this._shopDetailsSkinInfo.Disable();
			this.DisposeLoadPreview();
			if (previewKind != ItemPreviewKind.None)
			{
				this._loadDisposable = ObservableExtensions.Subscribe<Unit>(Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
				{
					this._loadingView.Show();
				}), this.LoadPreview(previewKind, assetName)), delegate(Unit _)
				{
					this._loadingView.Hide();
				}), this._animationIn.Play()));
			}
		}

		private IObservable<Unit> LoadPreview(ItemPreviewKind previewKind, string assetName)
		{
			switch (previewKind)
			{
			case ItemPreviewKind.None:
				throw new InvalidOperationException("Shouldn't try to load ItemPreviewKind None");
			case ItemPreviewKind.Sprite:
			case ItemPreviewKind.Lore:
				return Observable.Defer<Unit>(delegate()
				{
					this._previewDynamicSprite.SpriteName = assetName;
					return Observable.ReturnUnit();
				});
			case ItemPreviewKind.Model3D:
				return this._previewModelViewer.Load(assetName);
			case ItemPreviewKind.Video:
				return this._previewVideoPlayer.LoadVideo(assetName);
			case ItemPreviewKind.SpriteSheet:
				return Observable.Defer<Unit>(delegate()
				{
					this._previewAnimatedSprite.TryToLoadAsset(assetName);
					this._previewAnimatedSprite.StartAnimation();
					return Observable.ReturnUnit();
				});
			case ItemPreviewKind.SmallSprite:
				return Observable.Defer<Unit>(delegate()
				{
					this._previewSmallDynamicSprite.SpriteName = assetName;
					return Observable.ReturnUnit();
				});
			default:
				throw new ArgumentException(string.Format("Unknown ItemPreviewKind: {0}", previewKind));
			}
		}

		public void BuyItemType()
		{
			GameHubBehaviour.Hub.GuiScripts.SharedPreGameWindow.ItemBuyWindow.ShowBuyWindow(this._itemType, new Action(this.OnBoughtFinished), null, new Action(this.OnGoToShopCash), 1, false);
		}

		private void OnBoughtFinished()
		{
			this.UpdateBoughtStatus();
			this._shopGui.OnCustomizationItemBought();
		}

		private void OnGoToShopCash()
		{
			GameHubBehaviour.Hub.GuiScripts.SharedPreGameWindow.ItemBuyWindow.CloseWindow();
			this.Hide();
			GameHubBehaviour.Hub.State.Current.GetStateGuiController<MainMenuGui>().OnCashTopButtonClick();
		}

		protected void OnDisable()
		{
			this.DisposeLoadPreview();
		}

		[SerializeField]
		private UnityAnimation _animationIn;

		[SerializeField]
		private ShopGUI _shopGui;

		[SerializeField]
		private GameObject _rootGameObject;

		[SerializeField]
		private UILabel _titleLabel;

		[SerializeField]
		private UILabel _rarityLabel;

		[SerializeField]
		private UILabel _descriptionLabel;

		[SerializeField]
		private GameObject _boughtGroup;

		[SerializeField]
		private GameObject _softBalanceGroup;

		[SerializeField]
		private UILabel _softValueLabel;

		[SerializeField]
		private GameObject _hardBalanceGroup;

		[SerializeField]
		private UILabel _hardValueLabel;

		[SerializeField]
		private UIButton _buyButton;

		[SerializeField]
		private HMMUI2DDynamicSprite _previewSmallDynamicSprite;

		[SerializeField]
		private HMMUI2DDynamicSprite _previewDynamicSprite;

		[SerializeField]
		private AnimatedNguiTexture _previewAnimatedSprite;

		[SerializeField]
		private ModelViewerNGUI _previewModelViewer;

		[SerializeField]
		private UITexture _previewModelViewerTexture;

		[SerializeField]
		private VideoPreviewNGUI _previewVideoPlayer;

		[SerializeField]
		private ShopDetailsSkinInfo _shopDetailsSkinInfo;

		[SerializeField]
		private NguiLoadingView _loadingView;

		[SerializeField]
		private GameObject _topTabsGroup;

		[SerializeField]
		private UILabel _subtitleLabel;

		[Header("[Ui Navigation]")]
		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;

		[InjectOnClient]
		private IStoreBusinessFactory _storeBusinessFactory;

		private ItemTypeScriptableObject _itemType;

		private IDisposable _storeItemObservation;

		private IDisposable _loadDisposable;
	}
}
