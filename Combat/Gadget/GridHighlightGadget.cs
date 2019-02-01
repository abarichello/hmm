using System;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class GridHighlightGadget : BasicCannon
	{
		public GridHighlightGadgetInfo MyInfo
		{
			get
			{
				return base.Info as GridHighlightGadgetInfo;
			}
		}
	}
}
