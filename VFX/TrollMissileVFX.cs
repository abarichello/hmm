using System;
using Pocketverse.Util;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class TrollMissileVFX : BaseVFX
	{
		protected void Awake()
		{
			base.enabled = false;
		}

		protected void LateUpdate()
		{
			if (this._time <= this._totalTime)
			{
				this._time += Time.deltaTime;
				float num = this._time / this._totalTime;
				Vector3 position = this._targetFXInfo.Target.transform.position;
				Vector3 position2 = Vector3.Lerp(this._origPos, position, num);
				position2.y = ((num > 0.5f) ? Mathf.Lerp(this.MaxHeight, this._origHeight, LerpFunc.QuadIn(num - 0.5f, 0f, 1f, 0.5f)) : Mathf.Lerp(this._origHeight, this.MaxHeight, LerpFunc.QuadOut(num, 0f, 1f, 0.5f)));
				base.transform.position = position2;
			}
		}

		protected override void OnActivate()
		{
			this._totalTime = this._targetFXInfo.Gadget.LifeTime;
			this._time = 0f;
			this._origPos = this._targetFXInfo.Owner.transform.position;
			this._origHeight = this._origPos.y;
			base.enabled = true;
		}

		protected override void WillDeactivate()
		{
		}

		protected override void OnDeactivate()
		{
			base.enabled = false;
		}

		public float MaxHeight = 10f;

		private float _totalTime;

		private float _time;

		private Vector3 _origPos = Vector3.zero;

		private float _origHeight;
	}
}
