using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HudLifebarSettings : GameHubScriptableObject
	{
		public Vector2 GetCharacterOffset(CharacterTarget characterTarget)
		{
			for (int i = 0; i < this.CharacterOffsets.Length; i++)
			{
				HudLifebarSettings.HudLifebarCharacterOffset hudLifebarCharacterOffset = this.CharacterOffsets[i];
				if (hudLifebarCharacterOffset.CharacterTarget == characterTarget)
				{
					return hudLifebarCharacterOffset.Offset;
				}
			}
			return this.DefaultCharacterOffset;
		}

		[Header("[Player Text]")]
		public int PlayerNameMaxChars;

		[Header("[Player Name Colors]")]
		public Color PlayerHpTextColor;

		public Color PlayerHpMaxTextColor;

		public Color AllyNameColor;

		public Color EnemyNameColor;

		[Header("[Offset per Character]")]
		public Vector2 CreepOffset;

		public Vector2 DefaultCharacterOffset;

		public HudLifebarSettings.HudLifebarCharacterOffset[] CharacterOffsets;

		[Header("[Lifebar setup]")]
		public float LifebarWidth;

		public float LifebarHeight;

		public int LifebarSlotSpacing;

		public float SlotHp;

		public float HpTimeInSec;

		[Header("[Bleed]")]
		public Color BleedColor;

		public bool BleedOnShieldDamage;

		public float BleedDelayTimeInSec;

		public bool BleedResetDelayOnDamage;

		public float BleedHpReductionOverSec;

		public bool BleedSlowOnDamage;

		public float BleedSlowHpReductionOverSec;

		public float BleedSlowHpIncreaseOverSec;

		public bool BleedSlowDecayResetOnDamage;

		public bool BleedIgnoreDotDamage;

		[Range(0f, 1f)]
		public float BleedMaxDotDamagePct;

		[Header("[Shield]")]
		public Color ShieldColor;

		[Header("[Client Object Pool]")]
		public int PlayerMaxPool;

		public int CreepMaxPool;

		[Header("[BuffOnSpeed]")]
		public Color BuffOnSpeedColor;

		[Serializable]
		public struct HudLifebarCharacterOffset
		{
			public CharacterTarget CharacterTarget;

			public Vector2 Offset;
		}
	}
}
