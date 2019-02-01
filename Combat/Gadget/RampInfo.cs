using System;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class RampInfo : BasicCannonInfo
	{
		public override Type GadgetType()
		{
			return typeof(Ramp);
		}

		public float MinVelocityToJump = 20f;

		public bool IgnoreRelativeVelocityCheck;
	}
}
