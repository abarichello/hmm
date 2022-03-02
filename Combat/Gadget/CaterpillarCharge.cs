using System;
using System.Collections.Generic;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class CaterpillarCharge : BasicLink, TriggerEnterCallback.ITriggerEnterCallbackListener, CollisionCallback.ICollisionCallbackListener, TriggerExitCallback.ITriggerExitCallbackListener
	{
		public CaterpillarChargeInfo ChargeInfo
		{
			get
			{
				return base.Info as CaterpillarChargeInfo;
			}
		}

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			this._grabbedModifiers = ModifierData.CreateData(this.ChargeInfo.GrabbedModifiers, this.ChargeInfo);
			this._grabbedEndByCollisionModifiers = ModifierData.CreateData(this.ChargeInfo.GrabbedEndByCollisionModifiers, this.ChargeInfo);
			this._grabbedEndByLifetimeModifiers = ModifierData.CreateData(this.ChargeInfo.GrabbedEndByLifetimeModifiers, this.ChargeInfo);
		}

		protected override int FireGadget()
		{
			this._currentChargeEffect = base.FireGadget();
			return this._currentChargeEffect;
		}

		public override void OnTriggerEnterCallback(TriggerEnterCallback evt)
		{
			if (evt.FX != null && evt.FX.Data.EffectInfo == this.ChargeInfo.Effect)
			{
				this.ApplyDamageByCollision(evt.Combat);
				this.DestroyMyEffects();
				CaterpillarCharge.DestroyOtherEffect(evt);
			}
			else if (evt.FX == null && !this._carriedsEffects.ContainsKey(evt.Combat) && BaseFX.CheckHit(this.Combat, evt.Combat, this.ChargeInfo.GrabbedEffect))
			{
				this._carriedsEffects.Add(evt.Combat, this.FireExtraGadget(this.ChargeInfo.GrabbedEffect, this._grabbedModifiers, null, delegate(EffectEvent data)
				{
					data.TargetId = evt.Combat.Id.ObjId;
					data.Origin = evt.Combat.Transform.position;
					data.LifeTime = this.LifeTime;
				}));
			}
		}

		private static void DestroyOtherEffect(TriggerEnterCallback evt)
		{
			BaseFX baseFx = GameHubBehaviour.Hub.Effects.GetBaseFx(evt.FX.EventId);
			if (baseFx != null)
			{
				baseFx.TriggerDefaultDestroy(evt.Combat.Id.ObjId);
			}
		}

		public void OnTriggerExitCallback(TriggerExitCallback evt)
		{
			if (this._carriedsEffects.ContainsKey(evt.Combat))
			{
				BaseFX baseFx = GameHubBehaviour.Hub.Effects.GetBaseFx(this._carriedsEffects[evt.Combat]);
				if (baseFx != null)
				{
					baseFx.TriggerDefaultDestroy(evt.Combat.Id.ObjId);
				}
				this._carriedsEffects.Remove(evt.Combat);
			}
		}

		public void OnCollisionCallback(CollisionCallback evt)
		{
			if (evt.IsScenery && this._currentChargeEffect != -1 && Vector3.Dot(evt.CollisionNormal, this.Combat.Transform.forward) < 0f && (evt.Combat == this.Combat || this._carriedsEffects.ContainsKey(evt.Combat)))
			{
				this.ApplyDamageByCollision(null);
				this.DestroyMyEffects();
			}
		}

		public override void OnDestroyEffect(DestroyEffectMessage evt)
		{
			base.OnDestroyEffect(evt);
			if (evt.RemoveData.DestroyReason == BaseFX.EDestroyReason.Lifetime && evt.EffectData.EffectInfo.EffectId != this.ChargeInfo.WarmupEffect.EffectId)
			{
				CaterpillarCharge.<OnDestroyEffect>c__AnonStorey1 <OnDestroyEffect>c__AnonStorey = new CaterpillarCharge.<OnDestroyEffect>c__AnonStorey1();
				<OnDestroyEffect>c__AnonStorey.$this = this;
				<OnDestroyEffect>c__AnonStorey.combats = new List<CombatObject>(this._carriedsEffects.Keys);
				int i;
				for (i = 0; i < <OnDestroyEffect>c__AnonStorey.combats.Count; i++)
				{
					this.FireExtraGadget(this.ChargeInfo.GrabbedEndByLifetimeEffect, this._grabbedEndByLifetimeModifiers, null, delegate(EffectEvent data)
					{
						data.TargetId = <OnDestroyEffect>c__AnonStorey.combats[i].Id.ObjId;
						data.Origin = <OnDestroyEffect>c__AnonStorey.combats[i].Transform.position;
					});
				}
				this.FireExtraGadget(this.ChargeInfo.GrabbedEndByLifetimeEffect, this._grabbedEndByLifetimeModifiers, null, delegate(EffectEvent data)
				{
					data.TargetId = <OnDestroyEffect>c__AnonStorey.$this.Combat.Id.ObjId;
					data.Origin = <OnDestroyEffect>c__AnonStorey.$this.Combat.Transform.position;
				});
				this.DestroyMyEffects();
			}
		}

		public override void OnObjectUnspawned(UnspawnEvent evt)
		{
			base.OnObjectUnspawned(evt);
			this.DestroyMyEffects();
		}

		private void ApplyDamageByCollision(CombatObject ignore = null)
		{
			CaterpillarCharge.<ApplyDamageByCollision>c__AnonStorey3 <ApplyDamageByCollision>c__AnonStorey = new CaterpillarCharge.<ApplyDamageByCollision>c__AnonStorey3();
			<ApplyDamageByCollision>c__AnonStorey.$this = this;
			<ApplyDamageByCollision>c__AnonStorey.combats = new List<CombatObject>(this._carriedsEffects.Keys);
			int i;
			for (i = 0; i < <ApplyDamageByCollision>c__AnonStorey.combats.Count; i++)
			{
				if (!(<ApplyDamageByCollision>c__AnonStorey.combats[i] == ignore))
				{
					this.FireExtraGadget(this.ChargeInfo.GrabbedEndByCollisionEffect, this._grabbedEndByCollisionModifiers, null, delegate(EffectEvent data)
					{
						data.TargetId = <ApplyDamageByCollision>c__AnonStorey.combats[i].Id.ObjId;
						data.Origin = <ApplyDamageByCollision>c__AnonStorey.combats[i].Transform.position;
					});
				}
			}
			this.FireExtraGadget(this.ChargeInfo.GrabbedEndByCollisionEffect, this._grabbedEndByCollisionModifiers, null, delegate(EffectEvent data)
			{
				data.TargetId = <ApplyDamageByCollision>c__AnonStorey.$this.Combat.Id.ObjId;
				data.Origin = <ApplyDamageByCollision>c__AnonStorey.$this.Combat.Transform.position;
			});
		}

		private void DestroyMyEffects()
		{
			BaseFX baseFx = GameHubBehaviour.Hub.Effects.GetBaseFx(this._currentChargeEffect);
			if (baseFx != null)
			{
				baseFx.TriggerDefaultDestroy(this.Combat.Id.ObjId);
			}
			List<CombatObject> list = new List<CombatObject>(this._carriedsEffects.Keys);
			for (int i = 0; i < list.Count; i++)
			{
				BaseFX baseFx2 = GameHubBehaviour.Hub.Effects.GetBaseFx(this._carriedsEffects[list[i]]);
				if (baseFx2 != null)
				{
					baseFx2.TriggerDefaultDestroy(list[i].Id.ObjId);
				}
			}
			this._currentChargeEffect = -1;
			this._carriedsEffects.Clear();
		}

		public static readonly BitLogger Log = new BitLogger(typeof(CaterpillarCharge));

		private int _currentChargeEffect = -1;

		private Dictionary<CombatObject, int> _carriedsEffects = new Dictionary<CombatObject, int>();

		private ModifierData[] _grabbedModifiers;

		private ModifierData[] _grabbedEndByCollisionModifiers;

		private ModifierData[] _grabbedEndByLifetimeModifiers;
	}
}
