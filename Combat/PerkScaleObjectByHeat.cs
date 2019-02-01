using System;
using HeavyMetalMachines.Utils.Scale;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkScaleObjectByHeat : BasePerk
	{
		protected override void Awake()
		{
			base.Awake();
			base.enabled = GameHubBehaviour.Hub.Net.IsServer();
			this._scaleVector = new ScaleVectorByFactor(this.StartingValue, this.FinalValue);
		}

		public override void PerkInitialized()
		{
			base.transform.localScale = this.StartingValue;
			this._scaleVector.SetValuesAndReset(this.StartingValue, this.FinalValue);
			base.transform.localScale = this._scaleVector.Update(this.Effect.Gadget.CurrentHeat);
		}

		private void FixedUpdate()
		{
			if (!this.ScaleOnUpdate)
			{
				return;
			}
			base.transform.localScale = this._scaleVector.Update(this.Effect.Gadget.CurrentHeat);
		}

		public Vector3 StartingValue = Vector3.zero;

		public Vector3 FinalValue = Vector3.one;

		public bool ScaleOnUpdate = true;

		private ScaleVectorByFactor _scaleVector;
	}
}
