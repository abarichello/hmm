using System;
using System.Collections.Generic;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkDamageAngleOnTick : BaseDamageablePerk
	{
		protected override void OnPerkInitialized()
		{
			this._timedUpdater = new TimedUpdater(this.TickMillis, false, false);
			this._range = ((this.Range != 0f) ? this.Range : this.Effect.Data.Range);
			this._angle = this.Angle;
			Vector3 position = base._trans.position;
			position.y = 0f;
			base._trans.position = position;
		}

		private void FixedUpdate()
		{
			if (this._timedUpdater.ShouldHalt())
			{
				return;
			}
			this.DamageArea();
		}

		private void DamageArea()
		{
			Vector3 position = base._trans.position;
			position.y = 0f;
			base._trans.position = position;
			BarrierUtils.OverlapSphereRaycastFromCenter(base._trans.position, this._range, 1077054464, PerkDamageAngleOnTick._objects);
			for (int i = 0; i < PerkDamageAngleOnTick._objects.Count; i++)
			{
				BarrierUtils.CombatHit combatHit = PerkDamageAngleOnTick._objects[i];
				CombatObject combat = combatHit.Combat;
				if (!this.HitOnlyOnce || !this.Damaged.Contains(combat.Id.ObjId))
				{
					if (this.SingleTarget && this.DamagedCount > 0)
					{
						break;
					}
					bool flag = this.Effect.CheckHit(combat);
					if (flag && combat && combat.Controller)
					{
						if (PhysicsUtils.IsInsideAngle(base._trans, combat.transform, this._angle))
						{
							Vector3 normalized = (combat.transform.position - base._trans.position).normalized;
							base.ApplyDamage(combat, combat, combatHit.Barrier, normalized, base._trans.position);
						}
					}
				}
			}
			PerkDamageAngleOnTick._objects.Clear();
		}

		private void OnDrawGizmos()
		{
			if (!this.Effect || !base._trans)
			{
				return;
			}
			Vector3 vector = Quaternion.Euler(0f, this.Angle / 2f, 0f) * base._trans.forward;
			Vector3 vector2 = Quaternion.Euler(0f, -this.Angle / 2f, 0f) * base._trans.forward;
			Gizmos.color = Color.red;
			Gizmos.DrawLine(base._trans.position, base._trans.position + vector * this._range);
			Gizmos.DrawLine(base._trans.position, base._trans.position + Vector3.up * 10f);
			Gizmos.DrawLine(base._trans.position + Vector3.up * 10f, base._trans.position + vector * this._range + Vector3.up * 10f);
			Gizmos.DrawLine(base._trans.position + vector * this._range, base._trans.position + vector * this._range + Vector3.up * 10f);
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(base._trans.position, base._trans.position + vector2 * this._range);
			Gizmos.DrawLine(base._trans.position + vector2 * this._range, base._trans.position + vector2 * this._range + Vector3.up * 10f);
			Gizmos.DrawLine(base._trans.position + Vector3.up * 10f, base._trans.position + vector2 * this._range + Vector3.up * 10f);
			Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
			Gizmos.DrawSphere(base._trans.position, this._range);
		}

		private static readonly BitLogger Log = new BitLogger(typeof(PerkDamageAngleOnTick));

		public bool UseExtraModifiers;

		public int TickMillis;

		private TimedUpdater _timedUpdater;

		public float Angle = 180f;

		public float Range;

		private float _angle;

		private float _range;

		private static readonly List<BarrierUtils.CombatHit> _objects = new List<BarrierUtils.CombatHit>(10);
	}
}
