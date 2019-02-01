using System;
using HeavyMetalMachines.VFX;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class ShopPurchasedSkinWindow : GameHubBehaviour
	{
		public void Show(ShopPurchasedSkinWindow.PurchaseInfoType purchaseInfoType, string infoText, string splashSpriteName)
		{
			this._carPurchaseInfo.GroupGameObject.SetActive(false);
			this._cashPurchaseInfo.GroupGameObject.SetActive(false);
			if (purchaseInfoType != ShopPurchasedSkinWindow.PurchaseInfoType.Car)
			{
				if (purchaseInfoType == ShopPurchasedSkinWindow.PurchaseInfoType.Cash)
				{
					this._cashPurchaseInfo.Show(infoText, splashSpriteName);
				}
			}
			else
			{
				this._carPurchaseInfo.Show(infoText, splashSpriteName);
			}
		}

		public void OnOffAreaClick()
		{
			if (this.OnOffAreaClickAction != null)
			{
				this.OnOffAreaClickAction();
			}
		}

		[SerializeField]
		private ShopPurchasedSkinWindow.PurchaseInfo _carPurchaseInfo;

		[SerializeField]
		private ShopPurchasedSkinWindow.PurchaseInfo _cashPurchaseInfo;

		public Action OnOffAreaClickAction;

		public enum PurchaseInfoType
		{
			Car,
			Cash
		}

		[Serializable]
		private struct PurchaseInfo
		{
			public void Show(string infoText, string splashSpriteName)
			{
				this.TextInfoLabel.text = infoText;
				this.SplashSprite.SpriteName = splashSpriteName;
				this.GroupGameObject.SetActive(true);
			}

			public GameObject GroupGameObject;

			public UILabel TextInfoLabel;

			public HMMUI2DDynamicSprite SplashSprite;
		}
	}
}
