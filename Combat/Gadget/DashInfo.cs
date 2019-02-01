using System;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class DashInfo : BasicCannonInfo
	{
		public override Type GadgetType()
		{
			return typeof(Dash);
		}

		public FXInfo HitEffect;

		public ModifierInfo[] HitModifiers;

		public float HitEffectLifetime;

		public bool ExtraEffectAfterHit;

		public string ExtraEffectAfterHitUpgrade;
	}
}
