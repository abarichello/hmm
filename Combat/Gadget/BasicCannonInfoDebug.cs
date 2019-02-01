using System;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class BasicCannonInfoDebug : BasicCannonInfo
	{
		public override Type GadgetType()
		{
			return typeof(BasicCannonDebug);
		}
	}
}
