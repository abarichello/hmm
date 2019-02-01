using System;
using System.Collections;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using ClientAPI;
using Commons.Swordfish.Exceptions;
using Commons.Swordfish.Util;
using HeavyMetalMachines.Swordfish.Player;
using HeavyMetalMachines.VFX;
using Pocketverse;
using Swordfish.Common.exceptions;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class ItemBuyWindow : GameHubBehaviour
	{
		private void OnEnable()
		{
			GameHubBehaviour.Hub.Store.OnTransactionReceived += this.OnTransactionReceivedFromSW;
			GameHubBehaviour.Hub.State.ListenToStateChanged += this.LeavingMainmenu;
		}

		private void OnDisable()
		{
			if (GameHubBehaviour.Hub != null)
			{
				if (GameHubBehaviour.Hub.Store != null)
				{
					GameHubBehaviour.Hub.Store.OnTransactionReceived -= this.OnTransactionReceivedFromSW;
				}
				GameHubBehaviour.Hub.State.ListenToStateChanged -= this.LeavingMainmenu;
			}
		}

		private void LeavingMainmenu(GameState changedState)
		{
			this._onCloseWindow = null;
			this.SetWaitingWindow(false);
			this.CloseWindow();
		}

		private void OnBuyClick(int type)
		{
			this.buyType = type;
			int num = 0;
			int num2 = 0;
			Guid id = this._itemType.Id;
			GameHubBehaviour.Hub.Store.GetItemPrice(id, out num, out num2, false);
			num *= this._qnty;
			num2 *= this._qnty;
			StoreConfirmationWindow storeConfirmationWindow;
			SingletonMonoBehaviour<PanelController>.Instance.ShowModalWindow<StoreConfirmationWindow>(out storeConfirmationWindow);
			if (type == 1)
			{
				if ((long)num2 > GameHubBehaviour.Hub.Store.HardCurrency)
				{
					storeConfirmationWindow.ShowConfirmationMessage(Language.Get("No_Funds_HardCoin", TranslationSheets.Store), Language.Get("SIM", TranslationSheets.MainMenuGui), Language.Get("CANCELAR", TranslationSheets.MainMenuGui), this._onGoToShopCash);
					return;
				}
			}
			else if (num > GameHubBehaviour.Hub.Store.SoftCurrency)
			{
				storeConfirmationWindow.ShowOkConfirmationMessage(Language.Get("No_Funds_SoftCoin", "Store"), Language.Get("Ok", "Store"), this._onGoToShopCash);
				return;
			}
			storeConfirmationWindow.ShowBoughtMessage(Language.Get("Bought_Confirmation", "Store"), Language.Get("Bought_Confirmation_OK", "Store"), Language.Get("Bought_Confirmation_Cancel", "Store"), new System.Action(this.OnDoBuy));
		}

		private void OnDoBuy()
		{
			if (this._waitingForTransaction)
			{
				return;
			}
			this.SetButtonsActive(false);
			this.SetWaitingWindow(true);
			this._waitingForTransaction = true;
			if (this.buyType == 1)
			{
				if (this._itemType != null)
				{
					GameHubBehaviour.Hub.Store.BuyItemUsingHardCoins(this._itemType, this._qnty, new SwordfishClientApi.ParameterizedCallback<long>(this.OnBoughtWithHardCoinsSuccessfull), new SwordfishClientApi.ErrorCallback(this.OnBoughtFailure));
				}
				else
				{
					ItemBuyWindow.Log.WarnFormat("Null _itemType on BuyItemUsingHardCoins", new object[0]);
				}
			}
			else if (this._itemType != null)
			{
				GameHubBehaviour.Hub.Store.BuyItemTypeUsingSoftCurrency(this._itemType, new SwordfishClientApi.ParameterizedCallback<long>(this.OnBoughtSuccessfull), new SwordfishClientApi.ErrorCallback(this.OnBoughtFailure));
			}
			else
			{
				ItemBuyWindow.Log.WarnFormat("Null _itemType on BuyItemTypeUsingSoftCurrency", new object[0]);
			}
		}

		private void SetButtonsActive(bool activate)
		{
			this.hardCoinButton.SetActive(activate);
			this.softCoinButton.SetActive(activate);
			this.cancelButtonCollider.enabled = activate;
		}

		private void OnBoughtWithHardCoinsSuccessfull(object state, long transactionId)
		{
			base.StartCoroutine(this.WaitToClosePurchasedSkinWindowAsync());
		}

		private void OnTransactionReceivedFromSW(long itemId)
		{
			if (!this._waitingForTransaction)
			{
				ItemBuyWindow.Log.Error("Transaction callback received was NOT expected. Ignoring it.");
				return;
			}
			this.DoBoughtEnd(true, itemId);
		}

		private void OnBoughtSuccessfull(object state, long itemId)
		{
			this.DoBoughtEnd(true, itemId);
			base.StartCoroutine(this.WaitToClosePurchasedSkinWindowAsync());
		}

		private void DoBoughtEnd(bool success, long itemId)
		{
			this._waitingForTransaction = false;
			if (success)
			{
				if (itemId == -1L)
				{
					ItemBuyWindow.Log.Error("CANNOT HAVE SUCCESS AND HAVE ITEMID = -1 !!!!");
					return;
				}
				SwordfishStore.BalanceType balanceType = (this.buyType != 1) ? SwordfishStore.BalanceType.Soft : SwordfishStore.BalanceType.Hard;
				GameHubBehaviour.Hub.Store.GetBalance(null, balanceType);
				GameHubBehaviour.Hub.User.Inventory.ReloadItem(itemId, delegate
				{
					this.SetWaitingWindow(false);
					this.ConfigureFeedBack(delegate
					{
						base.StartCoroutine(this.AnimateFeedBack());
					});
				});
			}
			else
			{
				this.SetWaitingWindow(false);
				this.ShowErrorWindow();
			}
		}

		private IEnumerator AnimateFeedBack()
		{
			yield return UnityUtils.WaitForOneSecond;
			this.CloseWindow();
			yield break;
		}

		private void ConfigureFeedBack(System.Action whenDone = null)
		{
			this.hardCoinButton.SetActive(false);
			this.softCoinButton.SetActive(false);
			int num = (int)this._itemType.ItemTypePrices[0].Price;
			int referenceHardPrice = this._itemType.ReferenceHardPrice;
			string name = this._itemType.Name;
			string baseSKU = this._itemType.BaseSKU;
			string text = this._itemType.ItemCategoryId.ToString();
			string text2 = (this.buyType != 1) ? "SC" : "HC";
			int num2 = (this.buyType != 1) ? num : referenceHardPrice;
			UI2DSprite ui2DSprite = (this.buyType != 1) ? this.softCoinStamp : this.hardCoinStamp;
			GameObject go = (this.buyType != 1) ? this.softCoinGroup : this.hardCoinGroup;
			ui2DSprite.gameObject.SetActive(true);
			ui2DSprite.alpha = 0f;
			ui2DSprite.transform.localScale = new Vector3(2f, 2f, 1f);
			TweenAlpha.Begin(go, 0.25f, 0.5f);
			TweenScale.Begin(ui2DSprite.gameObject, 0.5f, Vector3.one);
			TweenAlpha tweenAlpha = TweenAlpha.Begin(ui2DSprite.gameObject, 0.5f, 1f);
			tweenAlpha.AddOnFinished(delegate()
			{
				if (this._onBoughtFinished != null)
				{
					this._onBoughtFinished();
					this._onBoughtFinished = null;
				}
				if (whenDone != null)
				{
					whenDone();
				}
			});
			string text3 = "U";
			string text4 = "R";
			string text5 = string.Format("{0}-{1}-{2}-{3}", new object[]
			{
				text2,
				text3,
				text4,
				baseSKU
			});
			GameHubBehaviour.Hub.Swordfish.Log.BILogClientMsg(ClientBITags.BuyItemComplete, string.Format("Sku={0} ItemName={1} ItemCategory={2} ItemPrice={3}", new object[]
			{
				text5,
				name,
				text,
				num2
			}), true);
		}

		private void OnBoughtFailure(object state, Exception exception)
		{
			ItemBuyWindow.Log.Error("OnBoughtFailure " + exception.ToString());
			if (exception is SwordfishException)
			{
				SwordfishExceptionJson swordfishExceptionJson = (SwordfishExceptionJson)((JsonSerializeable<T>)exception.Message);
				if (swordfishExceptionJson != null && swordfishExceptionJson.ExceptionType == SwordfishExceptionType.DuplicatedItem)
				{
					ItemBuyWindow.Log.WarnFormat("SwordfishException: DuplicatedItem. The software will assume it was just bought", new object[0]);
					Guid guid = (Guid)state;
					this._waitingForTransaction = false;
					ItemTypeScriptableObject itemTypeScriptableObject = GameHubBehaviour.Hub.InventoryColletion.AllItemTypes[guid];
					InventoryBag.InventoryKind inventoryKind = InventoryMapper.CategoriesToInventoryKind[itemTypeScriptableObject.ItemCategoryId];
					InventoryAdapter inventoryAdapterByKind = GameHubBehaviour.Hub.User.Inventory.GetInventoryAdapterByKind(inventoryKind);
					GameHubBehaviour.Hub.User.Inventory.ReloadItemByItemTypeId(guid, inventoryAdapterByKind.Inventory.Id, delegate(long itemId)
					{
						this.DoBoughtEnd(itemId >= 0L, itemId);
						base.StartCoroutine(this.WaitToClosePurchasedSkinWindowAsync());
					});
					return;
				}
			}
			this.DoBoughtEnd(false, -1L);
			this.CloseWindow();
		}

		private void ShowErrorWindow()
		{
			Guid confirmWindowGuid = Guid.NewGuid();
			ConfirmWindowProperties properties = new ConfirmWindowProperties
			{
				Guid = confirmWindowGuid,
				QuestionText = Language.Get("Buy_Item_Error", "Store"),
				OkButtonText = Language.Get("Ok", "Store"),
				OnOk = delegate()
				{
					this.SetButtonsActive(true);
					GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.HideConfirmWindow(confirmWindowGuid);
				}
			};
			GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.OpenConfirmWindow(properties);
		}

		public void ShowBuyWindow(ItemTypeScriptableObject itemType, System.Action onBoughtFinished, System.Action onBuyWindowClosed, System.Action onGoToShopCash, int qnty, bool portrait)
		{
			this._description.SetActive(portrait);
			this._description2.SetActive(!portrait);
			this._itemType = itemType;
			this._qnty = qnty;
			base.gameObject.SetActive(true);
			this.WindowGroupGameObject.SetActive(true);
			this.SetButtonsActive(true);
			bool isSoftPurchasable = itemType.IsSoftPurchasable;
			bool isHardPurchasable = itemType.IsHardPurchasable;
			string name = itemType.Name;
			string text = itemType.Id.ToString();
			bool isCharacter = false;
			string arg = itemType.ItemCategoryId.ToString();
			ShopItemTypeComponent component = itemType.GetComponent<ShopItemTypeComponent>();
			if (null == component)
			{
				this.SetupBuyWindow(name, text, isHardPurchasable, isSoftPurchasable, isCharacter, qnty);
			}
			else
			{
				this.SetupBuyWindow(itemType, component, qnty);
			}
			ItemBuyWindow.Log.InfoFormat("Buying item={0} id={1}", new object[]
			{
				name,
				text
			});
			string msg = string.Format("ItemType={0} Category={1}", text, arg);
			GameHubBehaviour.Hub.Swordfish.Log.BILogClientMsg(ClientBITags.OpenBuyItemWindow, msg, true);
			this._onCloseWindow = onBuyWindowClosed;
			this._onBoughtFinished = onBoughtFinished;
			this._onGoToShopCash = onGoToShopCash;
		}

		private void SetupBuyWindow(string itemName, string id, bool isHardPurchase, bool isSoftPurchase, bool isCharacter, int quantity)
		{
			this.SetupBuyWindowImages(isCharacter, itemName);
			this.SetupBuyWindowTranslation(isCharacter, itemName, quantity);
			if (isSoftPurchase)
			{
				this.softCoinGroup.SetActive(true);
				this.softCoinButton.SetActive(true);
				UICamera.hoveredObject = this.softCoinButton;
				TweenAlpha.Begin(this.softCoinGroup, 0.5f, 1f);
			}
			else
			{
				this.softCoinGroup.SetActive(false);
			}
			if (isHardPurchase)
			{
				this.hardCoinGroup.SetActive(true);
				this.hardCoinButton.SetActive(true);
				UICamera.hoveredObject = this.hardCoinButton;
				TweenAlpha.Begin(this.hardCoinGroup, 0.5f, 1f);
			}
			else
			{
				this.hardCoinGroup.SetActive(false);
			}
			this.hardCoinStamp.gameObject.SetActive(false);
			this.softCoinStamp.gameObject.SetActive(false);
			this.SetupBuyWindowPriecesLabel(id, quantity);
			this.SetupBuyWindowButtons();
		}

		private void SetupBuyWindow(ItemTypeScriptableObject itemType, ShopItemTypeComponent shopComponent, int quantity)
		{
			this.SetupBuyWindowImages(shopComponent);
			this.SetupBuyWindowTranslation(shopComponent, quantity);
			bool isSoftPurchasable = itemType.IsSoftPurchasable;
			this.softCoinGroup.SetActive(isSoftPurchasable);
			if (isSoftPurchasable)
			{
				this.softCoinButton.SetActive(true);
				UICamera.hoveredObject = this.softCoinButton;
				TweenAlpha.Begin(this.softCoinGroup, 0.5f, 1f);
			}
			bool isHardPurchasable = itemType.IsHardPurchasable;
			this.hardCoinGroup.SetActive(isHardPurchasable);
			if (isHardPurchasable)
			{
				this.hardCoinButton.SetActive(true);
				UICamera.hoveredObject = this.hardCoinButton;
				TweenAlpha.Begin(this.hardCoinGroup, 0.5f, 1f);
			}
			this.hardCoinStamp.gameObject.SetActive(false);
			this.softCoinStamp.gameObject.SetActive(false);
			this.SetupBuyWindowPriecesLabel(itemType.Id.ToString(), quantity);
			this.SetupBuyWindowButtons();
		}

		private void SetupBuyWindowPriecesLabel(string itemID, int quantity)
		{
			int num = 0;
			int num2 = 0;
			GameHubBehaviour.Hub.Store.GetItemPrice(new Guid(itemID), out num, out num2, false);
			num *= quantity;
			num2 *= quantity;
			this.softCoinLabel.text = num.ToString();
			this.hardCoinLabel.text = num2.ToString();
			this.playerSoftBalanceLabel.text = GameHubBehaviour.Hub.Store.SoftCurrency.ToString();
			this.playerHardBalanceLabel.text = GameHubBehaviour.Hub.Store.HardCurrency.ToString();
		}

		private void SetupBuyWindowImages(bool isCharacter, string itemName)
		{
			if (isCharacter)
			{
				((HMMUI2DDynamicTexture)this.icon).TextureName = itemName + "_skin_00";
				((HMMUI2DDynamicTexture)this.icon2).TextureName = itemName + "_skin_00";
			}
			else
			{
				((HMMUI2DDynamicTexture)this.icon).TextureName = itemName;
				((HMMUI2DDynamicTexture)this.icon2).TextureName = itemName;
			}
		}

		private void SetupBuyWindowImages(ShopItemTypeComponent shopComponent)
		{
			((HMMUI2DDynamicTexture)this.icon).TextureName = shopComponent.IconAssetName;
			((HMMUI2DDynamicTexture)this.icon2).TextureName = shopComponent.IconAssetName;
		}

		private void SetupBuyWindowTranslation(bool isCharacter, string itemName, int quantity)
		{
			string text = string.Format("{0}_name", itemName);
			string text2 = string.Format("{0}_description", itemName);
			TranslationSheets translationSheet;
			if (isCharacter)
			{
				translationSheet = TranslationSheets.CharactersBaseInfo;
				text = text.ToUpper();
				text2 = text2.ToUpper();
			}
			else
			{
				translationSheet = TranslationSheets.Items;
			}
			string format = Language.Get(text, translationSheet);
			string format2 = Language.Get(text2, translationSheet);
			string text3 = string.Format(format, quantity);
			string text4 = string.Format(format2, quantity);
			this.previewItemName.text = text3;
			this.previewItemName2.text = text3;
			this.previewItemDescription.text = text4;
			this.previewItemDescription2.text = text4;
		}

		private void SetupBuyWindowTranslation(ShopItemTypeComponent shopComponent, int quantity)
		{
			string format = Language.Get(shopComponent.TitleDraft, TranslationSheets.Items);
			string format2 = Language.Get(shopComponent.DescriptionDraft, TranslationSheets.Items);
			string text = string.Format(format, quantity);
			string text2 = string.Format(format2, quantity);
			this.previewItemName.text = text;
			this.previewItemName2.text = text;
			this.previewItemDescription.text = text2;
			this.previewItemDescription2.text = text2;
		}

		private void SetupBuyWindowButtons()
		{
			if (this.softCoinGroup.activeSelf)
			{
				UICamera.controllerNavigationObject = this.softCoinButton;
				if (this.hardCoinGroup.activeSelf)
				{
					this.softCoinButton.GetComponent<UIKeyNavigation>().onRight = this.hardCoinButton;
				}
				else
				{
					this.softCoinButton.GetComponent<UIKeyNavigation>().onRight = null;
				}
			}
			if (this.hardCoinGroup.activeSelf)
			{
				UICamera.controllerNavigationObject = this.hardCoinButton;
				if (this.softCoinGroup.activeSelf)
				{
					this.hardCoinButton.GetComponent<UIKeyNavigation>().onLeft = this.softCoinButton;
				}
				else
				{
					this.hardCoinButton.GetComponent<UIKeyNavigation>().onLeft = null;
				}
			}
			this._uiGrid.Reposition();
		}

		public void TryCloseWindow()
		{
			if (this._onCloseWindow != null)
			{
				this.CloseWindow();
			}
		}

		public void CloseWindow()
		{
			if (base.gameObject == null)
			{
				return;
			}
			this.SetButtonsActive(true);
			base.gameObject.SetActive(false);
			if (this._onCloseWindow != null)
			{
				this._onCloseWindow();
				this._onCloseWindow = null;
			}
		}

		private IEnumerator WaitToClosePurchasedSkinWindowAsync()
		{
			yield return new WaitForSeconds(this.PurchasedSkinDelayBeforeCloseInSec);
			this.SetWaitingWindow(false);
			this.CloseWindow();
			yield break;
		}

		private void SetWaitingWindow(bool waiting)
		{
			if (waiting)
			{
				GameHubBehaviour.Hub.GuiScripts.SharedPreGameWindow.ShowWaitingWindow(base.GetType());
			}
			else
			{
				GameHubBehaviour.Hub.GuiScripts.SharedPreGameWindow.HideWaitinWindow(base.GetType());
			}
		}

		public UILabel previewItemName;

		public UILabel previewItemDescription;

		[SerializeField]
		private UILabel previewItemName2;

		[SerializeField]
		private UILabel previewItemDescription2;

		public UITexture icon;

		public UITexture icon2;

		[SerializeField]
		private GameObject _description;

		[SerializeField]
		private GameObject _description2;

		private ItemTypeScriptableObject _itemType;

		private int _qnty;

		private System.Action _onBoughtFinished;

		private System.Action _onCloseWindow;

		public UI2DSprite softCoinStamp;

		public UI2DSprite hardCoinStamp;

		public UILabel softCoinLabel;

		public UILabel hardCoinLabel;

		[SerializeField]
		private UIGrid _uiGrid;

		public GameObject softCoinButton;

		public GameObject hardCoinButton;

		public GameObject softCoinGroup;

		public GameObject hardCoinGroup;

		public Collider cancelButtonCollider;

		public UILabel playerSoftBalanceLabel;

		public UILabel playerHardBalanceLabel;

		private int buyType;

		public GameObject WindowGroupGameObject;

		[Header("[Purchased Skin Window]")]
		public float PurchasedSkinDelayBeforeCloseInSec = 2f;

		[Header("[Purchased Skin Audio]")]
		[SerializeField]
		protected FMODAsset SfxUiStoreItemBuy;

		private System.Action _onGoToShopCash;

		private bool _waitingForTransaction;

		private static readonly BitLogger Log = new BitLogger(typeof(ItemBuyWindow));
	}
}
