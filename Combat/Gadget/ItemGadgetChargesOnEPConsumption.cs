using System;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class ItemGadgetChargesOnEPConsumption : GadgetBehaviour, EPComsuptionCallback.IEPComsuptionCallbackListener
	{
		private ItemGadgetChargesOnEPConsumptionInfo MyInfo
		{
			get
			{
				return base.Info as ItemGadgetChargesOnEPConsumptionInfo;
			}
		}

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			this._upgChargeMax = new Upgradeable(this.MyInfo.ChargeMaxUpgrade, (float)this.MyInfo.ChargeMax, info.UpgradesValues);
			this._upgChargeModifiers = ModifierData.CreateData(this.MyInfo.ChargeModifiers, info);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this._upgChargeMax.SetLevel(upgradeName, level);
			this._upgChargeModifiers.SetLevel(upgradeName, level);
			this.ChargeCount = 0;
		}

		public override void Activate()
		{
			base.Activate();
			this._transform = this.Combat.transform;
			this.CreateListener();
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

		private void CreateListener()
		{
			ItemGadgetChargesOnEPConsumptionInfo myInfo = this.MyInfo;
			EffectEvent effectEvent = base.GetEffectEvent(myInfo.ListenerEffect);
			effectEvent.Origin = (effectEvent.Target = this._transform.position);
			effectEvent.Direction = base.CalcDirection(effectEvent.Origin, effectEvent.Target + this._transform.forward);
			effectEvent.TargetId = this.Combat.Id.ObjId;
			effectEvent.Range = this.GetRange();
			GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
		}

		protected override int FireGadget()
		{
			if (this.ChargeCount == 0)
			{
				return -1;
			}
			ItemGadgetChargesOnEPConsumptionInfo myInfo = this.MyInfo;
			EffectEvent effectEvent = base.GetEffectEvent(myInfo.ChargeUsageEffect);
			effectEvent.Origin = (effectEvent.Target = this._transform.position);
			effectEvent.Direction = base.CalcDirection(effectEvent.Origin, effectEvent.Target + this._transform.forward);
			effectEvent.TargetId = this.Combat.Id.ObjId;
			effectEvent.Modifiers = ModifierData.CreateConvoluted(this._upgChargeModifiers, (float)this.ChargeCount);
			int result = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
			this.RemoveAllCharges();
			return result;
		}

		public void OnEPComsuptionCallback(EPComsuptionCallback evt)
		{
			if (evt.Amount == 0)
			{
				return;
			}
			if (!this.Combat.IsAlive())
			{
				return;
			}
			this.AddCharge();
		}

		private void AddCharge()
		{
			this.ChargeCount = Mathf.Min(this.ChargeCount + 1, base.MaxChargeCount);
		}

		private void RemoveAllCharges()
		{
			this.ChargeCount = 0;
		}

		protected Upgradeable _upgChargeMax;

		protected ModifierData[] _upgChargeModifiers;

		private Transform _transform;
	}
}
