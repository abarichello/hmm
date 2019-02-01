using System;
using ClientAPI;
using ClientAPI.Objects;
using Pocketverse;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HeavyMetalMachines.PurchaseFeedback
{
	[CreateAssetMenu(menuName = "ScriptableObject/Temp/PurchaseFeedbackComponent")]
	public class PurchaseFeedbackComponent : GameHubScriptableObject, IPurchaseFeedbackComponent
	{
		public void TryToShowBoughtHardCurrency(System.Action onTryToShowEnd)
		{
			this._onTryToShowEnd = onTryToShowEnd;
			GameHubScriptableObject.Hub.ClientApi.billing.GetMyProductsNotSaw(null, new SwordfishClientApi.ParameterizedCallback<UserHardCurrencyProduct[]>(this.OnGetMyProductsNotSaw), new SwordfishClientApi.ErrorCallback(this.OnGetMyProductsNotSawError));
		}

		private void OnGetMyProductsNotSaw(object state, UserHardCurrencyProduct[] userHardCurrencyProducts)
		{
			if (userHardCurrencyProducts.Length == 0)
			{
				this.TryCallEndCallback();
				return;
			}
			UserHardCurrencyProduct hardCurrencyProduct = userHardCurrencyProducts[0];
			this.CachePurchasedFeedbackViewdata(hardCurrencyProduct);
			this.LoadViewScene();
			this.UpdateUserSawProductList(hardCurrencyProduct);
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
			}, delegate(object obj, Exception exception)
			{
				PurchaseFeedbackComponent.Log.ErrorFormat("Error on LoadItems. Swordfish UpdateUserSawProductList - exception: {0}", new object[]
				{
					exception
				});
			});
		}

		private void CachePurchasedFeedbackViewdata(UserHardCurrencyProduct hardCurrencyProduct)
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
			this._purchasedFeedbackViewData.UserCurrentCurrencyValue = GameHubScriptableObject.Hub.Store.HardCurrency.ToString("0");
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
			GameHubScriptableObject.Hub.GuiScripts.TopMenu.PlayHardCoinsUpdateAnimation();
			this.UnloadViewScene();
			this.TryCallEndCallback();
		}

		private void LoadViewScene()
		{
			SceneManager.LoadSceneAsync("UI_ADD_PurchaseFeedback", LoadSceneMode.Additive);
		}

		private void UnloadViewScene()
		{
			SceneManager.UnloadSceneAsync("UI_ADD_PurchaseFeedback");
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PurchaseFeedbackComponent));

		private IPurchaseFeedbackView _purchaseFeedbackView;

		private PurchaseFeedbackView.PurchasedFeedbackViewData _purchasedFeedbackViewData;

		private System.Action _onTryToShowEnd;
	}
}
