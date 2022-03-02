using System;
using HeavyMetalMachines.Combat.Gadget;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HudGarageShopSettings : GameHubScriptableObject
	{
		public bool GetGadgetMaxQuantity(GadgetSlot slot, out int max)
		{
			switch (slot)
			{
			case GadgetSlot.CustomGadget0:
				max = this.Gadget0MaxQuantity;
				return true;
			case GadgetSlot.CustomGadget1:
				max = this.Gadget1MaxQuantity;
				return true;
			case GadgetSlot.CustomGadget2:
				max = this.UltimateMaxQuantity;
				return true;
			case GadgetSlot.GenericGadget:
				max = this.GenericMaxQuantity;
				return true;
			}
			max = 0;
			return false;
		}

		[Header("[Refund modifier: new_refund = cost * modifier]")]
		[Range(0f, 1f)]
		public float NonBuybackRefundModifier;

		[Header("[Gadget Max Quantity]")]
		public int Gadget0MaxQuantity;

		public int Gadget1MaxQuantity;

		public int UltimateMaxQuantity;

		public int GenericMaxQuantity;

		[Header("[Gadget Title Label Colors]")]
		public Color GadgetTitleLabelNormalColor;

		public Color GadgetTitleLabelFullColor;

		[Header("[Audio]")]
		public AudioEventAsset audioSnapshot;

		public AudioEventAsset OpenWindowFmodAsset;

		public AudioEventAsset CloseWindowFmodAsset;

		public AudioEventAsset BuyAudioFmodAsset;

		public AudioEventAsset DeniedByBalanceAudioFmodAsset;

		public AudioEventAsset DeniedByLevelAudioFmodAsset;

		public AudioEventAsset GadgetFullAudioFmodAsset;

		public AudioEventAsset SellAudioFmodAsset;

		public AudioEventAsset RevertAudioFmodAsset;
	}
}
