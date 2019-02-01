using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkScaleMassOnGadgetHeat : BasePerk
	{
		public override void PerkInitialized()
		{
			base.PerkInitialized();
			this._thisCombat = base.GetComponent<CombatObject>();
			this._updateMass = (this._thisCombat != null && GameHubBehaviour.Hub.Net.IsServer());
			this.UpdateMass();
		}

		private void FixedUpdate()
		{
			if (!this._everyFrame)
			{
				return;
			}
			this.UpdateMass();
		}

		private void UpdateMass()
		{
			if (!this._updateMass)
			{
				return;
			}
			this._thisCombat.Movement.Info.Mass = Mathf.Lerp(this._minMass, this._maxMass, this.Effect.Gadget.CurrentHeat);
		}

		[SerializeField]
		private float _minMass;

		[SerializeField]
		private float _maxMass;

		[SerializeField]
		private bool _everyFrame;

		private CombatObject _thisCombat;

		private bool _updateMass;
	}
}
