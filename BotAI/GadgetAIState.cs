using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Infra.Context;
using UnityEngine;

namespace HeavyMetalMachines.BotAI
{
	[Serializable]
	public class GadgetAIState
	{
		public GadgetAIState(IIsGadgetDisarmed isGadgetDisarmed)
		{
			this._isGadgetDisarmed = isGadgetDisarmed;
		}

		public void ForceStop()
		{
			this.ReactionTimeRemainingSeconds = 0f;
			this.ShouldUse = false;
			this.FlipAfterSwitch = false;
			this.Using = false;
		}

		public void Press(bool value)
		{
			if (this.ReactionTimeRemainingSeconds > 0f)
			{
				this.FlipAfterSwitch = (value != this.ShouldUse);
				return;
			}
			if (this.ShouldUse == value)
			{
				return;
			}
			this.ReactionTimeRemainingSeconds = this.UseInfo.ReactionTimeMin + Random.Range(0f, 1f) * (this.UseInfo.ReactionTimeMax - this.UseInfo.ReactionTimeMin);
			this.ShouldUse = value;
			this.FlipAfterSwitch = false;
		}

		public virtual bool Update(float dt, bool gadgetUsedThisFrame, ICombatObject combat)
		{
			if (gadgetUsedThisFrame)
			{
				this.ForceStop();
				return false;
			}
			if (this._isGadgetDisarmed.Check(combat))
			{
				return false;
			}
			if (this.ReactionTimeRemainingSeconds > 0f)
			{
				this.ReactionTimeRemainingSeconds -= dt;
			}
			if (this.ReactionTimeRemainingSeconds <= 0f)
			{
				bool shouldUse = this.ShouldUse;
				this.Using = shouldUse;
				if (this.FlipAfterSwitch)
				{
					this.FlipAfterSwitch = false;
					this.Press(!shouldUse);
				}
				return shouldUse;
			}
			return this.Using;
		}

		public BotAIGoal.GadgetUseInfo UseInfo;

		public GadgetBehaviour Gadget;

		public GadgetData.GadgetStateObject GadgetState;

		public bool ShouldUse;

		public bool Using;

		public float ReactionTimeRemainingSeconds;

		public bool FlipAfterSwitch;

		protected IIsGadgetDisarmed _isGadgetDisarmed;
	}
}
