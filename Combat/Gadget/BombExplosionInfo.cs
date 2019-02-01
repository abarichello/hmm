using System;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class BombExplosionInfo : BasicCannonInfo
	{
		public override Type GadgetType()
		{
			return typeof(BombExplosionGadget);
		}
	}
}
