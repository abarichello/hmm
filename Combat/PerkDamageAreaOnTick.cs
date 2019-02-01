using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat.Gadget;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkDamageAreaOnTick : BasePerk, PerkAttachToObject.IEffectAttachListener
	{
		private Collider[] GetHits()
		{
			float num = (!this.UseGadgetRadius) ? this.Range : this.Effect.Gadget.Radius;
			if (!this.UseGadgetRadius && num == 0f)
			{
				num = this.Effect.Data.Range;
			}
			return Physics.OverlapSphere(this.GetCenter(), num, 1077058560);
		}

		public override void PerkInitialized()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				base.enabled = false;
				return;
			}
			this._trans = base.transform;
			this._modifiers = base.GetModifiers(this.Source);
			this._timedUpdater = new TimedUpdater(this.TickMillis, false, false);
		}

		private void FixedUpdate()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			if (!this._timedUpdater.ShouldHalt())
			{
				this.DamageArea();
			}
		}

		private void DamageArea()
		{
			Collider[] hits = this.GetHits();
			List<CombatObject> list = new List<CombatObject>();
			List<BarrierUtils.CombatHit> list2 = new List<BarrierUtils.CombatHit>();
			foreach (Collider collider in hits)
			{
				CombatObject combat = CombatRef.GetCombat(collider);
				bool barrier = BarrierUtils.IsBarrier(collider);
				bool flag = this.Effect.CheckHit(combat);
				if (flag && combat && combat.Controller)
				{
					list2.Add(new BarrierUtils.CombatHit
					{
						Combat = combat,
						Col = collider,
						Barrier = barrier
					});
				}
			}
			BarrierUtils.FilterByRaycastFromPoint(this.GetCenter(), list2);
			ModifierData[] array = this._modifiers;
			for (int j = 0; j < list2.Count; j++)
			{
				CombatObject combat2 = list2[j].Combat;
				if (this.DamageByRange)
				{
					float baseAmount = this.DamageToRange.Evaluate(Vector3.Distance(combat2.Transform.position, this.GetCenter()));
					array = ModifierData.CreateConvoluted(array, baseAmount);
				}
				Vector3 normalized = (combat2.Transform.position - this.GetCenter()).normalized;
				BaseDamageablePerk.UpdateCustomDirection(this.Effect, array, combat2, this.CustomDirection, normalized, this.GetCenter());
				combat2.Controller.AddModifiers(array, this.Effect.Gadget.Combat, this.Effect.EventId, list2[j].Barrier);
				list.Add(combat2);
			}
			if (!this.IsDamageCallbackEnabled)
			{
				return;
			}
			Mural.Post(new DamageAreaCallback(list, this.GetCenter(), this.Effect, this.TargetGadgetCallback), this.Effect.Gadget);
		}

		private Vector3 GetCenter()
		{
			switch (this.Center)
			{
			case PerkDamageAreaOnTick.AreaCenter.EffectOrigin:
				return this.Effect.Data.Origin;
			case PerkDamageAreaOnTick.AreaCenter.EffectPosition:
				return this._trans.position;
			case PerkDamageAreaOnTick.AreaCenter.FollowTarget:
				if (!this.Effect.Target)
				{
					return Vector3.zero;
				}
				return this.Effect.Target.transform.position;
			}
			return this.Effect.Data.Target;
		}

		private void OnDrawGizmos()
		{
			if (!this.Effect || !this.Effect.Gadget || !this._trans)
			{
				return;
			}
			float num = (!this.UseGadgetRadius) ? this.Range : this.Effect.Gadget.Radius;
			if (!this.UseGadgetRadius && num == 0f)
			{
				num = this.Effect.Data.Range;
			}
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(this.GetCenter(), num);
		}

		public void OnAttachEffect(PerkAttachToObject.EffectAttachToTarget msg)
		{
			this._trans = msg.Target;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PerkDamageAreaOnTick));

		public float Range;

		public bool UseGadgetRadius;

		public int TickMillis;

		public bool IsDamageCallbackEnabled;

		public bool DamageByRange;

		public AnimationCurve DamageToRange;

		public GadgetSlot TargetGadgetCallback;

		public BasePerk.DamageSource Source = BasePerk.DamageSource.EventModifiers;

		private ModifierData[] _modifiers;

		private TimedUpdater _timedUpdater;

		private new Transform _trans;

		public PerkDamageAreaOnTick.AreaCenter Center;

		public BaseDamageablePerk.ECustomDirection CustomDirection;

		public enum AreaCenter
		{
			EffectTarget,
			EffectOrigin,
			EffectPosition,
			FollowTarget
		}
	}
}
