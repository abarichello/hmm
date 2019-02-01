using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat.Gadget;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkDamageIncreasingArea : BaseDamageablePerk, PerkAttachToObject.IEffectAttachListener
	{
		protected override void Awake()
		{
			base.Awake();
			this._initialMaxRange = this.MaxRange;
			this._initialDurationMillis = this.DurationMillis;
		}

		protected override void OnPerkInitialized()
		{
			this.MaxRange = this._initialMaxRange;
			if (this.MaxRange == 0f && !this.UseGadgetRadius)
			{
				this.MaxRange = this.Effect.Data.Range;
			}
			else if (this.MaxRange == 0f && this.UseGadgetRadius)
			{
				this.MaxRange = this.Effect.Gadget.Radius;
				this.MinimumRange = this.Effect.Data.Range;
			}
			this.DurationMillis = this._initialDurationMillis;
			if (this.DurationMillis == 0)
			{
				this.DurationMillis = (int)(this.Effect.Data.LifeTime * 1000f);
			}
			this._transform = base.transform;
			this._iterationCount = 0;
			this._isShrinking = false;
			this.DurationMillis /= ((!this.ShrinkBack) ? 1 : 2);
			this._iterationMax = Mathf.Max(this.DurationMillis / 100, 1);
			this._rangeDelta = (this.MaxRange - this.MinimumRange) / (float)this._iterationMax;
			this._timedUpdater = new TimedUpdater(100, false, false);
			this.ReduceDamageOnHit = Convert.ToBoolean(this.Effect.Data.CustomVar);
			if (this.LoopDamage)
			{
				this._isTickDeltaActive = false;
				int periodMillis = (this.TickMillis != 0) ? this.TickMillis : ((int)(this.Effect.Gadget.Cooldown * 1000f));
				this._tickDeltaUpdater = new TimedUpdater(periodMillis, false, false);
			}
			this._combatHits.Clear();
		}

		private void FixedUpdate()
		{
			if ((this._isTickDeltaActive || (this.ShrinkBack && this.LoopDamage)) && this._tickDeltaUpdater.ShouldHalt())
			{
				if (this.LoopDamage && this.ShrinkBack)
				{
					this._combatHits.Clear();
				}
				return;
			}
			if ((this._iterationCount == this._iterationMax && !this.ShrinkBack) || (this.ShrinkBack && this._iterationCount >= this._iterationMax * 2))
			{
				return;
			}
			if (this._timedUpdater.ShouldHalt())
			{
				return;
			}
			this._isTickDeltaActive = false;
			this._iterationCount++;
			float radius = ((!this._isShrinking) ? (this._rangeDelta * (float)this._iterationCount) : (this._rangeDelta * (float)(this._iterationMax - (this._iterationCount - this._iterationMax)))) + this.MinimumRange;
			if (this._iterationCount >= this._iterationMax)
			{
				this._isShrinking = true;
			}
			this._hitsCache.Clear();
			BarrierUtils.OverlapSphereRaycastFromCenter(this.GetCenter(), radius, 1077058560, this._hitsCache);
			List<BarrierUtils.CombatHit> list = null;
			List<CombatObject> list2 = null;
			if (this.IsPartialCallbackEnabled || this.ReduceDamageOnHit)
			{
				list = new List<BarrierUtils.CombatHit>(this._hitsCache.Count);
				list2 = new List<CombatObject>(this._hitsCache.Count);
			}
			for (int i = 0; i < this._hitsCache.Count; i++)
			{
				BarrierUtils.CombatHit item = this._hitsCache[i];
				CombatObject combat = item.Combat;
				if (combat && combat.Controller && !this._combatHits.Contains(combat) && this.Effect.CheckHit(combat))
				{
					if (!this.ReduceDamageOnHit)
					{
						combat.Controller.AddModifiers(this.Modifiers, this.Effect.Gadget.Combat, this.Effect.EventId, item.Barrier);
					}
					this._combatHits.Add(combat);
					if (list != null)
					{
						list.Add(item);
						list2.Add(item.Combat);
					}
				}
			}
			if (this.IsPartialCallbackEnabled)
			{
				Mural.Post(new DamageAreaCallback(list2, this.GetCenter(), this.Effect, GadgetSlot.None), this.Effect.Gadget);
			}
			if (this.ReduceDamageOnHit && list != null && list.Count > 0)
			{
				float a = (float)(this.TargetAmountReduceModifier + list.Count) * this.ReducedDamagePercentValue;
				this.Modifiers = ModifierData.RemoveAmountPercent(this.Modifiers, Mathf.Min(a, this.MaxDamageReductionPercent));
				for (int j = 0; j < list.Count; j++)
				{
					BarrierUtils.CombatHit combatHit = list[j];
					combatHit.Combat.Controller.AddModifiers(this.Modifiers, this.Effect.Gadget.Combat, this.Effect.EventId, combatHit.Barrier);
				}
			}
			if (this.IsFinalCallbackEnable && this._iterationCount == this._iterationMax)
			{
				Mural.Post(new DamageAreaCallback(new List<CombatObject>(this._combatHits), this.GetCenter(), this.Effect, GadgetSlot.None), this.Effect.Gadget);
			}
			if (this.LoopDamage && this._iterationCount == this._iterationMax && !this.ShrinkBack)
			{
				this._isTickDeltaActive = true;
				this._tickDeltaUpdater.Reset();
				this._iterationCount = 0;
				this._combatHits.Clear();
				this._hitsCache.Clear();
				this.Modifiers = base.GetModifiers(this.Source);
			}
		}

		private Vector3 GetCenter()
		{
			PerkDamageIncreasingArea.AreaCenter center = this.Center;
			if (center == PerkDamageIncreasingArea.AreaCenter.EffectPosition)
			{
				return this._transform.position;
			}
			if (center != PerkDamageIncreasingArea.AreaCenter.EffectOrigin)
			{
				return this.Effect.Data.Target;
			}
			return this.Effect.Data.Origin;
		}

		public void OnAttachEffect(PerkAttachToObject.EffectAttachToTarget msg)
		{
			this._transform = msg.Target;
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.red;
			Vector3 position = base.transform.position;
			float radius = ((!this._isShrinking) ? (this._rangeDelta * (float)this._iterationCount) : (this._rangeDelta * (float)(this._iterationMax - (this._iterationCount - this._iterationMax)))) + this.MinimumRange;
			Gizmos.color = Color.blue;
			Gizmos.DrawWireSphere(position, radius);
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(position, this.MaxRange);
		}

		public bool ShrinkBack;

		private bool _isShrinking;

		public bool UseGadgetRadius;

		public float MinimumRange;

		public float MaxRange;

		private float _initialMaxRange;

		public int DurationMillis;

		private int _initialDurationMillis;

		private const int CheckAreaMillis = 100;

		public bool LoopDamage;

		public int TickMillis;

		private bool _isTickDeltaActive;

		private TimedUpdater _tickDeltaUpdater;

		public bool ReduceDamageOnHit;

		public float ReducedDamagePercentValue;

		public float MaxDamageReductionPercent;

		public int TargetAmountReduceModifier;

		public bool IsPartialCallbackEnabled;

		public bool IsFinalCallbackEnable;

		private TimedUpdater _timedUpdater;

		private Transform _transform;

		private float _rangeDelta;

		private int _iterationCount;

		private int _iterationMax;

		private float _minimumRadius;

		private readonly HashSet<CombatObject> _combatHits = new HashSet<CombatObject>();

		private readonly List<BarrierUtils.CombatHit> _hitsCache = new List<BarrierUtils.CombatHit>(20);

		public PerkDamageIncreasingArea.AreaCenter Center;

		public enum AreaCenter
		{
			EffectTarget,
			EffectOrigin,
			EffectPosition
		}
	}
}
