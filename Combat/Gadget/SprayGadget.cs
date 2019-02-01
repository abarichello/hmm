using System;
using HeavyMetalMachines.Event;
using Pocketverse;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class SprayGadget : BasicCannon
	{
		public SprayGadgetInfo MyInfo
		{
			get
			{
				return base.Info as SprayGadgetInfo;
			}
		}

		protected override int FireGadget()
		{
			if (this._lastSprayId != -1)
			{
				EffectRemoveEvent content = new EffectRemoveEvent
				{
					TargetEventId = this._lastSprayId,
					DestroyReason = BaseFX.EDestroyReason.Gadget
				};
				GameHubBehaviour.Hub.Events.TriggerEvent(content);
			}
			this._lastSprayId = this.FireSpray();
			return -1;
		}

		private int FireSpray()
		{
			EffectEvent effectEvent = base.GetEffectEvent(this.MyInfo.Effect);
			effectEvent.MoveSpeed = this._moveSpeed.Get();
			effectEvent.Range = this.GetRange();
			effectEvent.Origin = GadgetBehaviour.DummyPosition(this.Combat, this.MyInfo.Effect);
			effectEvent.Origin.y = 0f;
			effectEvent.Target = base.Target;
			effectEvent.TargetId = this.TargetId;
			effectEvent.LifeTime = base.LifeTime;
			effectEvent.Modifiers = this._damage;
			effectEvent.ExtraModifiers = this.ExtraModifier;
			base.SetTargetAndDirection(effectEvent);
			return GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
		}

		private static readonly BitLogger Log = new BitLogger(typeof(SprayGadget));

		private int _lastSprayId = -1;
	}
}
