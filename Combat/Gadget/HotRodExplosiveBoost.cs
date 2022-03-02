using System;
using HeavyMetalMachines.Event;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class HotRodExplosiveBoost : BasicTrailDropper
	{
		private HotRodExplosiveBoostInfo MyInfo
		{
			get
			{
				return base.Info as HotRodExplosiveBoostInfo;
			}
		}

		protected override int FireExtraGadget(FXInfo effect, ModifierData[] modifierData, ModifierData[] modifierData2, Action<EffectEvent> customizeData)
		{
			if (this._lavaEffectId != -1)
			{
				return -1;
			}
			int num = base.FireExtraGadget(effect, modifierData, modifierData2, customizeData);
			if (this._extraEffectId == -1)
			{
				this._extraEffectId = num;
			}
			return num;
		}

		protected override void OnMyEffectDestroyed(DestroyEffectMessage evt)
		{
			if (this._lavaEffectId != -1 || !this._leaveLava.BoolGet())
			{
				return;
			}
			base.OnMyEffectDestroyed(evt);
			if (evt.RemoveData.TargetEventId == this._extraEffectId)
			{
				this._lavaEffectId = this.FireExtraGadget(this.MyInfo.LavaFX, ModifierData.CreateData(this.MyInfo.LavaModifiers, this.MyInfo), null, delegate(EffectEvent data)
				{
					data.Origin = evt.RemoveData.Origin;
					data.LifeTime = this.MyInfo.LavaLifetime;
				});
			}
		}

		protected override int FireGadget()
		{
			this._extraEffectId = -1;
			this._lavaEffectId = -1;
			return base.FireGadget();
		}

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			this._leaveLava = new Upgradeable(this.MyInfo.LavaUpgrade, this.MyInfo.LeaveLavaAfterExplosion, this.MyInfo.UpgradesValues);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this._leaveLava.SetLevel(upgradeName, level);
		}

		private int _extraEffectId = -1;

		private int _lavaEffectId = -1;

		private Upgradeable _leaveLava;
	}
}
