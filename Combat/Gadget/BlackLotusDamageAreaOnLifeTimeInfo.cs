using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class BlackLotusDamageAreaOnLifeTimeInfo : GadgetInfo
	{
		public override Type GadgetType()
		{
			return typeof(BlackLotusDamageAreaOnLifeTime);
		}

		public override string[] GetStatStrings()
		{
			return new string[]
			{
				"CastRange",
				"AreaRange",
				"AreaModifiers 0",
				"AreaModifiers 0 Duration",
				"AreaModifiers 1",
				"AreaModifiers 1 Duration",
				"AreaModifiers 2",
				"AreaModifiers 2 Duration",
				"LifeTime",
				"Cooldown",
				"EP"
			};
		}

		public override List<float> GetStats(int index)
		{
			switch (index)
			{
			case 0:
				return base.GetStatListSingleValue(this.CastRange);
			case 1:
				return base.GetStatListSingleValue(this.AreaRange);
			case 2:
				return base.GetStatListModifierAmount(this.AreaModifiers, 0);
			case 3:
				return base.GetStatListModifierLifeTime(this.AreaModifiers, 0);
			case 4:
				return base.GetStatListModifierAmount(this.AreaModifiers, 1);
			case 5:
				return base.GetStatListModifierLifeTime(this.AreaModifiers, 1);
			case 6:
				return base.GetStatListModifierAmount(this.AreaModifiers, 2);
			case 7:
				return base.GetStatListModifierLifeTime(this.AreaModifiers, 2);
			case 8:
				return base.GetStatListModifier(this.LifeTime, this.LifeTimeUpgrade);
			case 9:
				return base.GetStatListModifier(this.Cooldown, this.CooldownUpgrade);
			case 10:
				return base.GetStatListModifier((float)this.ActivationCost, this.ActivationCostUpgrade);
			default:
				return base.GetStats(index);
			}
		}

		public override ModifierInfo GetInfo(int index)
		{
			switch (index)
			{
			case 2:
				return base.GetInfo(this.AreaModifiers, 0);
			case 3:
				return base.GetInfo(this.AreaModifiers, 0);
			case 4:
				return base.GetInfo(this.AreaModifiers, 1);
			case 5:
				return base.GetInfo(this.AreaModifiers, 1);
			case 6:
				return base.GetInfo(this.AreaModifiers, 2);
			case 7:
				return base.GetInfo(this.AreaModifiers, 2);
			default:
				return base.GetInfo(index);
			}
		}

		public float CastRange;

		public float AreaRange;

		public ModifierInfo[] AreaModifiers;

		public FXInfo AreaEffect;

		public Texture CastTexture;

		public Texture AreaTexture;
	}
}
