using System;
using HeavyMetalMachines.VFX;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class BuffCannonInfo : GadgetInfo
	{
		public override Type GadgetType()
		{
			return typeof(BuffCannon);
		}

		public ModifierInfo[] ExtraDamage;

		public float MoveSpeed;

		public FXInfo YellowEffect;

		public ModifierInfo[] YellowDamage;

		public FXInfo CollisionEffect;

		public ModifierInfo[] CollisionDamage;

		public FXInfo GreenEffect;

		public ModifierInfo[] GreenDamage;

		public ModifierFeedbackInfo AmmoFeedback;

		public ModifierFeedbackInfo YellowFeedback;

		public ModifierFeedbackInfo CollisionFeedback;

		public ModifierFeedbackInfo GreenFeedback;
	}
}
