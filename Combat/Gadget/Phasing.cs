using System;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class Phasing : GadgetBehaviour
	{
		public PhasingInfo MyInfo
		{
			get
			{
				return base.Info as PhasingInfo;
			}
		}

		protected override void Awake()
		{
			base.Awake();
			if (GameHubBehaviour.Hub)
			{
				this.CurrentCooldownTime = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			}
		}

		public override void SetInfo(GadgetInfo gadget)
		{
			base.SetInfo(gadget);
			PhasingInfo myInfo = this.MyInfo;
			this._buffs = ModifierData.CreateData(myInfo.Damage, myInfo);
			this._lifeTimeUpgradeable = new Upgradeable(myInfo.LifeTimeUpgrade, myInfo.LifeTime, myInfo.UpgradesValues);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			for (int i = 0; i < this._buffs.Length; i++)
			{
				this._buffs[i].SetLevel(upgradeName, level);
			}
			this._lifeTimeUpgradeable.SetLevel(upgradeName, level);
		}

		private void ActivatePhasing()
		{
			if (!base.Activated)
			{
				return;
			}
			PhasingInfo myInfo = this.MyInfo;
			EffectEvent effectEvent = base.GetEffectEvent(myInfo.Effect);
			effectEvent.Origin = this.Combat.transform.position;
			effectEvent.Target = Vector3.zero;
			effectEvent.Direction = this.Combat.transform.forward;
			effectEvent.LifeTime = this._lifeTimeUpgradeable;
			effectEvent.Modifiers = this._buffs;
			this._currentPhasing = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
			base.OnGadgetUsed(this._currentPhasing);
			if (this.FireNormalAndExtraEffectsTogether.BoolGet())
			{
				this.FireExtraGadget();
			}
		}

		protected override int FireExtraGadget()
		{
			EffectEvent effectEvent = base.GetEffectEvent(this.MyInfo.ExtraEffect);
			effectEvent.Range = this.GetRange();
			effectEvent.Origin = this.Combat.transform.position;
			effectEvent.Target = Vector3.zero;
			effectEvent.LifeTime = this._lifeTimeUpgradeable;
			effectEvent.Modifiers = ModifierData.CopyData(this.ExtraModifier);
			return GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
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
			this.ActivatePhasing();
		}

		protected override void InnerOnDestroyEffect(DestroyEffectMessage evt)
		{
			if (evt.RemoveData.TargetEventId != this._currentPhasing)
			{
				return;
			}
		}

		private ModifierData[] _buffs;

		private int _currentPhasing;

		private Upgradeable _lifeTimeUpgradeable;
	}
}
