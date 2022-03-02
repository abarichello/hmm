using System;
using System.Collections;
using System.Diagnostics;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using ClientAPI.Objects;
using HeavyMetalMachines.DataTransferObjects.Exceptions;
using HeavyMetalMachines.DataTransferObjects.Util;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Store;
using HeavyMetalMachines.Store.Business;
using HeavyMetalMachines.Store.Business.GetStoreItem;
using HeavyMetalMachines.Store.Business.ObserveStoreItem;
using HeavyMetalMachines.Store.Business.PurchaseStoreItem;
using HeavyMetalMachines.Store.Business.PurchaseStoreItem.Exceptions;
using HeavyMetalMachines.VFX;
using Hoplon.Input.UiNavigation;
using Hoplon.Localization.TranslationTable;
using Hoplon.Serialization;
using JetBrains.Annotations;
using Pocketverse;
using Swordfish.Common.exceptions;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Frontend
{
	public class ItemBuyWindow : GameHubBehaviour
	{
		private UiNavigationGroupHolder UiNavigationGroupHolder
		{
			get
			{
				return this._uiNavigationGroupHolder;
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action StoreItemDeactivated;

		private void OnEnable()
		{
			GameHubBehaviour.Hub.Store.OnTransactionReceived += this.OnTransactionReceivedFromSW;
			GameHubBehaviour.Hub.State.ListenToStateChanged += this.LeavingMainMenu;
		}

		private void OnDisable()
		{
			if (GameHubBehaviour.Hub != null)
			{
				if (GameHubBehaviour.Hub.Store != null)
				{
					GameHubBehaviour.Hub.Store.OnTransactionReceived -= this.OnTransactionReceivedFromSW;
				}
				GameHubBehaviour.Hub.State.ListenToStateChanged -= this.LeavingMainMenu;
			}
		}

		private void LeavingMainMenu(GameState changedState)
		{
			this._onCloseWindow = null;
			this.SetWaitingWindow(false);
			this.CloseWindow();
		}

		[UsedImplicitly]
		public void OnBuyClick(int type)
		{
			this._buyType = ((type != 1) ? ItemBuyWindow.BuyType.Soft : ItemBuyWindow.BuyType.Hard);
			Guid id = this._itemType.Id;
			IStoreBusinessFactory storeBusinessFactory = this._diContainer.Resolve<IStoreBusinessFactory>();
			IGetStoreItem getStoreItem = storeBusinessFactory.CreateGetStoreItem();
			StoreItem storeItem = getStoreItem.Get(id);
			long num = storeItem.SoftPrice;
			long num2 = storeItem.HardPrice;
			ILocalBalanceStorage localBalanceStorage = this._diContainer.Resolve<ILocalBalanceStorage>();
			num *= (long)this._quantity;
			num2 *= (long)this._quantity;
			StoreConfirmationWindow storeConfirmationWindow;
			SingletonMonoBehaviour<PanelController>.Instance.ShowModalWindow<StoreConfirmationWindow>(out storeConfirmationWindow);
			if (this._buyType == ItemBuyWindow.BuyType.Hard)
			{
				if (num2 > localBalanceStorage.HardCurrency)
				{
					storeConfirmationWindow.ShowConfirmationMessage(Language.Get("No_Funds_HardCoin", TranslationContext.Store), Language.Get("SIM", TranslationContext.MainMenuGui), Language.Get("CANCELAR", TranslationContext.MainMenuGui), this._onGoToShopCash);
					return;
				}
			}
			else if (num > (long)localBalanceStorage.SoftCurrency)
			{
				storeConfirmationWindow.ShowOkConfirmationMessage(Language.Get("No_Funds_SoftCoin", TranslationContext.Store), Language.Get("Ok", TranslationContext.Store), this._onGoToShopCash);
				return;
			}
			storeConfirmationWindow.ShowBoughtMessage(Language.Get("Bought_Confirmation", TranslationContext.Store), Language.Get("Bought_Confirmation_OK", TranslationContext.Store), Language.Get("Bought_Confirmation_Cancel", TranslationContext.Store), new Action(this.OnDoBuy));
		}

		public void OnDoBuy()
		{
			if (this._waitingForTransaction)
			{
				return;
			}
			this.SetButtonsActive(false);
			this.SetWaitingWindow(true);
			this._waitingForTransaction = true;
			IStoreBusinessFactory storeBusinessFactory = this._diContainer.Resolve<IStoreBusinessFactory>();
			StoreItem storeItem = storeBusinessFactory.CreateGetStoreItem().Get(this._itemType.Id);
			ItemCategoryScriptableObject itemCategoryScriptableObject = GameHubBehaviour.Hub.InventoryColletion.AllItemCategories[this._itemType.ItemCategoryId];
			Inventory inventoryByKind = GameHubBehaviour.Hub.User.Inventory.GetInventoryByKind(itemCategoryScriptableObject.Kind);
			if (this._buyType == ItemBuyWindow.BuyType.Hard)
			{
				if (this._itemType != null)
				{
					HardCurrencyPurchase purchase = new HardCurrencyPurchase
					{
						StoreItemId = this._itemType.Id,
						SeenUnitPrice = storeItem.HardPrice,
						Quantity = (long)this._quantity,
						InventoryId = inventoryByKind.Id
					};
					ObservableExtensions.Subscribe<PurchaseResult>(storeBusinessFactory.CreatePurchaseStoreItem().PurchaseWithHardCurrency(purchase), new Action<PurchaseResult>(this.OnBoughtSuccessful), delegate(Exception exception)
					{
						this.OnBoughtFailure(this._itemType.Id, exception);
					});
				}
				else
				{
					ItemBuyWindow.Log.WarnFormat("Null _itemType on BuyItemUsingHardCoins", new object[0]);
				}
			}
			else if (this._itemType != null)
			{
				SoftCurrencyPurchase purchase2 = new SoftCurrencyPurchase
				{
					StoreItemId = this._itemType.Id,
					SeenUnitPrice = storeItem.SoftPrice,
					InventoryId = inventoryByKind.Id
				};
				ObservableExtensions.Subscribe<PurchaseResult>(storeBusinessFactory.CreatePurchaseStoreItem().PurchaseWithSoftCurrency(purchase2), new Action<PurchaseResult>(this.OnBoughtSuccessful), delegate(Exception exception)
				{
					this.OnBoughtFailure(this._itemType.Id, exception);
				});
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
			ItemBuyWindow.Log.DebugFormat("Item bought with hard coins successfully. {0}", new object[]
			{
				transactionId
			});
			base.StartCoroutine(this.WaitToClosePurchasedSkinWindowAsync());
		}

		private void OnTransactionReceivedFromSW(long itemId)
		{
			if (!this._waitingForTransaction)
			{
				ItemBuyWindow.Log.Error("Transaction callback received was NOT expected. Ignoring it.");
				return;
			}
			ItemBuyWindow.Log.Debug("Transaction callback received. Will show feedback window.");
			this.DoBoughtEnd(true, itemId, null);
		}

		private void OnBoughtSuccessful(PurchaseResult purchaseResult)
		{
			ItemBuyWindow.Log.InfoFormat("OnBoughtSuccessful itemId:{0}", new object[]
			{
				purchaseResult.PurchasedItem.Id
			});
			this.DoBoughtEnd(true, purchaseResult.PurchasedItem.Id, null);
			base.StartCoroutine(this.WaitToClosePurchasedSkinWindowAsync());
		}

		private void DoBoughtEnd(bool success, long itemId, Exception exception = null)
		{
			this._waitingForTransaction = false;
			ItemBuyWindow.Log.DebugFormat("XXX DoBoughtEnd success:{0} itemId:{1}", new object[]
			{
				success,
				itemId
			});
			if (success)
			{
				if (itemId == -1L)
				{
					ItemBuyWindow.Log.Error("CANNOT HAVE SUCCESS AND HAVE ITEMID = -1 !!!!");
					return;
				}
				SwordfishStore.BalanceType balanceType = (this._buyType != ItemBuyWindow.BuyType.Hard) ? SwordfishStore.BalanceType.Soft : SwordfishStore.BalanceType.Hard;
				GameHubBehaviour.Hub.Store.GetBalance(null, balanceType);
				this.SetWaitingWindow(false);
				this.ConfigureFeedBack(delegate
				{
					base.StartCoroutine(this.AnimateFeedBack());
				});
			}
			else
			{
				this.SetWaitingWindow(false);
				if (exception is StoreItemDeactivatedException)
				{
					this.ShowErrorWindow("WARNING_ITEM_DEACTIVATED");
				}
				else if (exception is StoreItemPriceChangedException)
				{
					this.ShowErrorWindow("WARNING_PRICE_ERROR");
				}
				else
				{
					this.ShowErrorWindow("Buy_Item_Error");
				}
			}
		}

		private IEnumerator AnimateFeedBack()
		{
			yield return UnityUtils.WaitForOneSecond;
			this.CloseWindow();
			yield break;
		}

		private void ConfigureFeedBack(Action whenDone = null)
		{
			this.hardCoinButton.SetActive(false);
			this.softCoinButton.SetActive(false);
			string name = this._itemType.Name;
			string baseSKU = this._itemType.BaseSKU;
			string text = this._itemType.ItemCategoryId.ToString();
			IStoreBusinessFactory storeBusinessFactory = this._diContainer.Resolve<IStoreBusinessFactory>();
			IGetStoreItem getStoreItem = storeBusinessFactory.CreateGetStoreItem();
			StoreItem storeItem = getStoreItem.Get(this._itemType.Id);
			string text2 = (this._buyType != ItemBuyWindow.BuyType.Hard) ? "SC" : "HC";
			long num = (this._buyType != ItemBuyWindow.BuyType.Hard) ? storeItem.SoftPrice : storeItem.HardPrice;
			UI2DSprite ui2DSprite = (this._buyType != ItemBuyWindow.BuyType.Hard) ? this.softCoinStamp : this.hardCoinStamp;
			GameObject go = (this._buyType != ItemBuyWindow.BuyType.Hard) ? this.softCoinGroup : this.hardCoinGroup;
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
			GameHubBehaviour.Hub.Swordfish.Log.BILogClientMsg(21, string.Format("Sku={0} ItemName={1} ItemCategory={2} ItemPrice={3}", new object[]
			{
				text5,
				name,
				text,
				num
			}), true);
		}

		private void OnBoughtFailure(Guid itemTypeId, Exception exception)
		{
			if (exception is StoreItemDeactivatedException || exception is StoreItemPriceChangedException)
			{
				ItemBuyWindow.Log.WarnFormat("Could not complete purchase. {0}", new object[]
				{
					exception.Message
				});
			}
			else
			{
				ItemBuyWindow.Log.Error("OnBoughtFailure " + exception);
			}
			if (exception is SwordfishException)
			{
				SwordfishExceptionJson swordfishExceptionJson = (SwordfishExceptionJson)((JsonSerializeable<!0>)exception.Message);
				if (swordfishExceptionJson != null && swordfishExceptionJson.ExceptionType == null)
				{
					ItemBuyWindow.Log.WarnFormat("SwordfishException: DuplicatedItem. The software will assume it was just bought", new object[0]);
					this._waitingForTransaction = false;
					ItemTypeScriptableObject itemTypeScriptableObject = GameHubBehaviour.Hub.InventoryColletion.AllItemTypes[itemTypeId];
					InventoryBag.InventoryKind inventoryKind = InventoryMapper.CategoriesToInventoryKind[itemTypeScriptableObject.ItemCategoryId];
					InventoryAdapter inventoryAdapterByKind = GameHubBehaviour.Hub.User.Inventory.GetInventoryAdapterByKind(inventoryKind);
					GameHubBehaviour.Hub.User.Inventory.ReloadItemByItemTypeId(itemTypeId, inventoryAdapterByKind.Inventory.Id, delegate(long itemId)
					{
						this.DoBoughtEnd(false, itemId, exception);
						this.StartCoroutine(this.WaitToClosePurchasedSkinWindowAsync());
					});
					return;
				}
			}
			else if (exception is StoreItemDeactivatedException && this.StoreItemDeactivated != null)
			{
				this.StoreItemDeactivated();
			}
			this.DoBoughtEnd(false, -1L, exception);
			this.CloseWindow();
		}

		private void ShowErrorWindow(string messageKey = "Buy_Item_Error")
		{
			Guid confirmWindowGuid = Guid.NewGuid();
			ConfirmWindowProperties properties = new ConfirmWindowProperties
			{
				Guid = confirmWindowGuid,
				QuestionText = Language.Get(messageKey, TranslationContext.Store),
				OkButtonText = Language.Get("Ok", TranslationContext.Store),
				OnOk = delegate()
				{
					this.SetButtonsActive(true);
					GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.HideConfirmWindow(confirmWindowGuid);
				}
			};
			GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.OpenConfirmWindow(properties);
		}

		public void ShowBuyWindow(IItemType itemType, Action onBoughtFinished, Action onBuyWindowClosed, Action onGoToShopCash, int qnty, bool portrait)
		{
			if (itemType.ItemCategoryId == InventoryMapper.EmoteCategoryGuid)
			{
				this._description.SetActive(false);
				this._description2.SetActive(false);
				this._description3.SetActive(true);
			}
			else
			{
				this._description.SetActive(portrait);
				this._description2.SetActive(!portrait);
				this._description3.SetActive(false);
			}
			this._itemType = itemType;
			this._quantity = qnty;
			base.gameObject.SetActive(true);
			this.WindowGroupGameObject.SetActive(true);
			this.SetButtonsActive(true);
			StoreItem storeItem = this._diContainer.Resolve<IStoreBusinessFactory>().CreateGetStoreItem().Get(itemType.Id);
			bool isSoftPurchasable = storeItem.IsSoftPurchasable;
			bool isHardPurchasable = storeItem.IsHardPurchasable;
			string name = itemType.Name;
			string text = itemType.Id.ToString();
			bool isCharacter = false;
			string arg = itemType.ItemCategoryId.ToString();
			ShopItemTypeComponent component = itemType.GetComponent<ShopItemTypeComponent>();
			if (null == component)
			{
				this.SetupBuyWindow(name, text, itemType.ItemCategoryId, isHardPurchasable, isSoftPurchasable, isCharacter, qnty, portrait);
			}
			else
			{
				this.SetupBuyWindow(itemType, component, qnty, portrait);
			}
			ItemBuyWindow.Log.InfoFormat("Buying item={0} id={1}", new object[]
			{
				name,
				text
			});
			string msg = string.Format("ItemType={0} Category={1}", text, arg);
			GameHubBehaviour.Hub.Swordfish.Log.BILogClientMsg(36, msg, true);
			this._onCloseWindow = onBuyWindowClosed;
			this._onBoughtFinished = onBoughtFinished;
			this._onGoToShopCash = onGoToShopCash;
			this.UiNavigationGroupHolder.AddHighPriorityGroup();
		}

		private void SetupBuyWindow(string itemName, string id, Guid categoryId, bool isHardPurchase, bool isSoftPurchase, bool isCharacter, int quantity, bool portrait)
		{
			if (categoryId == InventoryMapper.EmoteCategoryGuid)
			{
				this.SetupBuyWindowImages(isCharacter, itemName, this.spritesheet);
				this.SetupBuyWindowTranslation(isCharacter, itemName, quantity, this.previewItemName3, this.previewItemDescription3);
			}
			else if (portrait)
			{
				this.SetupBuyWindowImages(isCharacter, itemName, (HMMUI2DDynamicTexture)this.icon);
				this.SetupBuyWindowTranslation(isCharacter, itemName, quantity, this.previewItemName, this.previewItemDescription);
			}
			else
			{
				this.SetupBuyWindowImages(isCharacter, itemName, (HMMUI2DDynamicTexture)this.icon2);
				this.SetupBuyWindowTranslation(isCharacter, itemName, quantity, this.previewItemName2, this.previewItemDescription2);
			}
			if (isSoftPurchase)
			{
				this.softCoinGroup.SetActive(true);
				this.softCoinButton.SetActive(true);
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
				TweenAlpha.Begin(this.hardCoinGroup, 0.5f, 1f);
			}
			else
			{
				this.hardCoinGroup.SetActive(false);
			}
			this.hardCoinStamp.gameObject.SetActive(false);
			this.softCoinStamp.gameObject.SetActive(false);
			this.SetupPricing(new Guid(id), quantity);
			this.SetupBuyWindowButtons();
		}

		private void SetupBuyWindow(IItemType itemType, ShopItemTypeComponent shopComponent, int quantity, bool portrait)
		{
			if (itemType.ItemCategoryId == InventoryMapper.EmoteCategoryGuid)
			{
				this.SetupBuyWindowImages(shopComponent, this.spritesheet);
				this.SetupBuyWindowTranslation(itemType, quantity, this.previewItemName3, this.previewItemDescription3);
			}
			else if (portrait)
			{
				this.SetupBuyWindowImages(shopComponent, (HMMUI2DDynamicTexture)this.icon);
				this.SetupBuyWindowTranslation(itemType, quantity, this.previewItemName, this.previewItemDescription);
			}
			else
			{
				this.SetupBuyWindowImages(shopComponent, (HMMUI2DDynamicTexture)this.icon2);
				this.SetupBuyWindowTranslation(itemType, quantity, this.previewItemName2, this.previewItemDescription2);
			}
			StoreItem storeItem = this._diContainer.Resolve<IStoreBusinessFactory>().CreateGetStoreItem().Get(itemType.Id);
			bool isSoftPurchasable = storeItem.IsSoftPurchasable;
			this.softCoinGroup.SetActive(isSoftPurchasable);
			if (isSoftPurchasable)
			{
				this.softCoinButton.SetActive(true);
				TweenAlpha.Begin(this.softCoinGroup, 0.5f, 1f);
			}
			bool isHardPurchasable = storeItem.IsHardPurchasable;
			this.hardCoinGroup.SetActive(isHardPurchasable);
			if (isHardPurchasable)
			{
				this.hardCoinButton.SetActive(true);
				TweenAlpha.Begin(this.hardCoinGroup, 0.5f, 1f);
			}
			this.hardCoinStamp.gameObject.SetActive(false);
			this.softCoinStamp.gameObject.SetActive(false);
			this.SetupPricing(itemType.Id, quantity);
			this.SetupBuyWindowButtons();
		}

		private void SetupPricing(Guid itemId, int quantity)
		{
			IStoreBusinessFactory storeBusinessFactory = this._diContainer.Resolve<IStoreBusinessFactory>();
			IGetStoreItem getStoreItem = storeBusinessFactory.CreateGetStoreItem();
			StoreItem storeItem = getStoreItem.Get(itemId);
			this.SetupBuyWindowPricesLabel(storeItem, quantity);
			IObserveStoreItem observeStoreItem = storeBusinessFactory.CreateObserveStoreItem();
			IObservable<StoreItem> observable = observeStoreItem.CreateObservable(itemId);
			this._storeItemObservation = ObservableExtensions.Subscribe<StoreItem>(observable, delegate(StoreItem storeItemPrices)
			{
				this.SetupBuyWindowPricesLabel(storeItemPrices, quantity);
			});
		}

		private void SetupBuyWindowPricesLabel(StoreItem storeItem, int quantity)
		{
			ILocalBalanceStorage localBalanceStorage = this._diContainer.Resolve<ILocalBalanceStorage>();
			long num = storeItem.SoftPrice;
			long num2 = storeItem.HardPrice;
			num *= (long)quantity;
			num2 *= (long)quantity;
			this.softCoinLabel.text = num.ToString();
			this.hardCoinLabel.text = num2.ToString();
			this.playerSoftBalanceLabel.text = localBalanceStorage.SoftCurrency.ToString();
			this.playerHardBalanceLabel.text = localBalanceStorage.HardCurrency.ToString();
		}

		private void SetupBuyWindowImages(bool isCharacter, string itemName, HMMUI2DDynamicTexture dynamicTexture)
		{
			if (isCharacter)
			{
				dynamicTexture.TextureName = itemName + "_skin_00";
				((HMMUI2DDynamicTexture)this.icon2).TextureName = itemName + "_skin_00";
			}
			else
			{
				dynamicTexture.TextureName = itemName;
			}
		}

		private void SetupBuyWindowImages(ShopItemTypeComponent shopComponent, HMMUI2DDynamicTexture dynamicTexture)
		{
			dynamicTexture.TextureName = shopComponent.IconAssetName;
		}

		private void SetupBuyWindowTranslation(bool isCharacter, string itemName, int quantity, UILabel itemNameLabel, UILabel itemDescriptionLabel)
		{
			string text = string.Format("{0}_name", itemName);
			string text2 = string.Format("{0}_description", itemName);
			ContextTag context;
			if (isCharacter)
			{
				context = TranslationContext.CharactersBaseInfo;
				text = text.ToUpper();
				text2 = text2.ToUpper();
			}
			else
			{
				context = TranslationContext.Items;
			}
			string formatted = Language.GetFormatted(text, context, new object[]
			{
				quantity
			});
			string formatted2 = Language.GetFormatted(text2, context, new object[]
			{
				quantity
			});
			itemNameLabel.text = formatted;
			itemDescriptionLabel.text = formatted2;
		}

		private void SetupBuyWindowTranslation(IItemType itemType, int quantity, UILabel itemNameLabel, UILabel itemDescriptionLabel)
		{
			ShopItemTypeComponent component = itemType.GetComponent<ShopItemTypeComponent>();
			SkinPrefabItemTypeComponent skinPrefabItemTypeComponent;
			ShopDetailsComponent shopDetailsComponent;
			string text;
			string text2;
			if (itemType.TryGetComponent<SkinPrefabItemTypeComponent>(out skinPrefabItemTypeComponent) && itemType.TryGetComponent<ShopDetailsComponent>(out shopDetailsComponent))
			{
				text = Language.Get(skinPrefabItemTypeComponent.CardSkinDraft, TranslationContext.Items);
				text2 = Language.Get(shopDetailsComponent.SkinQuoteTextDraf, TranslationContext.Items);
			}
			else
			{
				text = Language.GetFormatted(component.TitleDraft, TranslationContext.Items, new object[]
				{
					quantity
				});
				text2 = Language.GetFormatted(component.DescriptionDraft, TranslationContext.Items, new object[]
				{
					quantity
				});
			}
			itemNameLabel.text = text;
			itemDescriptionLabel.text = text2;
		}

		private void SetupBuyWindowButtons()
		{
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
			Debug.Log("[RTS] ItemBuyWindow CloseWindow");
			if (this._storeItemObservation != null)
			{
				this._storeItemObservation.Dispose();
				this._storeItemObservation = null;
			}
			if (this._onCloseWindow != null)
			{
				this._onCloseWindow();
				this._onCloseWindow = null;
			}
			this.UiNavigationGroupHolder.RemoveHighPriorityGroup();
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
			ItemBuyWindow.Log.Debug(string.Format("enable/disable waitingWindow BuyWindow {0}, hash: {1}, InstanceID: {2} ", waiting, this.GetHashCode(), base.GetInstanceID()));
			if (waiting)
			{
				GameHubBehaviour.Hub.GuiScripts.SharedPreGameWindow.ShowWaitingWindow(base.GetType());
			}
			else
			{
				GameHubBehaviour.Hub.GuiScripts.SharedPreGameWindow.HideWaitingWindow(base.GetType());
			}
		}

		public UILabel previewItemName;

		public UILabel previewItemDescription;

		[SerializeField]
		private UILabel previewItemName2;

		[SerializeField]
		private UILabel previewItemDescription2;

		[SerializeField]
		private UILabel previewItemName3;

		[SerializeField]
		private UILabel previewItemDescription3;

		public UITexture icon;

		public UITexture icon2;

		[SerializeField]
		private AnimatedNguiTexture spritesheet;

		[SerializeField]
		private GameObject _description;

		[SerializeField]
		private GameObject _description2;

		[SerializeField]
		private GameObject _description3;

		private IItemType _itemType;

		private int _quantity;

		private Action _onBoughtFinished;

		private Action _onCloseWindow;

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

		public GameObject WindowGroupGameObject;

		[Header("[Purchased Skin Window]")]
		public float PurchasedSkinDelayBeforeCloseInSec = 2f;

		[Header("[Purchased Skin Audio]")]
		[SerializeField]
		protected AudioEventAsset SfxUiStoreItemBuy;

		[Header("[Ui Navigation]")]
		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;

		[InjectOnClient]
		private DiContainer _diContainer;

		private IDisposable _storeItemObservation;

		private Action _onGoToShopCash;

		private bool _waitingForTransaction;

		private static readonly BitLogger Log = new BitLogger(typeof(ItemBuyWindow));

		private ItemBuyWindow.BuyType _buyType;

		private enum BuyType
		{
			Soft,
			Hard
		}
	}
}
