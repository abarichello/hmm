using System;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class BuffOnSpeedInfo : BasicCannonInfo
	{
		public override Type GadgetType()
		{
			return typeof(BuffOnSpeed);
		}

		public float ActivationSpeed;

		public string ActivationSpeedUpgrade;

		public float ActivationTime;

		public string ActivationTimeUpgrade;

		public float FullBarTime;
	}
}
