using System;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class BasicAttackAutoInfo : BasicAttackInfo
	{
		public override Type GadgetType()
		{
			return typeof(BasicAttackAuto);
		}

		public int SearchIntervalMillis;
	}
}
