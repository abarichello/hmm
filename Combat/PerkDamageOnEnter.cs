using System;
using System.Collections.Generic;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkDamageOnEnter : BaseDamageablePerk, IPerkWithCollision
	{
		public int Priority()
		{
			return -2;
		}

		public void OnHit(Collider other, Vector3 hitPoint, Vector3 hitNormal, bool isOverlapping, bool isBarrier)
		{
			if (this.Effect.IsDead || GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			this.ServerHitEnter(other, isBarrier);
		}

		public void OnStay(Collider other, Vector3 hitPoint, Vector3 hitNormal, bool isOverlapping, bool isBarrier)
		{
		}

		public void OnEnter(Collider other, Vector3 hitPoint, Vector3 hitNormal, bool isOverlapping, bool isBarrier)
		{
		}

		public void OnExit(Collider other, Vector3 hitPoint, Vector3 hitNormal, bool isOverlapping, bool isBarrier)
		{
		}

		protected void FixedUpdate()
		{
			if (this.ResetDamagedOnExit)
			{
				this._hitted.IntersectWith(this.Damaged);
				this.Damaged.Clear();
				this.Damaged.AddRange(this._hitted);
			}
			this._hitted.RemoveWhere((int x) => !this._hittedThroughEnter.Contains(x));
		}

		protected void OnTriggerEnter(Collider other)
		{
			CombatObject combat = CombatRef.GetCombat(other);
			if (!this.Effect.CheckHit(combat))
			{
				return;
			}
			if (this.IgnoreOwnerTriggerEnter && this.Effect.Owner.ObjId == combat.Id.ObjId)
			{
				return;
			}
			this._hittedThroughEnter.Add(combat.Id.ObjId);
		}

		protected void OnTriggerExit(Collider other)
		{
			CombatObject combat = CombatRef.GetCombat(other);
			if (combat)
			{
				this._hittedThroughEnter.Remove(combat.Id.ObjId);
				this._hitted.Remove(combat.Id.ObjId);
			}
		}

		private void ServerHitEnter(Collider other, bool isBarrier)
		{
			if (this.SingleTarget && this.Damaged.Count > 0)
			{
				return;
			}
			CombatObject combatObject = null;
			CombatObject combatObject2 = null;
			bool flag = false;
			CombatObject combat = CombatRef.GetCombat(other);
			if (null == combat)
			{
				return;
			}
			if (this.IgnoreOwnerTriggerEnter && this.Effect.Owner.ObjId == combat.Id.ObjId)
			{
				return;
			}
			switch (this.Mode)
			{
			case PerkDamageOnEnter.ModeEnum.DamageOtherAndOwner:
				combatObject = (combatObject2 = combat);
				flag = this.Effect.CheckHit(combatObject2);
				break;
			case PerkDamageOnEnter.ModeEnum.DamageTargetOnEnterScenery:
				combatObject = (combatObject2 = CombatRef.GetCombat(this.Effect.Target));
				flag = (9 == other.gameObject.layer);
				break;
			case PerkDamageOnEnter.ModeEnum.DamageOtherIgnoreTarget:
				combatObject = (combatObject2 = combat);
				flag = (combatObject2 && combatObject2.Id != this.Effect.Target && this.Effect.CheckHit(combatObject2));
				break;
			case PerkDamageOnEnter.ModeEnum.DamageTarget:
				combatObject = CombatRef.GetCombat(this.Effect.Target);
				combatObject2 = combat;
				flag = this.Effect.CheckHit(combatObject2);
				break;
			case PerkDamageOnEnter.ModeEnum.DamageTargetIgnoreOther:
				combatObject = (combatObject2 = combat);
				flag = (combatObject2 && combatObject2.Id == this.Effect.Target && this.Effect.CheckHit(combatObject2));
				break;
			case PerkDamageOnEnter.ModeEnum.DamageOwner:
				combatObject = CombatRef.GetCombat(this.Effect.Owner);
				combatObject2 = combat;
				flag = this.Effect.CheckHit(combatObject2);
				break;
			case PerkDamageOnEnter.ModeEnum.DamageOther:
				combatObject = (combatObject2 = combat);
				if (combatObject && combatObject.Id.ObjId != this.Effect.Owner.ObjId)
				{
					flag = this.Effect.CheckHit(combatObject2);
				}
				break;
			}
			if (flag)
			{
				Vector3 vector = combatObject.transform.position - base._trans.position;
				if (combatObject && combatObject2 && (this.DamageThroughScenery || !Physics.Raycast(base._trans.position, vector, vector.magnitude, 512)))
				{
					this._hitted.Add(combatObject2.Id.ObjId);
					base.ApplyDamage(combatObject, combatObject2, isBarrier, this.Effect.Data.Direction, this.Effect.transform.position);
				}
			}
		}

		public override void OnDestroyEffect(DestroyEffectMessage evt)
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			if (!this.DamageOnDestroy)
			{
				return;
			}
			ModifierData[] modifiers = base.GetModifiers(this.DamageOnDestroySource);
			List<int> list = new List<int>(this._hitted);
			for (int i = 0; i < list.Count; i++)
			{
				CombatObject combat = CombatRef.GetCombat(list[i]);
				base.ApplyDamage(combat, combat, false, modifiers);
			}
		}

		protected override void OnPerkInitialized()
		{
			this._hitted.Clear();
			this._hittedThroughEnter.Clear();
		}

		private static readonly BitLogger Log = new BitLogger(typeof(PerkDamageOnEnter));

		[Header("Use this mode, forget Target on this perk")]
		public PerkDamageOnEnter.ModeEnum Mode = PerkDamageOnEnter.ModeEnum.DamageOtherAndOwner;

		public bool DamageThroughScenery = true;

		[Header("Simulate OnEnter instead of OnStay (With HitOnlyOnce)")]
		public bool ResetDamagedOnExit;

		public bool DamageOnDestroy;

		public BasePerk.DamageSource DamageOnDestroySource;

		[SerializeField]
		[Tooltip("Workaround for OnTriggerEnter behaviour change in Unity 2018")]
		private bool IgnoreOwnerTriggerEnter;

		private readonly HashSet<int> _hitted = new HashSet<int>();

		private readonly HashSet<int> _hittedThroughEnter = new HashSet<int>();

		public enum ModeEnum
		{
			DamageOtherAndOwner = 1,
			DamageTargetOnEnterScenery,
			DamageOtherIgnoreTarget,
			DamageTarget,
			DamageTargetIgnoreOther,
			DamageOwner,
			DamageOther
		}
	}
}
