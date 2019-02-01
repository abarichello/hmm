using System;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class BlackLotusDamageAreaOnLifeTime : GadgetBehaviour
	{
		public BlackLotusDamageAreaOnLifeTimeInfo MyInfo
		{
			get
			{
				return base.Info as BlackLotusDamageAreaOnLifeTimeInfo;
			}
		}

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			this._upgAreaModifiers = ModifierData.CreateData(this.MyInfo.AreaModifiers, this.MyInfo);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this._upgAreaModifiers.SetLevel(upgradeName, level);
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
			if (this.CurrentCooldownTime == 0L || !base.Pressed)
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
			base.OnGadgetUsed(this.FireGadget());
		}

		protected override int FireGadget()
		{
			EffectEvent effectEvent = base.GetEffectEvent(this.MyInfo.AreaEffect);
			Vector3 position = this.Combat.Transform.position;
			Vector3 vector = base.Target - position;
			vector.y = 0f;
			Vector3 normalized = vector.normalized;
			float magnitude = vector.magnitude;
			Vector3 origin;
			if (magnitude > this.MyInfo.CastRange)
			{
				origin = position + normalized * this.MyInfo.CastRange;
			}
			else
			{
				origin = base.Target;
			}
			origin.y = 0f;
			effectEvent.Origin = origin;
			effectEvent.Range = this.MyInfo.AreaRange;
			effectEvent.LifeTime = base.LifeTime;
			effectEvent.Modifiers = this._upgAreaModifiers;
			return GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
		}

		public static readonly BitLogger Log = new BitLogger(typeof(BlackLotusDamageAreaOnLifeTime));

		protected ModifierData[] _upgAreaModifiers;
	}
}
