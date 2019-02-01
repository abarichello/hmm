using System;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class BuffOnSpeed : BasicCannon, TriggerEnterCallback.ITriggerEnterCallbackListener
	{
		public int[] Healed
		{
			get
			{
				return this._gadgetState.AffectedIds;
			}
		}

		public BuffOnSpeedInfo MyInfo
		{
			get
			{
				return base.Info as BuffOnSpeedInfo;
			}
		}

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			this._activationSpeed = new Upgradeable(this.MyInfo.ActivationSpeedUpgrade, this.MyInfo.ActivationSpeed, this.MyInfo.UpgradesValues);
			this._activationTime = new Upgradeable(this.MyInfo.ActivationTimeUpgrade, this.MyInfo.ActivationTime, this.MyInfo.UpgradesValues);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this._activationSpeed.SetLevel(upgradeName, level);
			this._activationTime.SetLevel(upgradeName, level);
		}

		protected override void GadgetUpdate()
		{
			if (this.Combat.Movement.LastSpeed >= this._activationSpeed)
			{
				this._currentActivationTime += Time.deltaTime;
				if (this._currentActivationTime >= this._activationTime && base.ExistingFiredEffectsCount() == 0)
				{
					base.Pressed = true;
				}
			}
			else
			{
				this._currentActivationTime -= Time.deltaTime;
			}
			if (this._currentActivationTime <= 0f)
			{
				base.Pressed = false;
			}
			this._currentActivationTime = Mathf.Clamp(this._currentActivationTime, 0f, this.MyInfo.FullBarTime);
			base.GadgetUpdate();
			this.Combat.GadgetStates.SetJokerBarState(this._currentActivationTime, (base.ExistingFiredEffectsCount() <= 0 || this._activationTime <= 0f) ? this._activationTime : this.MyInfo.FullBarTime);
		}

		public override void OnTriggerEnterCallback(TriggerEnterCallback evt)
		{
			if (evt.Combat != this.Combat && evt.Combat.Team == this.Combat.Team)
			{
				base.FireExtraGadget(evt.Combat.Id.ObjId);
			}
		}

		public override void OnObjectUnspawned(UnspawnEvent evt)
		{
			base.OnObjectUnspawned(evt);
			base.Pressed = false;
			this._currentActivationTime = 0f;
			this.Combat.GadgetStates.SetJokerBarState(this._currentActivationTime, this.MyInfo.FullBarTime);
		}

		private float _currentActivationTime;

		private Upgradeable _activationSpeed;

		private Upgradeable _activationTime;
	}
}
