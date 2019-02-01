using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class CombatTextSettings : GameHubScriptableObject
	{
		[Header("[Offsets]")]
		public int TargetOffsetFromCenterX;

		public int TargetOffsetFromCenterY;

		[Header("[Stack Time]")]
		public float StackTimeInSec;

		[Header("[Animations]")]
		public CombatTextAnimation[] DamageAnimations;

		public CombatTextAnimation[] HealAnimations;

		[Header("[Pulse Info - RangeValue(n) < RangeValue(n+1)]")]
		public CombatPulseInfo[] CombatPulseInfoList;

		[Header("[Type Settings]")]
		public CombatTextSettings.TypeSettings[] Types;

		[Serializable]
		public struct TypeSettings
		{
			public CombatTextType Type;

			public Color Color;

			public float Threshold;
		}
	}
}
