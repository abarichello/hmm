using System;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class DopplerDoubleCannonInfo : MultipleEffectsCannonInfo
	{
		public override Type GadgetType()
		{
			return typeof(DopplerDoubleCannon);
		}

		[Header("Doppler Double Cannon")]
		public bool DoubleShoot;

		public string DoubleShootUpgrade;

		public bool IncludeWarmup;

		public float DoubleShootWarmupTime;
	}
}
