using System;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class MortarDamage : BasicCannon
	{
		private MortarDamageInfo MyInfo
		{
			get
			{
				return base.Info as MortarDamageInfo;
			}
		}

		protected override int FireGadget()
		{
			MortarDamageInfo myInfo = this.MyInfo;
			EffectEvent effectEvent = base.GetEffectEvent(myInfo.Effect);
			Transform transform = this.Combat.transform;
			Vector3 position = transform.position;
			float range = this.GetRange();
			float num = this._moveSpeed;
			float lifeTime = myInfo.LifeTime;
			bool fixedMoveSpeed = myInfo.FixedMoveSpeed;
			Vector3 vector = base.Target;
			vector.y = position.y;
			vector -= position;
			Vector3 normalized = vector.normalized;
			float num2 = vector.magnitude;
			Vector3 target;
			if (num2 > range)
			{
				num2 = range;
				target = position + normalized * range;
			}
			else
			{
				target = base.Target;
			}
			target.y = 0f;
			effectEvent.Origin = position;
			effectEvent.Direction = normalized;
			effectEvent.Target = target;
			if (fixedMoveSpeed)
			{
				effectEvent.LifeTime = num2 / num;
			}
			else
			{
				effectEvent.LifeTime = lifeTime;
			}
			float explosionRadius = myInfo.ExplosionRadius;
			effectEvent.Modifiers = this._damage;
			effectEvent.Range = explosionRadius;
			return GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
		}

		private static readonly BitLogger Log = new BitLogger(typeof(MortarDamage));
	}
}
