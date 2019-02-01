using System;
using System.Collections.Generic;
using HeavyMetalMachines.VFX;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class ItemGadgetSelfMultipleEffectsInfo : GadgetInfo
	{
		public override Type GadgetType()
		{
			return typeof(ItemGadgetSelfMulfipleEffects);
		}

		public override string[] GetStatStrings()
		{
			List<string> list = new List<string>();
			list.Add("Cooldown");
			list.Add("EP");
			for (int i = 0; i < this.MultipleInfos.Length; i++)
			{
				for (int j = 0; j < this.MultipleInfos[i].Modifiers.Length; j++)
				{
					list.Add(i + " Damage " + j);
					list.Add(string.Concat(new object[]
					{
						i,
						" Damage ",
						j,
						" Duration"
					}));
				}
				list.Add(i + " Duration");
				list.Add(i + " Range");
			}
			return list.ToArray();
		}

		public override List<float> GetStats(int index)
		{
			if (index == 0)
			{
				return base.GetStatListModifier(this.Cooldown, this.CooldownUpgrade);
			}
			if (index != 1)
			{
				index -= 2;
				int num = 0;
				for (int i = 0; i < this.MultipleInfos.Length; i++)
				{
					for (int j = 0; j < this.MultipleInfos[i].Modifiers.Length; j++)
					{
						if (num++ == index)
						{
							return base.GetStatListModifierAmount(this.MultipleInfos[i].Modifiers, j);
						}
						if (num++ == index)
						{
							return base.GetStatListModifierLifeTime(this.MultipleInfos[i].Modifiers, j);
						}
					}
					if (num++ == index)
					{
						return base.GetStatListSingleValue(this.MultipleInfos[i].LifeTime);
					}
					if (num++ == index)
					{
						return base.GetStatListSingleValue(this.MultipleInfos[i].Range);
					}
				}
				return new List<float>(new float[1]);
			}
			return base.GetStatListModifier((float)this.ActivationCost, this.ActivationCostUpgrade);
		}

		public override ModifierInfo GetInfo(int index)
		{
			if (index != 0 && index != 1)
			{
				index -= 2;
				int num = 0;
				for (int i = 0; i < this.MultipleInfos.Length; i++)
				{
					for (int j = 0; j < this.MultipleInfos[i].Modifiers.Length; j++)
					{
						if (num++ == index)
						{
							return base.GetInfo(this.MultipleInfos[i].Modifiers, j);
						}
						if (num++ == index)
						{
							return base.GetInfo(this.MultipleInfos[i].Modifiers, j);
						}
					}
				}
				return base.GetInfo(index);
			}
			return base.GetInfo(index);
		}

		public ItemGadgetSelfMultipleEffectsInfo.MultipleInfo[] MultipleInfos;

		[Serializable]
		public class MultipleInfo
		{
			public FXInfo Effect;

			public ModifierFeedbackInfo Feedback;

			public ModifierInfo[] Modifiers;

			public int LinkedTo = -1;

			public float LifeTime;

			public float Range;
		}
	}
}
