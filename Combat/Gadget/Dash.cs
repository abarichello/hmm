using System;
using HeavyMetalMachines.Event;
using Pocketverse;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class Dash : BasicCannon, TriggerEnterCallback.ITriggerEnterCallbackListener
	{
		private DashInfo MyInfo
		{
			get
			{
				return base.Info as DashInfo;
			}
		}

		public override void SetInfo(GadgetInfo gInfo)
		{
			base.SetInfo(gInfo);
			this._effectAfterHit = new Upgradeable(this.MyInfo.ExtraEffectAfterHitUpgrade, this.MyInfo.ExtraEffectAfterHit, this.MyInfo.UpgradesValues);
			this._hitModifiers = ModifierData.CreateData(this.MyInfo.HitModifiers, this.MyInfo);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this._effectAfterHit.SetLevel(upgradeName, level);
			this._hitModifiers.SetLevel(upgradeName, level);
		}

		protected override int FireGadget()
		{
			for (int i = 0; i < this.Upgrades.Length; i++)
			{
				GadgetBehaviour.UpgradeInstance upgradeInstance = this.Upgrades[i];
				base.Downgrade(upgradeInstance.Info.Name);
			}
			return base.FireGadget();
		}

		protected override int FireExtraGadget(FXInfo effect, ModifierData[] modifierData, ModifierData[] modifierData2, Action<EffectEvent> customizeData)
		{
			for (int i = 0; i < this.Upgrades.Length; i++)
			{
				GadgetBehaviour.UpgradeInstance upgradeInstance = this.Upgrades[i];
				base.Upgrade(upgradeInstance.Info.Name);
			}
			return base.FireExtraGadget(effect, modifierData, modifierData2, customizeData);
		}

		public override void OnTriggerEnterCallback(TriggerEnterCallback evt)
		{
			if (this._effectAfterHit.BoolGet())
			{
				EffectEvent effectEvent = base.GetEffectEvent(this.MyInfo.HitEffect);
				effectEvent.Origin = this.Combat.transform.position;
				effectEvent.Target = evt.Combat.transform.position;
				effectEvent.TargetId = evt.Combat.Id.ObjId;
				effectEvent.LifeTime = this.MyInfo.HitEffectLifetime;
				effectEvent.Direction = base.CalcDirection(effectEvent.Origin, effectEvent.Target);
				effectEvent.Modifiers = ModifierData.CopyData(this._hitModifiers);
				GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
			}
		}

		protected Upgradeable _effectAfterHit;

		protected ModifierData[] _hitModifiers;
	}
}
