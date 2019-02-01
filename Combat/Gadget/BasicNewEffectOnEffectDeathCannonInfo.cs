using System;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class BasicNewEffectOnEffectDeathCannonInfo : BasicCannonInfo
	{
		public override Type GadgetType()
		{
			return typeof(BasicNewEffectOnEffectDeathCannon);
		}

		public bool FireDeathEffectOnlyIfTargetIdIsValid;

		public bool FireDeathEffectOnWarmupDeath;

		public string FireDeathEffectOnWarmupDeathUpgrade;

		public bool FireDeathEffectOnEffectDeath;

		public string FireDeathEffectOnEffectDeathUpgrade;

		public bool FireDeathEffectOnExtraEffectDeath;

		public string FireDeathEffectOnExtraEffectDeathUpgrade;

		public FXInfo OnDeathEffect;

		public ModifierInfo[] OnDeathDamage;

		public float OnDeathMoveSpeed;

		public string OnDeathMoveSpeedUpgrade;

		public float OnDeathLifeTime;

		public string OnDeathLifeTimeUpgrade;

		public float DrainLifePctFromTarget;

		public string DrainLifePctFromTargetUpgrade;
	}
}
