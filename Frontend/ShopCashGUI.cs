using System;
using System.Globalization;
using ClientAPI;
using ClientAPI.Objects;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.PurchaseFeedback;
using HeavyMetalMachines.Store;
using HeavyMetalMachines.Swordfish;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Frontend
{
	public class ShopCashGUI : ShopScreen
	{
		public void Awake()
		{
			if (!this.EnableCashShop)
			{
				return;
			}
			this._numFilters = Enum.GetNames(typeof(ShopCashGUI.CashGuiFilter)).Length;
			this._currentFilter = ShopCashGUI.CashGuiFilter.Price;
			this.LocalizeAndconfigureFilterTitleLabel();
			this._shopCashGuiLoadFeedback.SetReadyFeedbackAction(new Action(this.EnableShopCashItemsDisplay));
			this.CreateCardsPool();
		}

		protected void OnDestroy()
		{
			this._disposed = true;
			this.TrySetStoreIcon(false);
		}

		private void OnDisable()
		{
			this.SetWaitingForTransaction(false);
		}

		private void CreateCardsPool()
		{
			Debug.Assert(this.ShopCashItemCardPool > 0, string.Format("Invalid value for ShopCashGUI.ShopCashItemCardPool[{0}] - Must be greater than zero.", this.ShopCashItemCardPool), Debug.TargetTeam.All);
			this.ShopCashItemPool = new ShopCashItem[this.ShopCashItemCardPool];
			this.ShopCashItemReference.gameObject.SetActive(false);
			this.ShopCashItemPool[0] = this.ShopCashItemReference;
			for (int i = 1; i < this.ShopCashItemCardPool; i++)
			{
				ShopCashItem shopCashItem = Object.Instantiate<ShopCashItem>(this.ShopCashItemReference, Vector3.zero, Quaternion.identity);
				shopCashItem.transform.parent = this.ShopCashItemReference.transform.parent;
				shopCashItem.transform.localScale = this.ShopCashItemReference.transform.localScale;
				shopCashItem.gameObject.name = "z_shop_item_" + i;
				this.ShopCashItemPool[i] = shopCashItem;
			}
			for (int j = 0; j < this.ShopCashItemPool.Length; j++)
			{
				ShopCashItem shopCashItem2 = this.ShopCashItemPool[j];
				shopCashItem2.ItemSprite.gameObject.SetActive(false);
				shopCashItem2.SoonLabel.gameObject.SetActive(true);
				shopCashItem2.SoonLabel.text = Language.Get("SHOP_CASH_SOON", TranslationContext.MainMenuGui);
				shopCashItem2.HardValueGroupGameObject.SetActive(false);
				shopCashItem2.PriceValueGroupGameObject.SetActive(false);
				shopCashItem2.DiscountGroupGameObject.SetActive(false);
				shopCashItem2.gameObject.SetActive(true);
				shopCashItem2.ButtonBoxCollider.enabled = false;
			}
			this.gridScript.repositionNow = true;
			this.gridScript.Reposition();
			base.UiNavigationAxisSelectorRebuilder.RebuildAndSelect();
			this.LoadItens();
		}

		private void AddShopItem(HardCurrencyProduct hardCurrencyProduct, string isoCurrency)
		{
			for (int i = 0; i < this.ShopCashItemPool.Length; i++)
			{
				ShopCashItem shopCashItem = this.ShopCashItemPool[i];
				if (!shopCashItem.ItemSprite.gameObject.activeSelf)
				{
					shopCashItem.SerialHardCurrencyProductId = hardCurrencyProduct.SerialHardCurrencyProductID;
					string text = string.Empty;
					for (int j = 0; j < hardCurrencyProduct.Images.Length; j++)
					{
						HardCurrencyProductImage hardCurrencyProductImage = hardCurrencyProduct.Images[j];
						if (hardCurrencyProductImage.Type == "in-game")
						{
							text = hardCurrencyProduct.Images[j].Url;
							break;
						}
					}
					Debug.Assert(!string.IsNullOrEmpty(text), string.Format("ShopCashGUI.AddShopItem - Image url not found.", new object[0]), Debug.TargetTeam.All);
					shopCashItem.ItemSprite.SpriteName = text;
					shopCashItem.ItemSprite.gameObject.SetActive(true);
					shopCashItem.SoonLabel.gameObject.SetActive(false);
					int num = hardCurrencyProduct.Amount + hardCurrencyProduct.Bonus;
					shopCashItem.HardTotalLabel.text = num.ToString("0");
					shopCashItem.HardQuantityLabel.text = hardCurrencyProduct.Amount.ToString("0");
					shopCashItem.HardBonusLabel.text = hardCurrencyProduct.Bonus.ToString("0");
					shopCashItem.HardBonusLabel.gameObject.SetActive(0 < hardCurrencyProduct.Bonus);
					shopCashItem.HardValueGroupGameObject.SetActive(true);
					decimal currencyValue = Convert.ToDecimal(hardCurrencyProduct.Value);
					decimal currencyValue2 = Convert.ToDecimal(hardCurrencyProduct.BaseValue);
					shopCashItem.FinalPriceLabel.text = CultureUtils.FormatValue(isoCurrency, currencyValue);
					shopCashItem.OldPriceLabel.text = CultureUtils.FormatValue(isoCurrency, currencyValue2);
					shopCashItem.OldPriceCutLineSprite.gameObject.SetActive(hardCurrencyProduct.IsPromo);
					shopCashItem.OldPriceLabel.gameObject.SetActive(hardCurrencyProduct.IsPromo);
					shopCashItem.PriceValueGroupGameObject.SetActive(true);
					shopCashItem.DiscountGroupGameObject.SetActive(hardCurrencyProduct.IsPromo);
					shopCashItem.DiscountLabel.text = string.Format("-{0:0}%", 100f - hardCurrencyProduct.Value * 100f / hardCurrencyProduct.BaseValue);
					shopCashItem.ButtonBoxCollider.enabled = true;
					UIButton[] components = shopCashItem.ButtonBoxCollider.GetComponents<UIButton>();
					for (int k = 0; k < components.Length; k++)
					{
						components[k].SetState(UIButtonColor.State.Normal, true);
					}
					shopCashItem.GuiEventListener.TheParameterKind = GUIEventListener.ParameterKind.Integer;
					shopCashItem.GuiEventListener.Kind = GUIEventListener.ClickKind.Left;
					shopCashItem.GuiEventListener.IntParameter = i;
					shopCashItem.GuiEventListener.EventListener = base.gameObject;
					return;
				}
			}
		}

		public override void Setup()
		{
			base.Setup();
			base.SetupPageToggleControllers(0);
		}

		private void LoadItens()
		{
			GameHubBehaviour.Hub.ClientApi.billing.GetHardCurrencyProductsWithCultureName(null, new SwordfishClientApi.ParameterizedCallback<HardCurrencyProducts>(this.OnGetHardCurrencyProductsSuccess), new SwordfishClientApi.ErrorCallback(this.OnGetHardCurrencyProductsFailure));
		}

		private void OnGetHardCurrencyProductsSuccess(object state, HardCurrencyProducts hardCurrencyProducts)
		{
			if (this._disposed)
			{
				return;
			}
			if (hardCurrencyProducts.HardCurrencyProduct == null || hardCurrencyProducts.HardCurrencyProduct.Length == 0)
			{
				this.SetEmptyStoreFeedback();
				return;
			}
			this._shopCashGuiLoadFeedback.SetState(ShopCashGUILoadFeedbackState.Ready);
			this.DisplayShopCashItems(hardCurrencyProducts);
		}

		private void OnGetHardCurrencyProductsFailure(object state, Exception exception)
		{
			ShopCashGUI.Log.ErrorFormat("Error on LoadItems. Swordfish GetHardCurrencyProductsWithCultureName - exception: {0}", new object[]
			{
				exception
			});
			this.SetEmptyStoreFeedback();
		}

		private void SetEmptyStoreFeedback()
		{
			this._shopCashGuiLoadFeedback.SetState(ShopCashGUILoadFeedbackState.Unavailable);
			this._shopCashGuiLoadFeedback.ShowFeedback();
		}

		private void DisplayShopCashItems(HardCurrencyProducts hardCurrencyProduct)
		{
			string currencyCultureName = hardCurrencyProduct.CurrencyCultureName;
			ShopCashGUI.Log.DebugFormat("LoadItems. Swordfish GetHardCurrencyProductsWithCultureName - Currency: {0}, CultureInfoName: {1}, TestFormat: {2}", new object[]
			{
				currencyCultureName,
				CultureUtils.GetCultureInfoName(currencyCultureName),
				CultureUtils.FormatValue(currencyCultureName, Convert.ToDecimal(1.99f))
			});
			Debug.Assert(currencyCultureName.Length == RegionInfo.CurrentRegion.ISOCurrencySymbol.Length, string.Format("Invalid CurrencyCultureName:[{0}]. Must be an ISOCurrencySymbol. Current region ISO:[{1}]", currencyCultureName, RegionInfo.CurrentRegion.ISOCurrencySymbol), Debug.TargetTeam.All);
			HardCurrencyProduct[] hardCurrencyProduct2 = hardCurrencyProduct.HardCurrencyProduct;
			Array.Sort<HardCurrencyProduct>(hardCurrencyProduct2, new Comparison<HardCurrencyProduct>(this.SortHardCurrencyProducts));
			for (int i = 0; i < hardCurrencyProduct2.Length; i++)
			{
				this.AddShopItem(hardCurrencyProduct2[i], currencyCultureName);
			}
			this.gridScript.repositionNow = true;
			this.gridScript.Reposition();
			this.LocalizeAndconfigureFilterTitleLabel();
			if (this.IsVisible())
			{
				this._shopCashGuiLoadFeedback.ShowFeedback();
			}
		}

		private int SortHardCurrencyProducts(HardCurrencyProduct x, HardCurrencyProduct y)
		{
			int num = x.Amount + x.Bonus;
			int value = y.Amount + y.Bonus;
			return num.CompareTo(value);
		}

		private void EnableShopCashItemsDisplay()
		{
			this.gridScript.gameObject.SetActive(true);
		}

		public override void Show()
		{
			base.Show();
			this._shopCashGuiLoadFeedback.ShowFeedback();
			this.ShowPage(base.CurrentPage);
			this.gridScript.Reposition();
			this.TrySetStoreIcon(true);
			if (this._shopCashGuiLoadFeedback.GetState() == ShopCashGUILoadFeedbackState.Unavailable)
			{
				ObservableExtensions.Subscribe<long>(Platform.Current.ShowEmptyStoreDialog(), delegate(long onNext)
				{
				}, delegate(Exception onError)
				{
					ShopCashGUI.Log.Error("TRC R4055 breach: Failed to display empty store dialog");
				});
			}
		}

		public override void Hide()
		{
			this.TrySetStoreIcon(false);
			this._shopCashGuiLoadFeedback.HideFeedback();
			base.Hide();
			base.gameObject.SetActive(false);
		}

		public void ShowPage(int page)
		{
			base.CurrentPage = page;
			this.gridScript.repositionNow = true;
			this.gridScript.Reposition();
			base.UpdatePageButtonControllers();
		}

		public void OnClickCashItem(int cashItemIndex)
		{
			this.SetWaitingForTransaction(true);
			this.TrySetStoreIcon(false);
			this.SetItemsColliders(false);
			bool flag = GameHubBehaviour.Hub.ClientApi.overlay.IsOverlayEnabled();
			int serialHardCurrencyProductId = this.ShopCashItemPool[cashItemIndex].SerialHardCurrencyProductId;
			string text = string.Format("SerialHardCurrencyProductId={0} IsOverlayEnabled={1}", serialHardCurrencyProductId, flag);
			this._clientBiLogger.BILogClientMsg(123, text, true);
			if (!flag)
			{
				Guid confirmWindowGuid = Guid.NewGuid();
				ConfirmWindowProperties properties = new ConfirmWindowProperties
				{
					Guid = confirmWindowGuid,
					QuestionText = Language.Get("SHOP_STEAM_OVERLAY_NOT_ENABLED", TranslationContext.Store),
					OkButtonText = Language.Get("Ok", TranslationContext.GUI),
					OnOk = delegate()
					{
						GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.HideConfirmWindow(confirmWindowGuid);
						this.SetItemsColliders(true);
					}
				};
				GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.OpenConfirmWindow(properties);
				this.SetWaitingForTransaction(false);
				this.TrySetStoreIcon(true);
				return;
			}
			CartItem cartItem = new CartItem
			{
				SerialProductId = serialHardCurrencyProductId,
				Quantity = 1
			};
			GameHubBehaviour.Hub.ClientApi.billing.PurchaseItem(null, cartItem, new SwordfishClientApi.ParameterizedCallback<long>(this.SteamPurchaseItemOkCallback), new SwordfishClientApi.ErrorCallback(this.SteamPurchaseItemErrorCallback));
		}

		private void SteamPurchaseItemErrorCallback(object state, Exception exception)
		{
			ShopCashGUI.Log.Error("SteamPurchaseItemErrorCallback: " + exception.Message);
			this.SetItemsColliders(true);
			this.SetWaitingForTransaction(false);
			this.TrySetStoreIcon(true);
		}

		private void SteamPurchaseItemOkCallback(object state, long obj)
		{
			ShopCashGUI.Log.Debug("SteamPurchaseItemOkCallback");
			this.SetWaitingForTransaction(false);
			this.TrySetStoreIcon(true);
			this._purchaseFeedbackComponent.TryToShowBoughtHardCurrency(delegate
			{
				this.SetItemsColliders(true);
			}, this._localBalanceStorage);
		}

		public void GoNextFilter()
		{
			base.CurrentPage = 0;
			if (this._currentFilter + 1 >= (ShopCashGUI.CashGuiFilter)this._numFilters)
			{
				this._currentFilter = ShopCashGUI.CashGuiFilter.Price;
			}
			else
			{
				this._currentFilter++;
			}
			this.ShowPage(base.CurrentPage);
			this.LocalizeAndconfigureFilterTitleLabel();
		}

		public void GoPreviousFilter()
		{
			base.CurrentPage = 0;
			if (this._currentFilter - ShopCashGUI.CashGuiFilter.PriceInverted < 0)
			{
				this._currentFilter = (ShopCashGUI.CashGuiFilter)(this._numFilters - 1);
			}
			else
			{
				this._currentFilter--;
			}
			this.ShowPage(base.CurrentPage);
			this.LocalizeAndconfigureFilterTitleLabel();
		}

		private void LocalizeAndconfigureFilterTitleLabel()
		{
			ShopCashGUI.CashGuiFilter currentFilter = this._currentFilter;
			if (currentFilter != ShopCashGUI.CashGuiFilter.Price)
			{
				if (currentFilter == ShopCashGUI.CashGuiFilter.PriceInverted)
				{
					this.FilterTitle.text = Language.Get("shop_cash_page_filter_PRICE_DESCENDING", TranslationContext.Store);
				}
			}
			else
			{
				this.FilterTitle.text = Language.Get("shop_cash_page_filter_PRICE", TranslationContext.Store);
			}
		}

		private void SetWaitingForTransaction(bool waiting)
		{
			ShopCashGUI.Log.Debug(string.Format("enable/disable waitingWindow purchase {0}, hash: {1}, InstanceID: {2} ", waiting, this.GetHashCode(), base.GetInstanceID()));
			if (waiting)
			{
				GameHubBehaviour.Hub.GuiScripts.SharedPreGameWindow.ShowWaitingWindow(base.GetType());
			}
			else
			{
				GameHubBehaviour.Hub.GuiScripts.SharedPreGameWindow.HideWaitingWindow(base.GetType());
			}
		}

		private void TrySetStoreIcon(bool show)
		{
			if (this.Shop.GetCurrentTab() == ShopGUI.Tab.Cash)
			{
				if (Platform.Current.IsConsole())
				{
					if (show)
					{
						if (!GameHubBehaviour.Hub.ClientApi.publisher.billing.TryShowStoreIcon(0))
						{
							ShopCashGUI.Log.Error("TRC R4051 breach: Publisher store icon not displayed");
						}
					}
					else
					{
						GameHubBehaviour.Hub.ClientApi.publisher.billing.TryHideStoreIcon();
					}
				}
				else
				{
					ShopCashGUI.Log.DebugFormat("TrySetStoreIcon: {0}", new object[]
					{
						show
					});
				}
			}
		}

		private void ShowErrorWindow()
		{
			Guid confirmWindowGuid = Guid.NewGuid();
			ConfirmWindowProperties properties = new ConfirmWindowProperties
			{
				Guid = confirmWindowGuid,
				QuestionText = Language.Get("Buy_Item_Error", TranslationContext.Store),
				OkButtonText = Language.Get("Ok", TranslationContext.Store),
				OnOk = delegate()
				{
					GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.HideConfirmWindow(confirmWindowGuid);
				}
			};
			GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.OpenConfirmWindow(properties);
		}

		private void SetItemsColliders(bool isEnabled)
		{
			this.Shop.SetBackButtonCollider(isEnabled);
			for (int i = 0; i < this.ShopCashItemPool.Length; i++)
			{
				ShopCashItem shopCashItem = this.ShopCashItemPool[i];
				if (shopCashItem.ItemSprite.gameObject.activeSelf)
				{
					shopCashItem.ButtonBoxCollider.enabled = isEnabled;
					UIButton[] components = shopCashItem.ButtonBoxCollider.GetComponents<UIButton>();
					for (int j = 0; j < components.Length; j++)
					{
						components[j].SetState((!isEnabled) ? UIButtonColor.State.Disabled : UIButtonColor.State.Normal, true);
					}
				}
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(ShopCashGUI));

		[Header("[Infra]")]
		[SerializeField]
		private PurchaseFeedbackComponent _purchaseFeedbackComponent;

		[Inject]
		private readonly ILocalBalanceStorage _localBalanceStorage;

		[Header("INTERNAL")]
		public bool EnableCashShop;

		public int ShopCashItemCardPool;

		public ShopCashItem ShopCashItemReference;

		public ShopCashItem[] ShopCashItemPool;

		public UIGrid gridScript;

		public UILabel FilterTitle;

		[SerializeField]
		private ShopCashGUILoadFeedback _shopCashGuiLoadFeedback;

		private ShopCashGUI.CashGuiFilter _currentFilter;

		private int _numFilters;

		public Sprite BestBenefitSprite;

		public Sprite BestValueSprite;

		private bool _disposed;

		[Inject]
		private IClientBILogger _clientBiLogger;

		private enum CashGuiFilter
		{
			Price,
			PriceInverted
		}
	}
}
