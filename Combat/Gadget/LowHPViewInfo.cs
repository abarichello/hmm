using System;
using System.Collections.Generic;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class LowHPViewInfo : GadgetInfo
	{
		public override Type GadgetType()
		{
			return typeof(LowHPView);
		}

		public override string[] GetStatStrings()
		{
			return new string[]
			{
				"Hp Percent"
			};
		}

		public override List<float> GetStats(int index)
		{
			if (index != 0)
			{
				return base.GetStats(index);
			}
			return base.GetStatListModifier(this.HPPercent, this.HPPercentUpgrade);
		}

		public float HPPercent;

		public string HPPercentUpgrade;

		public FXInfo RevealEffect;

		public ModifierInfo[] RevealStatus;
	}
}
