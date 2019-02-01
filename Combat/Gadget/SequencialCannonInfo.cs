using System;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class SequencialCannonInfo : MultipleEffectsCannonInfo
	{
		public override Type GadgetType()
		{
			return typeof(SequencialCannon);
		}
	}
}
