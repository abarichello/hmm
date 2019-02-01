using System;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class MultipleEffectsAngleCannon : BasicCannon, DamageTakenCallback.IDamageTakenCallbackListener
	{
		private MultipleEffectsAngleCannonInfo MyInfo
		{
			get
			{
				return base.Info as MultipleEffectsAngleCannonInfo;
			}
		}

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			this.Angle = new Upgradeable(this.MyInfo.AngleUpgrade, (float)this.MyInfo.Angle, this.MyInfo.UpgradesValues);
			this.DrainLifePctFromTarget = new Upgradeable(this.MyInfo.DrainLifePctFromTargetUpgrade, this.MyInfo.DrainLifePctFromTarget, this.MyInfo.UpgradesValues);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this.Angle.SetLevel(upgradeName, level);
			this.DrainLifePctFromTarget.SetLevel(upgradeName, level);
		}

		protected override int FireGadget()
		{
			Vector3 a = this.DummyPosition();
			Vector3 a2 = Quaternion.Euler(0f, this.Angle * 0.5f, 0f) * this.Combat.transform.forward;
			Vector3 a3 = Quaternion.Euler(0f, -this.Angle * 0.5f, 0f) * this.Combat.transform.forward;
			Vector3 vector = a + a2 * this.GetRange();
			Vector3 a4 = a + a3 * this.GetRange();
			Vector3 vector2 = a4 - vector;
			float magnitude = vector2.magnitude;
			float num = magnitude / base.Radius;
			float num2 = magnitude / num;
			int num3 = -1;
			int num4 = 0;
			while ((float)num4 < num)
			{
				Vector3 vector3 = this.DummyPosition();
				Vector3 a5 = vector + num2 * (float)(num4 + 1) * vector2.normalized;
				Vector3 vector4 = a5 - vector3;
				vector4.y = 0f;
				Vector3 normalized = vector4.normalized;
				Vector3 target = vector3 + normalized * this.GetRange();
				target.y = vector3.y;
				EffectEvent effectEvent = base.GetEffectEvent(this.MyInfo.Effect);
				effectEvent.MoveSpeed = this._moveSpeed.Get();
				effectEvent.Range = this.GetRange();
				effectEvent.Origin = vector3;
				effectEvent.Target = target;
				effectEvent.Direction = normalized;
				effectEvent.TargetId = this.TargetId;
				effectEvent.LifeTime = ((base.LifeTime <= 0f) ? (effectEvent.Range / effectEvent.MoveSpeed) : base.LifeTime);
				effectEvent.Modifiers = this._damage;
				effectEvent.ExtraModifiers = ModifierData.CopyData(this.ExtraModifier);
				num3 = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
				base.ExistingFiredEffectsAdd(num3);
				num4++;
			}
			base.ExistingFiredEffectsRemoveAt(base.ExistingFiredEffectsCount() - 1);
			return num3;
		}

		protected override void OnPosDamageCaused(CombatObject causer, CombatObject taker, ModifierData mod, float amount, int eventid)
		{
			if (this.DrainLifePctFromTarget == 0f || mod.GadgetInfo.GadgetId != this.MyInfo.GadgetId || !mod.Info.Effect.IsHPDamage())
			{
				return;
			}
			base.FireExtraGadget(taker.Id.ObjId);
		}

		public void OnDamageTakenCallback(DamageTakenCallback evt)
		{
			if (this.DrainLifePctFromTarget == 0f || !evt.CauserCombatObject || evt.CauserCombatObject.Team != this.Combat.Team)
			{
				return;
			}
			base.DrainLifeCheck(evt.CauserCombatObject, evt.TakerCombatObject, true, evt.Amount, this.DrainLifePctFromTarget, evt.CauserEventId, this.MyInfo.LifeStealFeedback);
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
			Gizmos.DrawWireSphere(vector + this.Combat.transform.forward * (base.Radius * 0.5f), base.Radius * 0.5f);
		}

		protected Upgradeable DrainLifePctFromTarget;

		public Upgradeable Angle;
	}
}
