using System;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using Assets.ClientApiObjects.Specializations;
using Assets.Standard_Assets.Scripts.HMM.Video;
using Commons.Swordfish.Battlepass;
using HeavyMetalMachines.VFX;
using ModelViewer;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class ItemTypeShopDetails : GameHubBehaviour
	{
		public void Show(ItemTypeScriptableObject itemType)
		{
			base.gameObject.SetActive(true);
			this._rootGameObject.SetActive(true);
			this._topTabsGroup.SetActive(false);
			ShopItemTypeComponent component = itemType.GetComponent<ShopItemTypeComponent>();
			if (null == component)
			{
				return;
			}
			ItemCategoryScriptableObject itemCategory = itemType.GetItemCategory();
			if (itemCategory.CustomizationSlot == PlayerCustomizationSlot.Spray)
			{
				this._subtitleLabel.text = Language.Get("SHOP_SPRAYS_TITLE", TranslationSheets.Store);
			}
			else
			{
				this._subtitleLabel.text = Language.Get("SHOP_EFFECTS_TITLE", TranslationSheets.Store);
			}
			this._titleLabel.text = Language.Get(component.TitleDraft, TranslationSheets.Items);
			this._rarityLabel.text = Language.Get(itemCategory.TitleDraft, TranslationSheets.Inventory);
			this._descriptionLabel.text = Language.Get(component.DescriptionDraft, TranslationSheets.Items);
			this._itemType = itemType;
			this.UpdateBoughtStatus();
			this.ShowPreview(component.PreviewKind, component.ArtAssetName);
		}

		private void UpdateBoughtStatus()
		{
			bool flag = GameHubBehaviour.Hub.User.Inventory.HasMaxStackItemType(this._itemType.ItemType);
			this._boughtGroup.SetActive(flag);
			this._buyButton.gameObject.SetActive(!flag && (this._itemType.IsSoftPurchasable || this._itemType.IsHardPurchasable));
			this._softBalanceGroup.SetActive(!flag && this._itemType.IsSoftPurchasable);
			this._hardBalanceGroup.SetActive(!flag && this._itemType.IsHardPurchasable);
			if (flag)
			{
				return;
			}
			int num;
			int num2;
			GameHubBehaviour.Hub.Store.GetItemPrice(this._itemType.Id, out num, out num2, false);
			this._softValueLabel.text = num.ToString("0");
			this._hardValueLabel.text = num2.ToString("0");
		}

		public void Hide()
		{
			this.ShowPreview(ItemPreviewKind.None, null);
			this._itemType = null;
			base.gameObject.SetActive(false);
		}

		private void ShowPreview(ItemPreviewKind previewKind, string assetName)
		{
			bool active = previewKind == ItemPreviewKind.Sprite || previewKind == ItemPreviewKind.Lore;
			this._previewDynamicSprite.gameObject.SetActive(active);
			this._previewVideoPlayer.gameObject.SetActive(previewKind == ItemPreviewKind.Video);
			this._previewModelViewerTexture.gameObject.SetActive(previewKind == ItemPreviewKind.Model3D);
			this._loadingView.Hide();
			this._previewVideoPlayer.Stop();
			this._shopDetailsSkinInfo.Disable();
			switch (previewKind)
			{
			case ItemPreviewKind.None:
				break;
			case ItemPreviewKind.Sprite:
			case ItemPreviewKind.Lore:
				this._previewDynamicSprite.SpriteName = assetName;
				break;
			case ItemPreviewKind.Model3D:
				this._previewModelViewer.ModelName = assetName;
				break;
			case ItemPreviewKind.Video:
				this._previewVideoPlayer.VideoClipName = assetName;
				this._loadingView.Show();
				break;
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

		protected void OnEnable()
		{
			this._previewVideoPlayer.OnVideoLoaded += this.PreviewVideoPlayerOnVideoLoaded;
		}

		protected void OnDisable()
		{
			this._previewVideoPlayer.OnVideoLoaded -= this.PreviewVideoPlayerOnVideoLoaded;
		}

		private void PreviewVideoPlayerOnVideoLoaded()
		{
			this._loadingView.Hide();
		}

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
		private HMMUI2DDynamicSprite _previewDynamicSprite;

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

		private ItemTypeScriptableObject _itemType;
	}
}
