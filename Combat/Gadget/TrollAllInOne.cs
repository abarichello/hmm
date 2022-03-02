using System;
using HeavyMetalMachines.Event;
using Pocketverse;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class TrollAllInOne : BasicTrailDropper, TriggerEnterCallback.ITriggerEnterCallbackListener
	{
		private TrollAllInOneInfo MyInfo
		{
			get
			{
				return base.Info as TrollAllInOneInfo;
			}
		}

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			this.StartTrailWithFireGadget = false;
			this.Damage3Modifier = ModifierData.CreateData(this.MyInfo.Damage3Modifier, this.MyInfo);
			this.UseEffect3 = new Upgradeable(this.MyInfo.UseEffect3Upgrade, this.MyInfo.UseEffect3, this.MyInfo.UpgradesValues);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this.Damage3Modifier.SetLevel(upgradeName, level);
			this.UseEffect3.SetLevel(upgradeName, level);
		}

		protected override int FireExtraGadget()
		{
			this.missileEffectID = base.FireExtraGadget();
			return this.missileEffectID;
		}

		protected override void InnerOnDestroyEffect(DestroyEffectMessage evt)
		{
			base.InnerOnDestroyEffect(evt);
			if (!this.UseEffect3.BoolGet() || this.missileEffectID != evt.RemoveData.TargetEventId)
			{
				return;
			}
			EffectEvent effectEvent = base.GetEffectEvent(this.MyInfo.Damage3Effect);
			CombatObject combat = CombatRef.GetCombat(evt.RemoveData.SrvEffect.Target.ObjId);
			if (!combat.IsAlive())
			{
				return;
			}
			effectEvent.Origin = GadgetBehaviour.DummyPosition(combat, this.MyInfo.Damage3Effect);
			effectEvent.LifeTime = this.MyInfo.Effect3LifeTime;
			effectEvent.Target = base.Target;
			effectEvent.TargetId = combat.Id.ObjId;
			effectEvent.Modifiers = ModifierData.CopyData(this.Damage3Modifier);
			base.SetTargetAndDirection(effectEvent);
			int effectID = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
			base.ExistingFiredEffectsAdd(effectID);
		}

		protected override int FireExtraGadgetOnDeath(DestroyEffectMessage destroyEvt)
		{
			int num = base.FireExtraGadgetOnDeath(destroyEvt);
			if (num == -1)
			{
				return -1;
			}
			if (this.UseTrail.BoolGet())
			{
				CombatObject combat = CombatRef.GetCombat(destroyEvt.RemoveData.TargetId);
				this.TrailDropper.FireCannon(combat.transform.position, combat.transform.forward, num, combat);
			}
			return num;
		}

		public override void Clear()
		{
			base.Clear();
			this.missileEffectID = -1;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(TrollAllInOne));

		private int missileEffectID;

		protected ModifierData[] Damage3Modifier;

		protected Upgradeable UseEffect3;
	}
}
