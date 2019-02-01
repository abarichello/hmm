using System;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class GadgetWithChargesUpdater : IGadgetUpdater
	{
		public GadgetWithChargesUpdater(HMMHub hub, GadgetBehaviour gadgetBehaviour, Action baseGadgetFixedUpdate, Func<int> fireCallback)
		{
			this._hub = hub;
			this._baseGadgetFixedUpdate = baseGadgetFixedUpdate;
			this._gadget = gadgetBehaviour;
			this._fireCallback = fireCallback;
		}

		public GadgetWithChargesUpdater(HMMHub hub, GadgetBehaviour gadgetBehaviour, Action baseGadgetFixedUpdate, Func<int> fireCallback, Action<int> onGadgetUsed)
		{
			this._hub = hub;
			this._baseGadgetFixedUpdate = baseGadgetFixedUpdate;
			this._gadget = gadgetBehaviour;
			this._fireCallback = fireCallback;
			this._onGadgetUsed = onGadgetUsed;
		}

		public void RunGadgetUpdate()
		{
			if (!this._hub || this._hub.Net.IsClient())
			{
				return;
			}
			this._gadget.CurrentTime = (long)this._hub.GameTime.GetPlaybackTime();
			if (this._gadget.Combat.GadgetStates)
			{
				this._gadget.Combat.GadgetStates.SetJokerBarState((float)this._gadget.ChargeCount, (float)this._gadget.ChargeCount);
			}
			if (this._gadget.Activated && this._gadget.ChargeTime <= this._gadget.CurrentTime)
			{
				this.AddCharge();
			}
			else if (this._gadget.ChargeCount == this._gadget.MaxChargeCount)
			{
				this._gadget.ChargeTime = this._gadget.CurrentTime + (long)(this.ChargeCooldown() * 1000f);
			}
			if (this._gadget.CurrentCooldownTime <= this._gadget.CurrentTime)
			{
				if (!this._gadget.Pressed)
				{
					this._gadget.CurrentCooldownTime = this._gadget.CurrentTime;
				}
				else if (this._gadget.ChargeCount > 0 && this._gadget.Combat.Data.CanSpendEP((float)this._gadget.ActivationCost))
				{
					int num = this._fireCallback();
					if (num != -1)
					{
						this._gadget.Combat.Controller.ConsumeEP((float)this._gadget.ActivationCost);
						this.ConsumeCharge();
						this.StartCooldown(this._gadget.CurrentTime);
						this._onGadgetUsed(num);
					}
				}
			}
			this._baseGadgetFixedUpdate();
		}

		private void FillCharges()
		{
			this._gadget.ChargeCount = this._gadget.MaxChargeCount;
			this._gadget.ChargeTime = this._gadget.CurrentTime;
		}

		private void AddCharge()
		{
			this._gadget.ChargeTime = this._gadget.CurrentTime + (long)(this.ChargeCooldown() * 1000f);
			this._gadget.ChargeCount = Mathf.Min(this._gadget.ChargeCount + 1, this._gadget.MaxChargeCount);
		}

		private void ConsumeCharge()
		{
			this._gadget.ChargeCount = Mathf.Max(this._gadget.ChargeCount - 1, 0);
		}

		private void StartCooldown(long time)
		{
			long num = time - this._gadget.CurrentCooldownTime;
			this._gadget.CurrentCooldownTime = (long)(this._gadget.Cooldown * 1000f) + time - num;
		}

		private float ChargeCooldown()
		{
			CombatAttributes attributes = this._gadget.Combat.Attributes;
			GadgetSlot slot = this._gadget.Slot;
			if (slot == GadgetSlot.CustomGadget0)
			{
				return this._gadget.MaxChargeTime * (1f - attributes.CooldownReductionGadget0Pct) - attributes.CooldownReductionGadget0;
			}
			if (slot == GadgetSlot.CustomGadget1)
			{
				return this._gadget.MaxChargeTime * (1f - attributes.CooldownReductionGadget1Pct) - attributes.CooldownReductionGadget1;
			}
			if (slot != GadgetSlot.CustomGadget2)
			{
				return this._gadget.MaxChargeTime;
			}
			return this._gadget.MaxChargeTime * (1f - attributes.CooldownReductionGadget2Pct) - attributes.CooldownReductionGadget2;
		}

		public void DestroyEffect(DestroyEffect evt)
		{
		}

		public void ObjectUnspawned(UnspawnEvent evt)
		{
		}

		public void ObjectSpawned(SpawnEvent evt)
		{
			this.FillCharges();
		}

		private readonly HMMHub _hub;

		private readonly GadgetBehaviour _gadget;

		private readonly Action _baseGadgetFixedUpdate;

		private readonly Func<int> _fireCallback;

		private readonly Action<int> _onGadgetUsed;
	}
}
