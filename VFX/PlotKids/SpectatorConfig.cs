using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX.PlotKids
{
	public class SpectatorConfig : GameHubScriptableObject
	{
		[Header("General Settings")]
		public Color NormalColor;

		public Color DamageColor;

		public Color RepareColor;

		public Color BombColor;

		public Color ControlColor;

		public Color BombAndMetalSectionNormalColor;

		public Color BombAndMetalSectionDisabledColor;

		public Color BombAndMetalSectionPorcentageNormalColor;

		public Color BombAndMetalSectionPorcentageDisabledColor;

		[Header("[Stats]")]
		public SpectatorConfig.StatsCenterGraphicConfig[] StatsCenterGraphicConfigs;

		[Header("[Team]")]
		public Color TeamRedGradientTopColor;

		public Color TeamRedGradientBottonColor;

		public Color TeamRedLabelEffectColor;

		public Color TeamBlueGradientTopColor;

		public Color TeamBlueGradientBottonColor;

		public Color TeamBlueLabelEffectColor;

		[Serializable]
		public struct StatsCenterGraphicConfig
		{
			public Color BaseIconTint;

			public Sprite IconSprite;

			public Sprite GlowSprite;
		}
	}
}
