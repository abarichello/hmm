using System;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class GenkiDamaFollower : GenkiDama
	{
		public GenkiDamaFollowerInfo MyInfo
		{
			get
			{
				return base.Info as GenkiDamaFollowerInfo;
			}
		}

		protected override int FireGadget()
		{
			int num = 0;
			Vector3 vector = this.Combat.transform.forward.normalized * this.ColliderRadius.Get();
			Vector3 vector2 = -this.Combat.transform.forward.normalized * this.ColliderRadius.Get();
			Vector3 vector3 = this.Combat.transform.right.normalized * this.ColliderRadius.Get();
			Vector3 vector4 = -this.Combat.transform.right.normalized * this.ColliderRadius.Get();
			for (int i = 0; i < this.MyInfo.EffectsToSpawnCount; i++)
			{
				EffectEvent effectEvent = base.GetEffectEvent(this.MyInfo.Effect);
				effectEvent.MoveSpeed = this._moveSpeed.Get();
				effectEvent.Range = this.GetRange();
				effectEvent.CustomVar = (byte)this.ColliderRadius.Get();
				effectEvent.Origin = this.DummyPosition() + vector;
				effectEvent.Target = vector;
				effectEvent.LifeTime = base.LifeTime;
				effectEvent.Direction = this.Combat.transform.forward.normalized;
				effectEvent.Modifiers = this._damage;
				effectEvent.ExtraModifiers = this.ExtraModifier;
				if (i == 0)
				{
					num = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
					this.TrailDropper.FireCannon(effectEvent.Origin, effectEvent.Direction, num, this.Combat);
					this.ActiveEffectsIds.Add(num);
				}
				else
				{
					if (i % 2 == 0)
					{
						Vector3 vector5 = vector + vector4 * (float)i + vector2 * (float)i;
						effectEvent.Origin = this.DummyPosition() + vector5;
						effectEvent.Target = vector5;
					}
					else
					{
						Vector3 vector6 = vector + vector3 * (float)(i + 1) + vector2 * (float)(i + 1);
						effectEvent.Origin = this.DummyPosition() + vector6;
						effectEvent.Target = vector6;
					}
					GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
				}
			}
			return num;
		}
	}
}
