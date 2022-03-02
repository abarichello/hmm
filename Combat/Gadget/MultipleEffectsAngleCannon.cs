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
			Vector3 vector = this.DummyPosition();
			Vector3 vector2 = Quaternion.Euler(0f, this.Angle * 0.5f, 0f) * this.Combat.transform.forward;
			Vector3 vector3 = Quaternion.Euler(0f, -this.Angle * 0.5f, 0f) * this.Combat.transform.forward;
			Vector3 vector4 = vector + vector2 * this.GetRange();
			Vector3 vector5 = vector + vector3 * this.GetRange();
			Vector3 vector6 = vector5 - vector4;
			float magnitude = vector6.magnitude;
			float num = magnitude / base.Radius;
			float num2 = magnitude / num;
			int num3 = -1;
			int num4 = 0;
			while ((float)num4 < num)
			{
				Vector3 vector7 = this.DummyPosition();
				Vector3 vector8 = vector4 + num2 * (float)(num4 + 1) * vector6.normalized;
				Vector3 vector9 = vector8 - vector7;
				vector9.y = 0f;
				Vector3 normalized = vector9.normalized;
				Vector3 target = vector7 + normalized * this.GetRange();
				target.y = vector7.y;
				EffectEvent effectEvent = base.GetEffectEvent(this.MyInfo.Effect);
				effectEvent.MoveSpeed = this._moveSpeed.Get();
				effectEvent.Range = this.GetRange();
				effectEvent.Origin = vector7;
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
			Vector3 vector = Quaternion.Euler(0f, this.Angle * 0.5f, 0f) * this.Combat.transform.forward;
			Vector3 vector2 = Quaternion.Euler(0f, -this.Angle * 0.5f, 0f) * this.Combat.transform.forward;
			Gizmos.color = Color.red;
			float range = this.GetRange();
			Vector3 vector3 = this.DummyPosition();
			Gizmos.DrawLine(vector3, vector3 + vector * range);
			Gizmos.DrawLine(vector3, vector3 + Vector3.up * 10f);
			Gizmos.DrawLine(vector3 + Vector3.up * 10f, vector3 + vector * range + Vector3.up * 10f);
			Gizmos.DrawLine(vector3 + vector * range, vector3 + vector * range + Vector3.up * 10f);
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(vector3, vector3 + vector2 * range);
			Gizmos.DrawLine(vector3 + vector2 * range, vector3 + vector2 * range + Vector3.up * 10f);
			Gizmos.DrawLine(vector3 + Vector3.up * 10f, vector3 + vector2 * range + Vector3.up * 10f);
			Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
			Gizmos.DrawSphere(vector3, range);
			Gizmos.color = Color.blue;
			Gizmos.DrawWireSphere(vector3 + this.Combat.transform.forward * (base.Radius * 0.5f), base.Radius * 0.5f);
		}

		protected Upgradeable DrainLifePctFromTarget;

		public Upgradeable Angle;
	}
}
