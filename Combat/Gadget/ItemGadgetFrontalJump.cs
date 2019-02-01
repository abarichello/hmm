using System;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class ItemGadgetFrontalJump : GadgetBehaviour
	{
		private ItemGadgetFrontalJumpInfo MyInfo
		{
			get
			{
				return base.Info as ItemGadgetFrontalJumpInfo;
			}
		}

		private void Start()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			this._transform = this.Combat.transform;
		}

		protected override void Awake()
		{
			base.Awake();
			this.CurrentCooldownTime = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
		}

		protected override void GadgetUpdate()
		{
			if (!GameHubBehaviour.Hub || GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			this.CurrentTime = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			base.GadgetUpdate();
			if (this.CurrentCooldownTime > this.CurrentTime)
			{
				return;
			}
			if (!base.Pressed)
			{
				this.CurrentCooldownTime = this.CurrentTime;
				return;
			}
			if (!this.Combat.Controller.ConsumeEP((float)base.ActivationCost))
			{
				return;
			}
			long num = this.CurrentTime - this.CurrentCooldownTime;
			this.CurrentCooldownTime = (long)(this.Cooldown * 1000f) + this.CurrentTime - num;
			this.FireGadget();
		}

		protected override int FireGadget()
		{
			Vector3 position = this._transform.position;
			Vector3 forward = this._transform.forward;
			ItemGadgetFrontalJumpInfo myInfo = this.MyInfo;
			EffectEvent effectEvent = base.GetEffectEvent(myInfo.Effect);
			effectEvent.Origin = position;
			effectEvent.Direction = forward;
			effectEvent.TargetId = this.Combat.Id.ObjId;
			Vector3 target = position + forward * this.GetRange();
			target.y = 0f;
			effectEvent.Target = target;
			effectEvent.Target = base.GetValidPosition(effectEvent.Origin, effectEvent.Target);
			effectEvent.Range = Vector3.Distance(effectEvent.Origin, effectEvent.Target);
			effectEvent.LifeTime = effectEvent.Range / this.GetRange() * base.LifeTime;
			return GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
		}

		private static readonly BitLogger Log = new BitLogger(typeof(ItemGadgetFrontalJump));

		private Transform _transform;
	}
}
