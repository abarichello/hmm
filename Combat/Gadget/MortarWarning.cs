using System;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class MortarWarning : BounceableProjectile
	{
		private MortarWarningInfo MyInfo
		{
			get
			{
				return base.Info as MortarWarningInfo;
			}
		}

		protected override int FireGadget()
		{
			Vector3 vector = this.DummyPosition();
			float range = this.GetRange();
			Vector3 a = base.Target;
			a.y = vector.y;
			a -= vector;
			Vector3 normalized = a.normalized;
			float magnitude = a.magnitude;
			Vector3 targetPos;
			if (magnitude > range)
			{
				targetPos = vector + normalized * range;
			}
			else
			{
				targetPos = base.Target;
			}
			return this.FireCannonFromTo(vector, targetPos);
		}

		protected virtual int FireCannonFromTo(Vector3 originPos, Vector3 targetPos)
		{
			return this.StartEffect(originPos, targetPos, false, false, false, default(DestroyEffect), 0, -1f);
		}

		protected virtual int StartEffect(Vector3 originPosition, Vector3 finalPosition, bool isReflectable, bool isReflected, bool isAdditionalBounce, DestroyEffect destroyEffectEvent, byte customVar = 0, float paramLifeTime = -1f)
		{
			finalPosition.y = 0f;
			float magnitude = (finalPosition - originPosition).magnitude;
			Vector3 normalized = (finalPosition - originPosition).normalized;
			EffectEvent effectEvent = base.GetEffectEvent(this.MyInfo.Effect);
			effectEvent.Origin = originPosition;
			effectEvent.Direction = normalized;
			effectEvent.Range = magnitude;
			effectEvent.Target = finalPosition;
			effectEvent.LifeTime = ((paramLifeTime <= 0f) ? base.LifeTime : paramLifeTime);
			effectEvent.MoveSpeed = effectEvent.Range / effectEvent.LifeTime;
			effectEvent.Modifiers = ((!isAdditionalBounce) ? this._damage : this._bounceDamage);
			this.LastEffectId = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
			base.OnGadgetUsed(this.LastEffectId);
			return this.LastEffectId;
		}

		protected override int FireExtraGadget()
		{
			return this.FireExtraGadget(this.LastEffectPosition);
		}

		public override float GetDps()
		{
			return base.GetDpsFromModifierData(this._damage);
		}

		private static readonly BitLogger Log = new BitLogger(typeof(MortarWarning));
	}
}
