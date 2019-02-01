using System;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class BasicNewEffectOnEffectDeathCannon : BasicCannon, DamageTakenCallback.IDamageTakenCallbackListener
	{
		public BasicNewEffectOnEffectDeathCannonInfo MyInfo
		{
			get
			{
				return base.Info as BasicNewEffectOnEffectDeathCannonInfo;
			}
		}

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			this.OnDeathDamage = ModifierData.CreateData(this.MyInfo.OnDeathDamage, this.MyInfo);
			this.OnDeathMoveSpeed = new Upgradeable(this.MyInfo.OnDeathMoveSpeedUpgrade, this.MyInfo.OnDeathMoveSpeed, this.MyInfo.UpgradesValues);
			this.OnDeathLifeTime = new Upgradeable(this.MyInfo.OnDeathLifeTimeUpgrade, this.MyInfo.OnDeathLifeTime, this.MyInfo.UpgradesValues);
			this.FireDeathEffectOnWarmupDeath = new Upgradeable(this.MyInfo.FireDeathEffectOnWarmupDeathUpgrade, this.MyInfo.FireDeathEffectOnWarmupDeath, base.Info.UpgradesValues);
			this.FireDeathEffectOnEffectDeath = new Upgradeable(this.MyInfo.FireDeathEffectOnEffectDeathUpgrade, this.MyInfo.FireDeathEffectOnEffectDeath, base.Info.UpgradesValues);
			this.FireDeathEffectOnExtraEffectDeath = new Upgradeable(this.MyInfo.FireDeathEffectOnExtraEffectDeathUpgrade, this.MyInfo.FireDeathEffectOnExtraEffectDeath, base.Info.UpgradesValues);
			this.DrainLifePctFromTarget = new Upgradeable(this.MyInfo.DrainLifePctFromTargetUpgrade, this.MyInfo.DrainLifePctFromTarget, this.MyInfo.UpgradesValues);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this.OnDeathDamage.SetLevel(upgradeName, level);
			this.OnDeathMoveSpeed.SetLevel(upgradeName, level);
			this.OnDeathLifeTime.SetLevel(upgradeName, level);
			this.FireDeathEffectOnWarmupDeath.SetLevel(upgradeName, level);
			this.FireDeathEffectOnEffectDeath.SetLevel(upgradeName, level);
			this.FireDeathEffectOnExtraEffectDeath.SetLevel(upgradeName, level);
			this.DrainLifePctFromTarget.SetLevel(upgradeName, level);
		}

		public override void OnDestroyEffect(DestroyEffect evt)
		{
			base.OnDestroyEffect(evt);
			if (this._effectDict.ContainsKey(evt.RemoveData.TargetEventId))
			{
				this._effectDict.Remove(evt.RemoveData.TargetEventId);
			}
			if (this.MyInfo.FireDeathEffectOnlyIfTargetIdIsValid && evt.RemoveData.TargetId == -1)
			{
				return;
			}
			if ((!this.FireDeathEffectOnWarmupDeath.BoolGet() || evt.EffectData.EffectInfo.EffectId != this.MyInfo.WarmupEffect.EffectId || evt.RemoveData.DestroyReason != BaseFX.EDestroyReason.Lifetime) && (!this.FireDeathEffectOnEffectDeath.BoolGet() || evt.EffectData.EffectInfo.EffectId != this.MyInfo.Effect.EffectId) && (!this.FireDeathEffectOnExtraEffectDeath.BoolGet() || evt.EffectData.EffectInfo.EffectId != this.MyInfo.ExtraEffect.EffectId))
			{
				return;
			}
			this.FireOnDeathEffect(evt.RemoveData.Origin, evt.RemoveData.TargetId);
		}

		protected virtual int FireOnDeathEffect(Vector3 origin, int targetId)
		{
			EffectEvent effectEvent = base.GetEffectEvent(this.MyInfo.OnDeathEffect);
			effectEvent.MoveSpeed = this.OnDeathMoveSpeed.Get();
			effectEvent.Range = this.GetRange();
			effectEvent.Origin = origin;
			effectEvent.Target = base.Target;
			effectEvent.TargetId = targetId;
			effectEvent.LifeTime = this.OnDeathLifeTime.Get();
			effectEvent.Modifiers = this.OnDeathDamage;
			base.SetTargetAndDirection(effectEvent);
			return GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
		}

		protected override void OnPosDamageCaused(CombatObject causer, CombatObject taker, ModifierData mod, float amount, int eventid)
		{
			if (this.DrainLifePctFromTarget == 0f || mod.GadgetInfo.GadgetId != this.MyInfo.GadgetId || !mod.Info.Effect.IsHPDamage())
			{
				return;
			}
			if (this._effectDict.ContainsKey(taker))
			{
				return;
			}
			int targetId = this.TargetId;
			this.TargetId = taker.Id.ObjId;
			this._effectDict.Add(taker, this.FireExtraGadget(Vector3.zero));
			this.TargetId = targetId;
		}

		public override void Clear()
		{
			base.Clear();
			this._effectDict.Clear();
		}

		public void OnDamageTakenCallback(DamageTakenCallback evt)
		{
			if (this.DrainLifePctFromTarget == 0f || !evt.CauserCombatObject || evt.CauserCombatObject.Team != this.Combat.Team)
			{
				return;
			}
			base.DrainLifeCheck(evt.CauserCombatObject, evt.TakerCombatObject, true, evt.Amount, this.DrainLifePctFromTarget, evt.CauserEventId, this.MyInfo.LifeStealFeedback);
		}

		protected ModifierData[] OnDeathDamage;

		protected Upgradeable OnDeathMoveSpeed;

		protected Upgradeable OnDeathLifeTime;

		protected Upgradeable FireDeathEffectOnWarmupDeath;

		protected Upgradeable FireDeathEffectOnEffectDeath;

		protected Upgradeable FireDeathEffectOnExtraEffectDeath;

		protected Upgradeable DrainLifePctFromTarget;

		private readonly BiDictionary<CombatObject, int> _effectDict = new BiDictionary<CombatObject, int>();
	}
}
