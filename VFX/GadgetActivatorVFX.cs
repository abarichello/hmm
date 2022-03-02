using System;
using HeavyMetalMachines.Combat.Gadget;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class GadgetActivatorVFX : BaseVFX
	{
		protected override void OnActivate()
		{
			GadgetActivatorVFX.GadgetInteractionOwner gadgetInteractionOwner = this.gadgetInteractionOwner;
			if (gadgetInteractionOwner != GadgetActivatorVFX.GadgetInteractionOwner.effectOwner)
			{
				if (gadgetInteractionOwner == GadgetActivatorVFX.GadgetInteractionOwner.effectTarget)
				{
					this.gadgetActivatorVFXCtrl = this._targetFXInfo.Target.GetComponentInChildren<GadgetActivatorVFXCtrl>();
				}
			}
			else
			{
				this.gadgetActivatorVFXCtrl = this._targetFXInfo.Owner.GetComponentInChildren<GadgetActivatorVFXCtrl>();
			}
			if (!this.gadgetActivatorVFXCtrl)
			{
				return;
			}
			this.gadgetActivatorVFXCtrl.Activate(base.GetInstanceID());
		}

		protected override void OnDeactivate()
		{
			if (!this.gadgetActivatorVFXCtrl)
			{
				return;
			}
			this.gadgetActivatorVFXCtrl.Deactivate(base.GetInstanceID());
		}

		protected override void WillDeactivate()
		{
			if (!this.gadgetActivatorVFXCtrl)
			{
				return;
			}
			this.gadgetActivatorVFXCtrl.Deactivate(base.GetInstanceID());
		}

		[SerializeField]
		private GadgetActivatorVFX.GadgetInteractionOwner gadgetInteractionOwner;

		private GadgetActivatorVFXCtrl gadgetActivatorVFXCtrl;

		private enum GadgetInteractionOwner
		{
			effectOwner,
			effectTarget
		}
	}
}
