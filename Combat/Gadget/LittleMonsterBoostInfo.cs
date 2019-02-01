using System;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class LittleMonsterBoostInfo : ReflectiveProjectileInfo
	{
		public override Type GadgetType()
		{
			return typeof(LittleMonsterBoost);
		}

		[Header("LittleMonster Boost")]
		public FXInfo SuperSuctionEffect;

		public ModifierInfo[] SuperSuctionModifiers;

		public bool ActivateSuperSuction;

		public string ActivateSuperSuctionUpgrade;
	}
}
