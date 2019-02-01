using System;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class MultipleEffectsAngleCannonInfo : BasicCannonInfo
	{
		public override Type GadgetType()
		{
			return typeof(MultipleEffectsAngleCannon);
		}

		public int Angle;

		public string AngleUpgrade;

		public float DrainLifePctFromTarget;

		public string DrainLifePctFromTargetUpgrade;
	}
}
