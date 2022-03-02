using System;
using HeavyMetalMachines.Customizations.Skins;
using UnityEngine;

namespace HeavyMetalMachines.Customization.Skins
{
	[Serializable]
	public struct SkinRarityInfo
	{
		public TierKind TierKind;

		public string LongDraftName;

		public string ShortDraftName;

		public Color TierColor;

		public Sprite TierBorderSprite;
	}
}
