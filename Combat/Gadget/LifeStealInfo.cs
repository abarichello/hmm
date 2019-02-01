using System;
using HeavyMetalMachines.VFX;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class LifeStealInfo : BasicCannonInfo
	{
		public override Type GadgetType()
		{
			return typeof(LifeSteal);
		}

		[Header("DrainLifeAurea doesn't affect GadgetOwner, only others")]
		public float DrainLifeAuraRange;

		public string DrainLifeAuraRangeUpgrade;

		public float DrainLifeAuraPct;

		public string DrainLifeAuraPctUpgrade;

		public ModifierFeedbackInfo DrainLifeAuraFeedback;
	}
}
