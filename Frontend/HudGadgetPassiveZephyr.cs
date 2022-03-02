using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Utils;

namespace HeavyMetalMachines.Frontend
{
	public class HudGadgetPassiveZephyr : HudGadgetObject
	{
		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (this._gadgetData != null)
			{
				GadgetData.GadgetStateObject gadgetState = this._gadgetData.GetGadgetState(GadgetSlot.PassiveGadget);
				gadgetState.ListenToGadgetReady -= this.GadgetStateObjectOnGadgetReady;
			}
			this._gadgetData = null;
		}

		protected override bool Setup(CombatObject combatObject)
		{
			if (!base.Setup(combatObject))
			{
				return false;
			}
			if (combatObject.Player.GetCharacter() == CharacterTarget.Zephyr)
			{
				return false;
			}
			this._buffOnSpeed = (combatObject.PassiveGadget as BuffOnSpeed);
			if (this._buffOnSpeed == null)
			{
				Debug.Assert(false, "Expected BuffOnSpeed as Zephyr PassiveGadget", Debug.TargetTeam.All);
				return false;
			}
			this._gadgetData = combatObject.Data.gameObject.GetComponent<GadgetData>();
			GadgetData.GadgetStateObject gadgetState = this._gadgetData.GetGadgetState(GadgetSlot.PassiveGadget);
			gadgetState.ListenToGadgetReady += this.GadgetStateObjectOnGadgetReady;
			this._isReady = (gadgetState.GadgetState == GadgetState.Ready);
			if (this._isReady)
			{
				this.GadgetStateObjectOnGadgetReady();
			}
			this.ProgressBar.value = 0f;
			return true;
		}

		private void GadgetStateObjectOnGadgetReady()
		{
		}

		protected override void RenderUpdate()
		{
			if (this._gadgetData == null)
			{
				return;
			}
			float maxValue = this._gadgetData.GadgetJokeBarState.MaxValue;
			this.ProgressBar.value = ((Math.Abs(maxValue) >= 0.001f) ? (this._gadgetData.GadgetJokeBarState.Value / maxValue) : 0f);
		}

		public UIProgressBar ProgressBar;

		private BuffOnSpeed _buffOnSpeed;

		private GadgetData _gadgetData;

		private bool _isReady;
	}
}
