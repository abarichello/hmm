using System;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class GadgetOverheatUpdater : GadgetPressedUpdater
	{
		public GadgetOverheatUpdater(HMMHub hub, GadgetBehaviour gadgetBehaviour, Action beforeBaseGadgetFixedUpdate, Action baseGadgetFixedUpdate, Func<int> warmupCallback, Func<int> fireCallback, Func<int> fireExtraCallback, Action<int> onGadgetUsed) : base(hub, gadgetBehaviour, beforeBaseGadgetFixedUpdate, baseGadgetFixedUpdate, warmupCallback, fireCallback, fireExtraCallback, onGadgetUsed)
		{
		}

		public bool IsOverheated { get; private set; }

		public bool IsCooling { get; private set; }

		public bool WasExecuting { get; private set; }

		public override void ObjectUnspawned(UnspawnEvent evt)
		{
			base.ObjectUnspawned(evt);
			if (!this._hub || this._hub.Net.IsClient())
			{
				return;
			}
			SpawnController bitComponent = this._gadget.Combat.Id.GetBitComponent<SpawnController>();
			long num = (long)bitComponent.GetPlayerMaxTimeDeadMillis();
			float num2 = (!this.IsOverheated) ? this._gadget.OverheatCoolingRate : this._gadget.OverheatUnblockRate;
			float num3 = this._gadget.CurrentHeat - num2 * (float)num / 1000f;
			this._gadget.CurrentHeat = Mathf.Clamp01(num3);
			this._gadget.UpdateGadgetStateObjectHeat();
			this._gadget.Toggled = false;
			if (this._gadget.CurrentHeat <= 0f)
			{
				this.IsOverheated = false;
				this.IsCooling = false;
			}
		}

		public override void RunGadgetUpdate()
		{
			if (!this._hub || this._hub.Net.IsClient() || !this._gadget.Activated)
			{
				return;
			}
			if (this.IsOverheated)
			{
				this._gadget.CurrentHeat -= this._gadget.OverheatUnblockRate * Time.deltaTime;
				if (this._gadget.CurrentHeat > 0f)
				{
					this._baseGadgetFixedUpdate();
					return;
				}
				this.IsOverheated = false;
				this.IsCooling = false;
				this._gadget.CurrentHeat = 0f;
			}
			if (base.HasEffect)
			{
				if (!this.WasExecuting)
				{
					this.WasExecuting = true;
					this._gadget.CurrentHeat += this._gadget.ActivationOverheatHeatCost;
				}
				this.IsCooling = false;
				this._gadget.CurrentHeat += this._gadget.OverheatHeatRate * Time.deltaTime;
				if (this._gadget.CurrentHeat >= 1f)
				{
					this._gadget.CurrentHeat = 1f;
					this.IsOverheated = true;
					this.StopGadget();
				}
			}
			else
			{
				float num = (float)this._hub.GameTime.GetPlaybackTime() / 1000f;
				if (this.WasExecuting)
				{
					this.WasExecuting = false;
					this._timeToStartCooling = num + this._gadget.OverheatDelayBeforeCooling;
				}
				this.IsCooling = false;
				if (this._gadget.CurrentHeat > 0f && num >= this._timeToStartCooling)
				{
					this.IsCooling = true;
					this._gadget.CurrentHeat = Mathf.Clamp01(this._gadget.CurrentHeat - this._gadget.OverheatCoolingRate * Time.deltaTime);
				}
			}
			if (!this.IsOverheated)
			{
				base.RunGadgetUpdate();
			}
		}

		private float _timeToStartCooling;
	}
}
