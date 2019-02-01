using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using ClientAPI;
using ClientAPI.Objects;
using HeavyMetalMachines.PurchaseFeedback;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine;

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
			this.gridScript.sorting = UIGrid.Sorting.Custom;
			this.gridScript.onCustomSort = new Comparison<Transform>(this.GridCustomSort);
			this._shopCashGuiLoadFeedback.SetReadyFeedbackAction(new System.Action(this.EnableShopCashItemsDisplay));
			base.StartCoroutine(this.CreateCardsPool());
		}

		protected void OnDestroy()
		{
			this._disposed = true;
		}

		private void OnDisable()
		{
			this.SetWaitingForTransaction(false);
		}

		private IEnumerator CreateCardsPool()
		{
			HeavyMetalMachines.Utils.Debug.Assert(this.ShopCashItemCardPool > 0, string.Format("Invalid value for ShopCashGUI.ShopCashItemCardPool[{0}] - Must be greater than zero.", this.ShopCashItemCardPool), HeavyMetalMachines.Utils.Debug.TargetTeam.All);
			this.ShopCashItemPool = new ShopCashItem[this.ShopCashItemCardPool];
			this.ShopCashItemReference.gameObject.SetActive(false);
			this.ShopCashItemPool[0] = this.ShopCashItemReference;
			for (int i = 1; i < this.ShopCashItemCardPool; i++)
			{
				ShopCashItem shopCashItem = UnityEngine.Object.Instantiate<ShopCashItem>(this.ShopCashItemReference, Vector3.zero, Quaternion.identity);
				shopCashItem.transform.parent = this.ShopCashItemReference.transform.parent;
				shopCashItem.transform.localScale = this.ShopCashItemReference.transform.localScale;
				shopCashItem.gameObject.name = "z_shop_item_" + i;
				this.ShopCashItemPool[i] = shopCashItem;
				yield return UnityUtils.WaitForEndOfFrame;
			}
			for (int j = 0; j < this.ShopCashItemPool.Length; j++)
			{
				ShopCashItem shopCashItem2 = this.ShopCashItemPool[j];
				shopCashItem2.ItemSprite.gameObject.SetActive(false);
				shopCashItem2.SoonLabel.gameObject.SetActive(true);
				shopCashItem2.SoonLabel.text = Language.Get("SHOP_CASH_SOON", TranslationSheets.MainMenuGui);
				shopCashItem2.HardValueGroupGameObject.SetActive(false);
				shopCashItem2.PriceValueGroupGameObject.SetActive(false);
				shopCashItem2.DiscountGroupGameObject.SetActive(false);
				shopCashItem2.gameObject.SetActive(true);
				shopCashItem2.ButtonBoxCollider.enabled = false;
			}
			this.gridScript.repositionNow = true;
			this.gridScript.Reposition();
			this.LoadItens();
			yield break;
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
					HeavyMetalMachines.Utils.Debug.Assert(!string.IsNullOrEmpty(text), string.Format("ShopCashGUI.AddShopItem - Image url not found.", new object[0]), HeavyMetalMachines.Utils.Debug.TargetTeam.All);
					shopCashItem.ItemSprite.SpriteName = text;
					shopCashItem.ItemSprite.gameObject.SetActive(true);
					shopCashItem.SoonLabel.gameObject.SetActive(false);
					int num = hardCurrencyProduct.Amount + hardCurrencyProduct.Bonus;
					shopCashItem.gameObject.name = num.ToString("0");
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
			if (this.IsHardCurrencyProductsUnavailable(hardCurrencyProducts.HardCurrencyProduct))
			{
				this._shopCashGuiLoadFeedback.SetState(ShopCashGUILoadFeedbackState.Unavailable);
				this._shopCashGuiLoadFeedback.ShowFeedback();
				return;
			}
			this._shopCashGuiLoadFeedback.SetState(ShopCashGUILoadFeedbackState.Ready);
			this.DisplayShopCashItems(hardCurrencyProducts);
		}

		private bool IsHardCurrencyProductsUnavailable(ICollection<HardCurrencyProduct> hardCurrencyProduct)
		{
			return hardCurrencyProduct == null || hardCurrencyProduct.Count == 0;
		}

		private void DisplayShopCashItems(HardCurrencyProducts hardCurrencyProduct)
		{
			HardCurrencyProduct[] hardCurrencyProduct2 = hardCurrencyProduct.HardCurrencyProduct;
			string currencyCultureName = hardCurrencyProduct.CurrencyCultureName;
			HeavyMetalMachines.Utils.Debug.Assert(currencyCultureName.Length == RegionInfo.CurrentRegion.ISOCurrencySymbol.Length, string.Format("Invalid CurrencyCultureName:[{0}]. Must be an ISOCurrencySymbol. Current region ISO:[{1}]", currencyCultureName, RegionInfo.CurrentRegion.ISOCurrencySymbol), HeavyMetalMachines.Utils.Debug.TargetTeam.All);
			for (int i = 0; i < hardCurrencyProduct2.Length; i++)
			{
				this.AddShopItem(hardCurrencyProduct2[i], currencyCultureName);
			}
			this.gridScript.repositionNow = true;
			this.gridScript.Reposition();
			this.LocalizeAndconfigureFilterTitleLabel();
			this._shopCashGuiLoadFeedback.ShowFeedback();
		}

		private void EnableShopCashItemsDisplay()
		{
			this.gridScript.gameObject.SetActive(true);
		}

		private void OnGetHardCurrencyProductsFailure(object state, Exception exception)
		{
			ShopCashGUI.Log.ErrorFormat("Error on LoadItems. Swordfish GetHardCurrencyProductsWithCultureName - exception: {0}", new object[]
			{
				exception
			});
		}

		public override void Show()
		{
			this._shopCashGuiLoadFeedback.ShowFeedback();
			base.Show();
			this.ShowPage(base.CurrentPage);
			this.gridScript.Reposition();
		}

		public override void Hide()
		{
			this._shopCashGuiLoadFeedback.HideFeedback();
			base.Hide();
		}

		public void ShowPage(int page)
		{
			base.CurrentPage = page;
			this.gridScript.repositionNow = true;
			this.gridScript.Reposition();
			base.UpdatePageButtonControllers();
		}

		public void OnClickCashItem(int charIndex)
		{
			this.SetItemsColliders(false);
			if (!GameHubBehaviour.Hub.ClientApi.overlay.IsOverlayEnabled())
			{
				Guid confirmWindowGuid = Guid.NewGuid();
				ConfirmWindowProperties properties = new ConfirmWindowProperties
				{
					Guid = confirmWindowGuid,
					QuestionText = Language.Get("SHOP_STEAM_OVERLAY_NOT_ENABLED", TranslationSheets.Store),
					OkButtonText = Language.Get("Ok", "GUI"),
					OnOk = delegate()
					{
						GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.HideConfirmWindow(confirmWindowGuid);
						this.SetItemsColliders(true);
					}
				};
				GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.OpenConfirmWindow(properties);
				return;
			}
			this.SetWaitingForTransaction(true);
			CartItem cartItem = new CartItem
			{
				SerialProductId = this.ShopCashItemPool[charIndex].SerialHardCurrencyProductId,
				Quantity = 1
			};
			GameHubBehaviour.Hub.ClientApi.billing.SteamPurchaseItem(null, cartItem, new SwordfishClientApi.ParameterizedCallback<long>(this.SteamPurchaseItemOkCallback), new SwordfishClientApi.ErrorCallback(this.SteamPurchaseItemErrorCallback));
		}

		private void SteamPurchaseItemErrorCallback(object state, Exception exception)
		{
			ShopCashGUI.Log.Error("SteamPurchaseItemErrorCallback: " + exception.Message);
			this.SetItemsColliders(true);
			this.SetWaitingForTransaction(false);
		}

		private void SteamPurchaseItemOkCallback(object state, long obj)
		{
			this.SetWaitingForTransaction(false);
			this._purchaseFeedbackComponent.TryToShowBoughtHardCurrency(delegate
			{
				this.SetItemsColliders(true);
			});
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
					this.FilterTitle.text = Language.Get("shop_cash_page_filter_PRICE_DESCENDING", TranslationSheets.Store);
				}
			}
			else
			{
				this.FilterTitle.text = Language.Get("shop_cash_page_filter_PRICE", TranslationSheets.Store);
			}
		}

		private int GridCustomSort(Transform x, Transform y)
		{
			int num = (this._currentFilter != ShopCashGUI.CashGuiFilter.PriceInverted) ? 1 : -1;
			int num2;
			bool flag = int.TryParse(x.name, out num2);
			int value;
			bool flag2 = int.TryParse(y.name, out value);
			if (flag && flag2)
			{
				return num2.CompareTo(value) * num;
			}
			if (flag)
			{
				return -1;
			}
			if (flag2)
			{
				return 1;
			}
			return string.CompareOrdinal(x.name, y.name) * num;
		}

		private void SetWaitingForTransaction(bool waiting)
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
					GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.HideConfirmWindow(confirmWindowGuid);
				}
			};
			GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.OpenConfirmWindow(properties);
		}

		private void SetItemsColliders(bool isEnabled)
		{
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

		private enum CashGuiFilter
		{
			Price,
			PriceInverted
		}
	}
}
