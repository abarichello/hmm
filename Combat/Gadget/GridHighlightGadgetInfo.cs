using System;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class GridHighlightGadgetInfo : BasicCannonInfo
	{
		public override Type GadgetType()
		{
			return typeof(GridHighlightGadget);
		}
	}
}
