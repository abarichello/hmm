using System;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class GenkiDamaFollowerInfo : GenkiDamaInfo
	{
		public override Type GadgetType()
		{
			return typeof(GenkiDamaFollower);
		}
	}
}
