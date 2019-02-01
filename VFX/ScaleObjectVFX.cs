using System;
using HeavyMetalMachines.Utils.Scale;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class ScaleObjectVFX : BaseVFX
	{
		protected void Awake()
		{
			base.enabled = false;
			this._scaleVector = new ScaleVectorOverTime(this._startingValue, this._finalValue, this._scaleDuration);
		}

		protected override void OnActivate()
		{
			base.enabled = true;
			base.transform.localScale = this._startingValue;
			this._scaleVector.SetValuesAndReset(this._startingValue, this._finalValue, this._scaleDuration);
			this.CanCollectToCache = false;
		}

		private void Update()
		{
			if (this.CanCollectToCache)
			{
				return;
			}
			base.transform.localScale = this._scaleVector.Update();
			if (this._scaleVector.Finished)
			{
				this.CanCollectToCache = true;
				base.enabled = false;
			}
		}

		protected override void WillDeactivate()
		{
		}

		protected override void OnDeactivate()
		{
		}

		private static readonly BitLogger Log = new BitLogger(typeof(ScaleObjectVFX));

		[SerializeField]
		private float _scaleDuration = 1f;

		[SerializeField]
		private Vector3 _startingValue = Vector3.zero;

		[SerializeField]
		private Vector3 _finalValue = Vector3.one;

		[NonSerialized]
		private ScaleVectorOverTime _scaleVector;
	}
}
