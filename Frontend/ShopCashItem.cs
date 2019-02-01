using System;
using HeavyMetalMachines.VFX;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class ShopCashItem : GameHubBehaviour
	{
		[HideInInspector]
		public int SerialHardCurrencyProductId;

		public UI2DSprite BackgroundSprite;

		public HMMUI2DDynamicSprite ItemSprite;

		public UILabel SoonLabel;

		public GameObject HardValueGroupGameObject;

		public UILabel HardQuantityLabel;

		public UILabel HardBonusLabel;

		public UILabel HardTotalLabel;

		public GameObject PriceValueGroupGameObject;

		public UILabel FinalPriceLabel;

		public UILabel OldPriceLabel;

		public UI2DSprite OldPriceCutLineSprite;

		public GameObject DiscountGroupGameObject;

		public UILabel DiscountLabel;

		public BoxCollider ButtonBoxCollider;

		public GUIEventListener GuiEventListener;
	}
}
