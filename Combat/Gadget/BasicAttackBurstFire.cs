using System;
using HeavyMetalMachines.Event;
using Pocketverse;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class BasicAttackBurstFire : BasicCannon, TickCallback.ITickCallbackCallbackListener
	{
		private BasicAttackBurstFireInfo MyInfo
		{
			get
			{
				return base.Info as BasicAttackBurstFireInfo;
			}
		}

		protected override int FireGadget()
		{
			return this.FireCannon(this.MyInfo.CallbackEffect, this.GetRange() / this._moveSpeed.Get());
		}

		protected int FireCannon(FXInfo effectInfo, float lifeTime)
		{
			EffectEvent effectEvent = base.GetEffectEvent(effectInfo);
			effectEvent.Origin = this.Combat.Transform.position;
			effectEvent.LifeTime = lifeTime;
			effectEvent.Direction = this.Combat.transform.forward;
			effectEvent.Direction.y = 0f;
			effectEvent.Direction.Normalize();
			effectEvent.Target = effectEvent.Origin + effectEvent.Direction * this.GetRange();
			effectEvent.MoveSpeed = this._moveSpeed.Get();
			effectEvent.Modifiers = this._damage;
			effectEvent.Range = this.GetRange();
			effectEvent.Origin = this.DummyPosition();
			return GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
		}

		public void OnTickCallback(TickCallback evt)
		{
			if (evt.Effect.Gadget != this)
			{
				return;
			}
			this.FireCannon(this.MyInfo.Effect, this.MyInfo.CallbackLifeTime);
		}
	}
}
