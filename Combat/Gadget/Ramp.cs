using System;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class Ramp : BasicCannon, TriggerEnterCallback.ITriggerEnterCallbackListener
	{
		public RampInfo MyInfo
		{
			get
			{
				return base.Info as RampInfo;
			}
		}

		protected override int FireGadget()
		{
			return base.FireGadget();
		}

		public override void OnTriggerEnterCallback(TriggerEnterCallback evt)
		{
			if (this != evt.Gadget || evt.Combat == this.Combat || base.ExistingFiredEffectsCount() == 0)
			{
				return;
			}
			if (!this.CanRamp(evt.Combat))
			{
				return;
			}
			base.DestroyExistingFiredEffects();
			this.FireExtraGadget();
		}

		protected override int FireExtraGadget()
		{
			EffectEvent effectEvent = base.GetEffectEvent(this.MyInfo.ExtraEffect);
			effectEvent.MoveSpeed = this._moveSpeed.Get();
			effectEvent.Range = this.GetRange();
			effectEvent.Origin = this.DummyPosition(effectEvent);
			effectEvent.TargetId = this.TargetId;
			effectEvent.LifeTime = ((base.LifeTime <= 0f) ? (effectEvent.Range / effectEvent.MoveSpeed) : base.LifeTime);
			effectEvent.Modifiers = this._damage;
			effectEvent.ExtraModifiers = ModifierData.CopyData(this.ExtraModifier);
			base.SetTargetAndDirection(this.Combat.Movement.LastVelocity.normalized, effectEvent);
			return GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
		}

		private bool CanRamp(CombatObject otherCombat)
		{
			if (this.Combat.Movement.LastSpeed < this.MyInfo.MinVelocityToJump)
			{
				return false;
			}
			Vector3 forward = this.Combat.transform.forward;
			if (Vector3.Dot(this.Combat.Movement.LastVelocity, forward) < 0f)
			{
				return false;
			}
			if (Vector3.Dot(this.Combat.Movement.LastVelocity, otherCombat.Transform.position - this.Combat.Transform.position) < 0f)
			{
				return false;
			}
			if (!PhysicsUtils.IsInFront(this.Combat.transform.position, forward, otherCombat.transform.position))
			{
				return false;
			}
			if ((otherCombat.CustomGadget1 is CaterpillarCharge || otherCombat.CustomGadget1 is GenkiDamaFollower) && otherCombat.CustomGadget1.ExistingFiredEffectsCount() > 0 && PhysicsUtils.IsInFront(otherCombat.transform.position, otherCombat.transform.forward, this.Combat.transform.position))
			{
				return false;
			}
			if (this.MyInfo.IgnoreRelativeVelocityCheck)
			{
				return true;
			}
			Vector3 lastVelocity = this.Combat.Movement.LastVelocity;
			Vector3 lastVelocity2 = this.Combat.Movement.LastVelocity;
			Vector3 vector = lastVelocity - lastVelocity2;
			float num = Vector3.Dot(vector.normalized, forward);
			float num2 = Math.Abs(num * vector.magnitude);
			return num2 >= this.MyInfo.MinVelocityToJump;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(Ramp));
	}
}
