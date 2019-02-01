using System;
using System.Collections.Generic;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class PassiveBombInfo : GadgetInfo
	{
		public override Type GadgetType()
		{
			return typeof(PassiveBomb);
		}

		public override string[] GetStatStrings()
		{
			return new string[]
			{
				"Buff 0",
				"Buff 1",
				"Buff 2",
				"Duration"
			};
		}

		public override List<float> GetStats(int index)
		{
			switch (index)
			{
			case 0:
				return base.GetStatListModifierAmount(this.Buff, 0);
			case 1:
				return base.GetStatListModifierAmount(this.Buff, 1);
			case 2:
				return base.GetStatListModifierAmount(this.Buff, 2);
			case 3:
				return base.GetStatListModifier(this.BombLifeTime, this.BombLifeTimeUpgrade);
			default:
				return base.GetStats(index);
			}
		}

		public override ModifierInfo GetInfo(int index)
		{
			if (index == 0)
			{
				return base.GetInfo(this.Buff, 0);
			}
			if (index == 1)
			{
				return base.GetInfo(this.Buff, 1);
			}
			if (index != 2)
			{
				return base.GetInfo(index);
			}
			return base.GetInfo(this.Buff, 2);
		}

		public FXInfo BombEffect;

		public ModifierInfo[] Buff;

		public float BombLifeTime;

		public string BombLifeTimeUpgrade;

		public float BombStackLifeTime;

		public string BombStackLifeTimeUpgrade;
	}
}
