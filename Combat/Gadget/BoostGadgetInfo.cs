using System;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class BoostGadgetInfo : BasicCannonInfo
	{
		public override Type GadgetType()
		{
			return typeof(BoostGadget);
		}

		[Header("[BoostGadget]")]
		public float[] StagesLifetime;

		public BoostGadgetInfo.ModifierList[] StagesModifiers;

		public FXInfo JammedEffect;

		public FXInfo DriftingEffect;

		public float ZSpeedToStartBoostEffect = 0.3f;

		[Serializable]
		public class ModifierList
		{
			public ModifierInfo[] Modifiers;
		}
	}
}
