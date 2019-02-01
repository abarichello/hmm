using System;
using HeavyMetalMachines.Utils.Scale;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkScaleObjectOverTime : BasePerk
	{
		protected override void Awake()
		{
			base.Awake();
			base.enabled = GameHubBehaviour.Hub.Net.IsServer();
			this._scaleVector = new ScaleVectorOverTime(this.StartingValue, this.FinalValue, this.ScaleDuration);
		}

		public override void PerkInitialized()
		{
			base.transform.localScale = this.StartingValue;
			this._scaleVector.SetValuesAndReset(this.StartingValue, this.FinalValue, this.ScaleDuration);
		}

		private void FixedUpdate()
		{
			if (this._scaleVector.Finished)
			{
				return;
			}
			base.transform.localScale = this._scaleVector.Update();
		}

		public float ScaleDuration = 1f;

		public Vector3 StartingValue = Vector3.zero;

		public Vector3 FinalValue = Vector3.one;

		private ScaleVectorOverTime _scaleVector;
	}
}
