using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Utils.Scale;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class ScaleObjectOnHeatVFX : BaseVFX
	{
		protected void Awake()
		{
			base.enabled = false;
			this._scaleVector = new ScaleVectorByFactor(this._startingValue, this._finalValue);
		}

		protected override void OnActivate()
		{
			base.enabled = true;
			base.transform.localScale = this._startingValue;
			this._scaleVector.SetValuesAndReset(this._startingValue, this._finalValue);
			this.CanCollectToCache = false;
		}

		private void Update()
		{
			if (this.CanCollectToCache)
			{
				return;
			}
			this._shouldCollect = (this._targetFXInfo.Gadget == null || this._targetFXInfo.Gadget.Combat.GadgetStates.GetGadgetState(this._targetFXInfo.Gadget.Slot).Heat >= 1f);
			if (this._shouldCollect)
			{
				this.CanCollectToCache = true;
				base.enabled = false;
			}
			else
			{
				base.transform.localScale = this._scaleVector.Update(this._targetFXInfo.Gadget.Combat.GadgetStates.GetGadgetState(this._targetFXInfo.Gadget.Slot).Heat);
			}
		}

		private float GetCurrentHeat()
		{
			GadgetData.GadgetStateObject gadgetState = this._targetFXInfo.Gadget.Combat.GadgetStates.GetGadgetState(this._targetFXInfo.Gadget.Slot);
			return gadgetState.Heat;
		}

		protected override void WillDeactivate()
		{
		}

		protected override void OnDeactivate()
		{
		}

		[SerializeField]
		private Vector3 _startingValue = Vector3.zero;

		[SerializeField]
		private Vector3 _finalValue = Vector3.one;

		[NonSerialized]
		private ScaleVectorByFactor _scaleVector;

		[NonSerialized]
		private bool _shouldCollect;
	}
}
