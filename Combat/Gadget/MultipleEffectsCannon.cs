using System;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class MultipleEffectsCannon : BasicNewEffectOnEffectDeathCannon
	{
		private new MultipleEffectsCannonInfo MyInfo
		{
			get
			{
				return base.Info as MultipleEffectsCannonInfo;
			}
		}

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			for (int i = 0; i < this.MyInfo.AllEffects.Length; i++)
			{
				this.MyInfo.AllEffects[i].ModifiersData = ModifierData.CreateData(this.MyInfo.AllEffects[i].Modifiers, this.MyInfo);
			}
			for (int j = 0; j < this.MyInfo.AllExtraEffects.Length; j++)
			{
				this.MyInfo.AllExtraEffects[j].ExtraModifiersData = ModifierData.CreateData(this.MyInfo.AllExtraEffects[j].Modifiers, this.MyInfo);
			}
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			for (int i = 0; i < this.MyInfo.AllEffects.Length; i++)
			{
				this.MyInfo.AllEffects[i].ModifiersData.SetLevel(upgradeName, level);
			}
			for (int j = 0; j < this.MyInfo.AllExtraEffects.Length; j++)
			{
				this.MyInfo.AllExtraEffects[j].ExtraModifiersData.SetLevel(upgradeName, level);
			}
		}

		protected override int FireGadget()
		{
			int num = -1;
			for (int i = 0; i < this.MyInfo.AllEffects.Length; i++)
			{
				FullEffect fullEffect = this.MyInfo.AllEffects[i];
				num = base.FireGadget(fullEffect.Effect, fullEffect.ModifiersData, GadgetBehaviour.DummyPosition(this.Combat, fullEffect.Effect));
				base.ExistingFiredEffectsAdd(num);
			}
			return num;
		}

		protected override int FireExtraGadget()
		{
			int num = -1;
			for (int i = 0; i < this.MyInfo.AllExtraEffects.Length; i++)
			{
				FullEffect fullEffect = this.MyInfo.AllExtraEffects[i];
				num = base.FireExtraGadget(fullEffect.Effect, fullEffect.ExtraModifiersData, null);
				base.ExistingFiredEffectsAdd(num);
			}
			return num;
		}
	}
}
