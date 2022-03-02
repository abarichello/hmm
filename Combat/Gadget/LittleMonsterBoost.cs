using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class LittleMonsterBoost : ReflectiveProjectile
	{
		public LittleMonsterBoostInfo MyInfo
		{
			get
			{
				return base.Info as LittleMonsterBoostInfo;
			}
		}

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			this._suctionModifiers = ModifierData.CreateData(this.MyInfo.SuperSuctionModifiers);
			this._activateSuperSuction = new Upgradeable(this.MyInfo.ActivateSuperSuctionUpgrade, this.MyInfo.ActivateSuperSuction, base.Info.UpgradesValues);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this._activateSuperSuction.SetLevel(upgradeName, level);
		}

		protected override int FireGadget()
		{
			if (this._activateSuperSuction.BoolGet())
			{
				this._reflectionEffectId = base.FireExtraGadget(this.MyInfo.SuperSuctionEffect, this._suctionModifiers, null);
			}
			return base.FireGadget();
		}

		protected override void InnerOnDestroyEffect(DestroyEffectMessage evt)
		{
			BaseFX baseFx = GameHubBehaviour.Hub.Effects.GetBaseFx(this._reflectionEffectId);
			if (baseFx != null)
			{
				baseFx.TriggerDefaultDestroy(-1);
			}
			base.InnerOnDestroyEffect(evt);
		}

		private Upgradeable _activateSuperSuction;

		private ModifierData[] _suctionModifiers;

		private int _reflectionEffectId;
	}
}
