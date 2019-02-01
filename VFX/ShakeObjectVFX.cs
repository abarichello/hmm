using System;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class ShakeObjectVFX : BaseVFX
	{
		protected override void OnActivate()
		{
		}

		private void Update()
		{
			if (this._targetFXInfo.Gadget == null || this._targetFXInfo.Gadget.Combat.GadgetStates.GetGadgetState(this._targetFXInfo.Gadget.Slot).Heat < 1f)
			{
				return;
			}
			this.ShakeEffect();
		}

		private void ShakeEffect()
		{
			float time = Time.time;
			float num = time - this._lastShakeTime;
			if (num < this._shakeInterval)
			{
				return;
			}
			this._lastShakeTime = time;
			float value = UnityEngine.Random.value;
			Vector3 zero = Vector3.zero;
			if (value < 0.5f)
			{
				zero.x += ((!this._canShakeLeft) ? this._shakeOffsetAmount : (-this._shakeOffsetAmount));
				this._canShakeLeft = !this._canShakeLeft;
			}
			else
			{
				zero.z += ((!this._canShakeForward) ? (-this._shakeOffsetAmount) : this._shakeOffsetAmount);
				this._canShakeForward = !this._canShakeForward;
			}
			base.transform.localPosition += zero;
		}

		private void OnDisable()
		{
			base.transform.localPosition = Vector3.zero;
		}

		protected override void WillDeactivate()
		{
		}

		protected override void OnDeactivate()
		{
		}

		[SerializeField]
		private float _shakeOffsetAmount = 0.4f;

		[SerializeField]
		private float _shakeInterval = 0.03f;

		[NonSerialized]
		private float _lastShakeTime;

		[NonSerialized]
		private bool _canShakeLeft;

		[NonSerialized]
		private bool _canShakeForward;
	}
}
