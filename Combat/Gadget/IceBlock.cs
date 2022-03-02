using System;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class IceBlock : BasicNewEffectOnEffectDeathCannon
	{
		private IceBlockInfo IceBlockInfo
		{
			get
			{
				return base.Info as IceBlockInfo;
			}
		}

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			this.OnWarmupLifeTimeEndDamage = ModifierData.CreateData(this.IceBlockInfo.OnWarmupLifeTimeEndDamage, this.IceBlockInfo);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this.OnWarmupLifeTimeEndDamage.SetLevel(upgradeName, level);
		}

		protected override int FireGadget()
		{
			if (base.CannonInfo.UseLastWarmupPosition)
			{
				this.Origin = this.LastWarmupPosition;
			}
			else
			{
				this.Origin = GadgetBehaviour.DummyPosition(this.Combat, base.CannonInfo.Effect);
			}
			ModifierData[] modifier = ModifierData.CreateConvoluted(this._damage, this.CurrentHeat);
			return this.FireGadget(base.CannonInfo.Effect, modifier, this.Origin);
		}

		public override void OnDestroyEffect(DestroyEffectMessage evt)
		{
			base.OnDestroyEffect(evt);
			if (evt.RemoveData.TargetEventId == this.LastWarmupId && evt.RemoveData.DestroyReason == BaseFX.EDestroyReason.Lifetime)
			{
				this.Origin = evt.RemoveData.Origin;
				this.FireGadget(this.IceBlockInfo.OnWarmupLifeTimeEndEffect, this.OnWarmupLifeTimeEndDamage, evt.RemoveData.Origin);
			}
		}

		protected ModifierData[] OnWarmupLifeTimeEndDamage;
	}
}
