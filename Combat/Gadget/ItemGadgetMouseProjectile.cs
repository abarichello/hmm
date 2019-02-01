using System;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class ItemGadgetMouseProjectile : GadgetBehaviour
	{
		public ItemGadgetMouseProjectileInfo MyInfo
		{
			get
			{
				return base.Info as ItemGadgetMouseProjectileInfo;
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

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			this._upgModifiers = ModifierData.CreateData(this.MyInfo.Modifiers, info);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this._upgModifiers.SetLevel(upgradeName, level);
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
			ItemGadgetMouseProjectileInfo myInfo = this.MyInfo;
			EffectEvent effectEvent = base.GetEffectEvent(myInfo.Effect);
			effectEvent.Range = this.GetRange();
			effectEvent.MoveSpeed = myInfo.MoveSpeed;
			effectEvent.Origin = this._transform.position;
			effectEvent.Modifiers = this._upgModifiers;
			effectEvent.Target = base.Target;
			effectEvent.Direction = base.CalcDirection(effectEvent.Origin, effectEvent.Target);
			return GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
		}

		public static readonly BitLogger Log = new BitLogger(typeof(ItemGadgetMouseProjectile));

		private ModifierData[] _upgModifiers;

		private Transform _transform;
	}
}
