using System;
using ClientAPI;
using ClientAPI.Objects;
using HeavyMetalMachines.Store;
using Pocketverse;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HeavyMetalMachines.PurchaseFeedback
{
	[CreateAssetMenu(menuName = "ScriptableObject/Temp/PurchaseFeedbackComponent")]
	public class PurchaseFeedbackComponent : GameHubScriptableObject, IPurchaseFeedbackComponent
	{
		public void TryToShowBoughtHardCurrency(Action onTryToShowEnd, ILocalBalanceStorage localBalanceStorage)
		{
			this._onTryToShowEnd = onTryToShowEnd;
			GameHubScriptableObject.Hub.ClientApi.billing.GetMyProductsNotSaw(localBalanceStorage, new SwordfishClientApi.ParameterizedCallback<UserHardCurrencyProduct[]>(this.OnGetMyProductsNotSaw), new SwordfishClientApi.ErrorCallback(this.OnGetMyProductsNotSawError));
		}

		private void OnGetMyProductsNotSaw(object state, UserHardCurrencyProduct[] userHardCurrencyProducts)
		{
			if (userHardCurrencyProducts.Length == 0)
			{
				this.TryCallEndCallback();
				return;
			}
			ILocalBalanceStorage localBalanceStorage = (ILocalBalanceStorage)state;
			UserHardCurrencyProduct userHardCurrencyProduct = userHardCurrencyProducts[0];
			PurchaseFeedbackComponent.Log.DebugFormat("ClientApi billing GetMyProductsNotSaw. Item:[{0}]", new object[]
			{
				userHardCurrencyProduct.Id
			});
			this.CachePurchasedFeedbackViewdata(userHardCurrencyProduct, localBalanceStorage);
			this.LoadViewScene();
			this.UpdateUserSawProductList(userHardCurrencyProduct);
		}

		private void TryCallEndCallback()
		{
			if (this._onTryToShowEnd != null)
			{
				this._onTryToShowEnd();
				this._onTryToShowEnd = null;
			}
		}

		private void OnGetMyProductsNotSawError(object state, Exception exception)
		{
			PurchaseFeedbackComponent.Log.ErrorFormat("Error on LoadItems. Swordfish GetMyProductsNotSaw - exception: {0}", new object[]
			{
				exception
			});
			this.TryCallEndCallback();
		}

		private void UpdateUserSawProductList(UserHardCurrencyProduct hardCurrencyProduct)
		{
			GameHubScriptableObject.Hub.ClientApi.billing.UpdateUserSawProductList(null, new long[]
			{
				hardCurrencyProduct.Id
			}, delegate(object obj)
			{
				PurchaseFeedbackComponent.Log.Debug("ClientApi billing UpdateUserSawProductList done.");
			}, delegate(object obj, Exception exception)
			{
				PurchaseFeedbackComponent.Log.ErrorFormat("Error on LoadItems. Swordfish UpdateUserSawProductList - exception: {0}", new object[]
				{
					exception
				});
			});
		}

		private void CachePurchasedFeedbackViewdata(UserHardCurrencyProduct hardCurrencyProduct, ILocalBalanceStorage localBalanceStorage)
		{
			this._purchasedFeedbackViewData = default(PurchaseFeedbackView.PurchasedFeedbackViewData);
			for (int i = 0; i < hardCurrencyProduct.Images.Length; i++)
			{
				HardCurrencyProductImage hardCurrencyProductImage = hardCurrencyProduct.Images[i];
				if (hardCurrencyProductImage.Type == "in-game")
				{
					this._purchasedFeedbackViewData.ItemImageAssetName = hardCurrencyProductImage.Url;
					break;
				}
			}
			this._purchasedFeedbackViewData.ItemHardCurrencyValue = hardCurrencyProduct.Value;
			this._purchasedFeedbackViewData.UserCurrentCurrencyValue = localBalanceStorage.HardCurrency.ToString("0");
		}

		public void RegisterView(IPurchaseFeedbackView purchaseFeedbackView)
		{
			this._purchaseFeedbackView = purchaseFeedbackView;
			this._purchaseFeedbackView.Show(this._purchasedFeedbackViewData);
		}

		public void HideView()
		{
			this._purchaseFeedbackView.Hide();
		}

		public void OnViewClosed()
		{
			this.UnloadViewScene();
			this.TryCallEndCallback();
		}

		private void LoadViewScene()
		{
			SceneManager.LoadSceneAsync("UI_ADD_PurchaseFeedback", 1);
		}

		private void UnloadViewScene()
		{
			SceneManager.UnloadSceneAsync("UI_ADD_PurchaseFeedback");
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PurchaseFeedbackComponent));

		private IPurchaseFeedbackView _purchaseFeedbackView;

		private PurchaseFeedbackView.PurchasedFeedbackViewData _purchasedFeedbackViewData;

		private Action _onTryToShowEnd;
	}
}
