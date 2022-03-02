using System;
using System.Collections.Generic;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkCallbackOnEnter : BasePerk, IPerkWithCollision
	{
		public override void PerkInitialized()
		{
			this._hitted.Clear();
			this.Damaged.Clear();
			this.SanityCheck();
		}

		protected void FixedUpdate()
		{
			if (this.ResetOnExit)
			{
				this._hitted.IntersectWith(this.Damaged);
				this.Damaged.Clear();
				this.Damaged.AddRange(this._hitted);
			}
			this._hitted.Clear();
		}

		protected bool SanityCheck()
		{
			if (this.IsSanityChecked)
			{
				return true;
			}
			if (GameHubBehaviour.Hub && GameHubBehaviour.Hub.Net.IsClient())
			{
				base.enabled = false;
				return false;
			}
			this.IsSanityChecked = true;
			return true;
		}

		public int Priority()
		{
			return -1;
		}

		public void OnHit(Collider other, Vector3 hitPoint, Vector3 hitNormal, bool isOverlapping, bool isBarrier)
		{
			if (!this.SanityCheck())
			{
				return;
			}
			this.ServerHitEnter(other, isBarrier);
		}

		public void OnStay(Collider other, Vector3 hitPoint, Vector3 hitNormal, bool isOverlapping, bool barrier)
		{
		}

		public void OnEnter(Collider other, Vector3 hitPoint, Vector3 hitNormal, bool isOverlapping, bool barrier)
		{
		}

		public void OnExit(Collider other, Vector3 hitPoint, Vector3 hitNormal, bool isOverlapping, bool barrier)
		{
		}

		protected virtual void ServerHitEnter(Collider other, bool isBarrier)
		{
			if (this.Effect.IsDead)
			{
				return;
			}
			if (this.SingleTarget && this.Damaged.Count > 0)
			{
				return;
			}
			CombatObject combatObject = null;
			CombatObject combatObject2 = null;
			BaseFX baseFX = null;
			bool flag = false;
			switch (this.Mode)
			{
			case PerkCallbackOnEnter.ModeEnum.None:
				combatObject = (combatObject2 = CombatRef.GetCombat(other));
				flag = this.Effect.CheckHit(combatObject2);
				break;
			case PerkCallbackOnEnter.ModeEnum.TargetOnly:
				combatObject = (combatObject2 = CombatRef.GetCombat(other));
				flag = (combatObject2 && combatObject2.Id == this.Effect.Target && this.Effect.CheckHit(combatObject2));
				break;
			case PerkCallbackOnEnter.ModeEnum.Scenery:
				if (other.gameObject.layer == 9)
				{
					combatObject = (combatObject2 = this.Effect.GetTargetCombat(BasePerk.PerkTarget.Target));
					flag = true;
				}
				break;
			case PerkCallbackOnEnter.ModeEnum.Projectile:
				if (other.gameObject.layer == 13)
				{
					baseFX = BaseFX.GetFX(other);
					if (baseFX != null && !baseFX.IsDead && baseFX.enabled)
					{
						combatObject = (combatObject2 = baseFX.GetTargetCombat(BasePerk.PerkTarget.Owner));
						flag = this.Effect.CheckHit(combatObject2);
					}
				}
				break;
			}
			if (!flag)
			{
				return;
			}
			if (combatObject2)
			{
				this._hitted.Add(combatObject2.Id.ObjId);
			}
			if (this.HitOnlyOnce && combatObject2 && this.Damaged.Contains(combatObject2.Id.ObjId))
			{
				return;
			}
			if (combatObject)
			{
				if (this.Effect.Gadget is TriggerEnterCallback.ITriggerEnterCallbackListener)
				{
					((TriggerEnterCallback.ITriggerEnterCallbackListener)this.Effect.Gadget).OnTriggerEnterCallback(new TriggerEnterCallback(combatObject, this.Effect.Gadget, this.Effect.Data, baseFX));
				}
				if (this.SingleTarget || this.HitOnlyOnce)
				{
					this.Damaged.Add(combatObject2.Id.ObjId);
				}
			}
		}

		public PerkCallbackOnEnter.ModeEnum Mode = PerkCallbackOnEnter.ModeEnum.None;

		public bool SingleTarget = true;

		public bool HitOnlyOnce = true;

		[Header("Simulate OnEnter instead of OnStay (With HitOnlyOnce)")]
		public bool ResetOnExit;

		protected bool IsSanityChecked;

		protected readonly List<int> Damaged = new List<int>();

		private readonly HashSet<int> _hitted = new HashSet<int>();

		public enum ModeEnum
		{
			None = 1,
			TargetOnly,
			Scenery,
			Projectile
		}
	}
}
