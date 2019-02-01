using System;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class IndestructibleEffectCannonInfo : BasicCannonInfo
	{
		public override Type GadgetType()
		{
			return typeof(IndestructibleEffectCannon);
		}
	}
}
