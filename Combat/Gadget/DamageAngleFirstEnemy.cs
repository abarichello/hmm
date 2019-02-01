using System;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class DamageAngleFirstEnemy : BasicCannon
	{
		public DamageAngleFirstEnemyInfo MyInfo
		{
			get
			{
				return base.Info as DamageAngleFirstEnemyInfo;
			}
		}

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			this._updater = new TimedUpdater(50, false, false);
			if (this.Combat.Team == TeamKind.Red)
			{
				this._enemyMask = 2048;
			}
			else
			{
				this._enemyMask = 1024;
			}
			this._enemyMask |= 1048576;
			this._enemyMask |= 512;
			this.Angle = new Upgradeable(this.MyInfo.AngleUpgrade, (float)this.MyInfo.Angle, this.MyInfo.UpgradesValues);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this.Angle.SetLevel(upgradeName, level);
		}

		protected override int FireGadget()
		{
			if (this._currentTargetEffectId != -1)
			{
				return -1;
			}
			this._remainingLifetime = base.LifeTime;
			CombatObject closestCombatInFront = this.GetClosestCombatInFront();
			if (closestCombatInFront)
			{
				this._currentTargetEffectId = this.FireEffectOnEnemy(closestCombatInFront);
			}
			else
			{
				this._currentTargetEffectId = base.FireGadget();
			}
			return this._currentTargetEffectId;
		}

		protected override void InnerOnDestroyEffect(DestroyEffect evt)
		{
			if (evt.RemoveData.TargetEventId != this._currentTargetEffectId && this._currentTargetEffectId != -1)
			{
				return;
			}
			this._currentTargetEffectId = -1;
			this._currentTargetCombat = null;
		}

		private CombatObject GetClosestCombatInFront()
		{
			Vector3 vector = this.DummyPosition();
			float num = float.MaxValue;
			Collider collider = null;
			Vector3 vector2 = this.Combat.transform.forward;
			if (this._currentTargetCombat != null)
			{
				vector2 = this._currentTargetCombat.transform.position - this.DummyPosition();
			}
			Collider[] array = Physics.OverlapSphere(vector, base.Radius, this._enemyMask);
			for (int i = 0; i < array.Length; i++)
			{
				Collider component = array[i].GetComponent<Collider>();
				CombatObject combat = CombatRef.GetCombat(component);
				if (!(combat != null) || combat.Team != this.Combat.Team)
				{
					float num2 = Vector3.SqrMagnitude(PhysicsUtils2.GetHit(vector, component).point - vector);
					if (num2 < num)
					{
						num = num2;
						collider = component;
					}
				}
			}
			if (collider == null)
			{
				RaycastHit[] array2 = Physics.SphereCastAll(vector, base.Radius, vector2.normalized, this.GetRange() - base.Radius, this._enemyMask);
				num = float.MaxValue;
				foreach (RaycastHit raycastHit in array2)
				{
					CombatObject combat2 = CombatRef.GetCombat(raycastHit.collider);
					if (!(combat2 != null) || combat2.Team != this.Combat.Team)
					{
						float num3 = Vector3.SqrMagnitude(raycastHit.point - vector);
						if (num3 < num)
						{
							num = num3;
							collider = raycastHit.collider;
						}
					}
				}
			}
			if (collider != null && collider.gameObject.layer != 9)
			{
				CombatObject combat3 = CombatRef.GetCombat(collider.GetComponent<Collider>());
				if (combat3 != null && combat3.Team != this.Combat.Team)
				{
					return combat3;
				}
			}
			return null;
		}

		private int FireEffectOnEnemy(CombatObject targetCombat)
		{
			this._currentTargetCombat = targetCombat;
			int targetId = this.TargetId;
			this.TargetId = targetCombat.Id.ObjId;
			this._currentTargetEffectId = this.FireExtraGadget(this.MyInfo.ExtraEffect, ModifierData.CopyData(this.ExtraModifier), ModifierData.CopyData(this._damage), delegate(EffectEvent data)
			{
				data.LifeTime = this._remainingLifetime;
			});
			this.TargetId = targetId;
			base.ExistingFiredEffectsAdd(this._currentTargetEffectId);
			return this._currentTargetEffectId;
		}

		protected override void RunBeforeUpdate()
		{
			if (this._currentTargetEffectId == -1 || !this.Combat.Data.IsAlive())
			{
				return;
			}
			this._remainingLifetime -= Time.deltaTime;
			if (this._updater.ShouldHalt())
			{
				return;
			}
			bool flag = false;
			if (this._currentTargetCombat != null && !PhysicsUtils.IsInsideAngleAndRange(this.DummyPosition(), this.Combat.transform.forward, this._currentTargetCombat.transform.GetComponent<Collider>(), this.Angle, this.GetRange() + base.Radius))
			{
				flag = true;
				this.DestroyTargetEffect(this._currentTargetEffectId, BaseFX.EDestroyReason.LostTarget);
				this._currentTargetCombat = null;
				base.ExistingFiredEffectsRemove(this._currentTargetEffectId);
			}
			CombatObject closestCombatInFront = this.GetClosestCombatInFront();
			if (flag && !closestCombatInFront && this._remainingLifetime > 0f)
			{
				this._currentTargetEffectId = this.FireExtraGadget(this.MyInfo.Effect, ModifierData.CopyData(this._damage), ModifierData.CopyData(this.ExtraModifier), delegate(EffectEvent data)
				{
					data.LifeTime = this._remainingLifetime;
				});
				base.ExistingFiredEffectsAdd(this._currentTargetEffectId);
			}
			if (closestCombatInFront)
			{
				if (this._currentTargetCombat != null && this._currentTargetCombat.Id.ObjId == closestCombatInFront.Id.ObjId)
				{
					return;
				}
				if (this._currentTargetEffectId != -1)
				{
					this.DestroyTargetEffect(this._currentTargetEffectId, BaseFX.EDestroyReason.HitIdentifiable);
				}
				this.FireEffectOnEnemy(closestCombatInFront);
			}
		}

		private void DestroyTargetEffect(int effectId, BaseFX.EDestroyReason reason)
		{
			if (effectId == -1)
			{
				return;
			}
			BaseFX baseFx = GameHubBehaviour.Hub.Effects.GetBaseFx(effectId);
			if (baseFx)
			{
				baseFx.TriggerDestroy(this.Combat.Id.ObjId, this.Combat.transform.position, false, null, Vector3.zero, reason, false);
			}
		}

		private void OnDrawGizmos()
		{
			if (!base.Pressed)
			{
				return;
			}
			Vector3 a = Quaternion.Euler(0f, this.Angle * 0.5f, 0f) * this.Combat.transform.forward;
			Vector3 a2 = Quaternion.Euler(0f, -this.Angle * 0.5f, 0f) * this.Combat.transform.forward;
			Gizmos.color = Color.red;
			float range = this.GetRange();
			Vector3 vector = this.DummyPosition();
			Gizmos.DrawLine(vector, vector + a * range);
			Gizmos.DrawLine(vector, vector + Vector3.up * 10f);
			Gizmos.DrawLine(vector + Vector3.up * 10f, vector + a * range + Vector3.up * 10f);
			Gizmos.DrawLine(vector + a * range, vector + a * range + Vector3.up * 10f);
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(vector, vector + a2 * range);
			Gizmos.DrawLine(vector + a2 * range, vector + a2 * range + Vector3.up * 10f);
			Gizmos.DrawLine(vector + Vector3.up * 10f, vector + a2 * range + Vector3.up * 10f);
			Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
			Gizmos.DrawSphere(vector, range);
			Gizmos.color = Color.blue;
			Gizmos.DrawWireSphere(vector + this.Combat.transform.forward * (float)this.MyInfo.InitialOverlapSphereRadius, (float)this.MyInfo.InitialOverlapSphereRadius);
			Vector3 vector2 = this.Combat.transform.forward;
			if (this._currentTargetCombat)
			{
				vector2 = this._currentTargetCombat.transform.position - this.DummyPosition();
			}
			Vector3 b = vector2.normalized * range;
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(this.DummyPosition(), (!(this._currentTargetCombat != null)) ? (this.DummyPosition() + b) : (this._currentTargetCombat.transform.position + b));
		}

		private int _currentTargetEffectId = -1;

		protected float _remainingLifetime;

		protected Upgradeable Angle;

		private int _enemyMask;

		protected CombatObject _currentTargetCombat;

		private TimedUpdater _updater;
	}
}
