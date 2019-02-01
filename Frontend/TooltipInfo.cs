using System;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class TooltipInfo
	{
		public TooltipInfo(TooltipInfo.TooltipType type, TooltipInfo.DescriptionSummaryType summaryType, PreferredDirection arrowAnchor, Sprite iconSprite, string spriteName, string title, string rechargeDescription, string descriptionFull, string cooldownDescription, string price, string hintDescription, Vector3 position, string simpleText)
		{
			this.Type = type;
			this.SummaryType = summaryType;
			this.ArrowAnchor = arrowAnchor;
			this.IconSprite = iconSprite;
			this.SpriteName = spriteName;
			this.Title = title;
			this.RechargeDescription = rechargeDescription;
			this.DescriptionFull = descriptionFull;
			this.CooldownDescription = cooldownDescription;
			this.Price = price;
			this.HintDescription = hintDescription;
			this.Position = position;
			this.SimpleText = simpleText;
		}

		public readonly TooltipInfo.TooltipType Type;

		public readonly TooltipInfo.DescriptionSummaryType SummaryType;

		public readonly PreferredDirection ArrowAnchor;

		public readonly Sprite IconSprite;

		public readonly string SpriteName;

		public readonly string SimpleText;

		public readonly string Title;

		public readonly string RechargeDescription;

		public readonly string DescriptionFull;

		public readonly string CooldownDescription;

		public readonly string Price;

		public readonly string HintDescription;

		public readonly Vector3 Position;

		public enum TooltipType
		{
			Normal,
			GadgetShopSell,
			GadgetShopRevert,
			GadgetShopNoFunds,
			GadgetShopBlock,
			SimpleText
		}

		public enum DescriptionSummaryType
		{
			None,
			Passive,
			UseTargeting
		}
	}
}
