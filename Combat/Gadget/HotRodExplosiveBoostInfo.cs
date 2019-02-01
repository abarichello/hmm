using System;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class HotRodExplosiveBoostInfo : BasicTrailDropperInfo
	{
		public override Type GadgetType()
		{
			return typeof(HotRodExplosiveBoost);
		}

		[Header("HotRod Explosive Boost")]
		public bool LeaveLavaAfterExplosion;

		public string LavaUpgrade;

		public FXInfo LavaFX;

		public ModifierInfo[] LavaModifiers;

		public float LavaLifetime;
	}
}
