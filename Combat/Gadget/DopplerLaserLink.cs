using System;
using HeavyMetalMachines.Event;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class DopplerLaserLink : DamageAngleFirstEnemy
	{
		public new DopplerLaserLinkInfo MyInfo
		{
			get
			{
				return base.Info as DopplerLaserLinkInfo;
			}
		}

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			this._useInfiniteRay = new Upgradeable(this.MyInfo.InfiniteRayUpgrade, this.MyInfo.UseInfiniteRay, info.UpgradesValues);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this._useInfiniteRay.SetLevel(upgradeName, level);
		}

		protected override int FireExtraGadget(FXInfo effect, ModifierData[] modifierData, ModifierData[] modifierData2, Action<EffectEvent> customizeData)
		{
			FXInfo effect2 = effect;
			if (this._useInfiniteRay.BoolGet() && effect.EffectId == base.Info.ExtraEffect.EffectId)
			{
				effect2 = this.MyInfo.InfiniteRay;
			}
			return base.FireExtraGadget(effect2, modifierData, modifierData2, customizeData);
		}

		protected override void RunBeforeUpdate()
		{
			base.RunBeforeUpdate();
			if (this._useInfiniteRay.BoolGet() && this._currentTargetCombat != null)
			{
				this._remainingLifetime = base.LifeTime;
			}
		}

		private Upgradeable _useInfiniteRay;
	}
}
