using System;
using HeavyMetalMachines.VFX;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class MortarWarningWithStacksInfo : MortarWarningInfo
	{
		public override Type GadgetType()
		{
			return typeof(MortarWarningWithStacks);
		}

		public int NextStackTickMillis;

		public int StackMaxCount;

		public float StackLifeTime;

		public float ExplosionRange;

		public string ExplosionRangeUpgrade;

		public float[] ExplosionMultipliers;

		public ModifierFeedbackInfo[] StackFeedback;

		public FXInfo StackEffect;

		public ModifierInfo[] StackModifiers;

		public FXInfo ExplosionEffect;

		public ModifierInfo[] ExplosionModifiers;

		public ModifierInfo[] ExplosionMaxChargedModifiers;
	}
}
