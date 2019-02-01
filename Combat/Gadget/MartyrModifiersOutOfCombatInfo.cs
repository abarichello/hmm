using System;
using System.Collections.Generic;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class MartyrModifiersOutOfCombatInfo : GadgetInfo
	{
		public override Type GadgetType()
		{
			return typeof(MartyrModifiersOutOfCombat);
		}

		public override string[] GetStatStrings()
		{
			return new string[]
			{
				"Modifiers 0",
				"Cooldown",
				"EP"
			};
		}

		public override List<float> GetStats(int index)
		{
			if (index == 0)
			{
				return base.GetStatListModifierAmount(this.Modifiers, 0);
			}
			if (index == 1)
			{
				return base.GetStatListModifier(this.Cooldown, this.CooldownUpgrade);
			}
			if (index != 2)
			{
				return base.GetStats(index);
			}
			return base.GetStatListModifier((float)this.ActivationCost, this.ActivationCostUpgrade);
		}

		public override ModifierInfo GetInfo(int index)
		{
			if (index != 0)
			{
				return base.GetInfo(index);
			}
			return base.GetInfo(this.Modifiers, 0);
		}

		public FXInfo ListenerEffect;

		public FXInfo ModifiersEffect;

		public ModifierInfo[] Modifiers;
	}
}
