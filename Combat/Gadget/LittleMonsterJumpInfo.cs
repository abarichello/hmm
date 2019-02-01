using System;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class LittleMonsterJumpInfo : FireEffectsInAreaInfo
	{
		public override Type GadgetType()
		{
			return typeof(LittleMonsterJump);
		}

		[Header("LittleMonster Jump")]
		[Tooltip("Max distance from damaged target to activate next jump")]
		public float MultiJumpMaxDistance;

		public string MultiJumpUpgrade;
	}
}
