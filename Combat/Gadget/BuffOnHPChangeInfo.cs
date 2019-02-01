using System;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class BuffOnHPChangeInfo : GadgetInfo
	{
		public override Type GadgetType()
		{
			return typeof(BuffOnHPChange);
		}

		public float MinHPpct;

		public string MinHPUpgrade;

		public float MaxHPpct;

		public string MaxHPUpgrade;

		public bool Reverse;
	}
}
