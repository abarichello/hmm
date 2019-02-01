using System;
using System.Collections.Generic;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class BasicAttackInfo : GadgetInfo
	{
		public new ModifierInfo Damage
		{
			get
			{
				return this.Modifiers[0];
			}
		}

		public override Type GadgetType()
		{
			return typeof(BasicAttack);
		}

		public override string[] GetStatStrings()
		{
			return new string[]
			{
				"Damage",
				"Fire Rate"
			};
		}

		public override List<float> GetStats(int index)
		{
			if (index == 0)
			{
				return base.GetStatListModifier(this.Damage.Amount, this.Damage.AmountUpgrade);
			}
			if (index != 1)
			{
				return base.GetStats(index);
			}
			List<float> list = new List<float>(base.GetStatListModifier(0f, this.CooldownUpgrade));
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] > 0f)
				{
					list[i] = 1f / list[i];
				}
				else
				{
					list[i] = 1f / this.Cooldown;
				}
			}
			return list;
		}

		public override ModifierInfo GetInfo(int index)
		{
			if (index != 0)
			{
				return base.GetInfo(index);
			}
			return this.Damage;
		}

		public ModifierInfo[] Modifiers;

		public float MoveSpeed;

		public string MoveSpeedUpgrade;
	}
}
