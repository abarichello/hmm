using System;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class RageInfo : BasicCannonInfo
	{
		public override Type GadgetType()
		{
			return typeof(Rage);
		}

		public float Gadget0HitRatio;

		public string Gadget0HitRatioUpgrade;

		public float Gadget1HitRatio;

		public string Gadget1HitRatioUpgrade;

		public float Gadget2HitRatio;

		public string Gadget2HitRatioUpgrade;

		public float PowerPctRageRatio;

		public float DegenerationRatio;

		public int[] RageValues;
	}
}
