using System;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class BasicCannonDebug : BasicCannon
	{
		public new BasicCannonInfoDebug CannonInfo
		{
			get
			{
				return base.Info as BasicCannonInfoDebug;
			}
		}

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
		}

		protected override int FireGadget()
		{
			return base.FireGadget();
		}

		protected override int FireExtraGadget()
		{
			return base.FireExtraGadget();
		}
	}
}
