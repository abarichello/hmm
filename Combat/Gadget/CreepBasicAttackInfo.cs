using System;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class CreepBasicAttackInfo : BasicAttackInfo
	{
		public override Type GadgetType()
		{
			return typeof(CreepBasicAttack);
		}
	}
}
