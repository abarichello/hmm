using System;
using HeavyMetalMachines.Combat;
using UnityEngine;

namespace HeavyMetalMachines.Render
{
	internal class GadgetFeedbackAnimation : BaseGadgetFeedback
	{
		private new void Start()
		{
			base.Start();
			if (!string.IsNullOrEmpty(this.OnGadgetReadyBoolean))
			{
				this.OnGadgetReadyBooleanId = Animator.StringToHash(this.OnGadgetReadyBoolean);
			}
			if (!string.IsNullOrEmpty(this.OnGadgetUsedTrigger))
			{
				this.OnGadgetUsedTriggerId = Animator.StringToHash(this.OnGadgetUsedTrigger);
			}
		}

		protected override void OnEnable()
		{
			this.OnGadgetStateChanged();
		}

		protected override void UpdateImpl()
		{
			if (this.gadgetState == null)
			{
				return;
			}
			if (this.gadgetState.GadgetState == this.previousState)
			{
				return;
			}
			this.OnGadgetStateChanged();
		}

		private void OnGadgetStateChanged()
		{
			if (this.gadgetState == null)
			{
				return;
			}
			if (this.gadgetState.GadgetState == GadgetState.Ready && this.OnGadgetReadyBooleanId > 0)
			{
				this.animator.SetBool(this.OnGadgetReadyBooleanId, true);
			}
			if (this.gadgetState.GadgetState == GadgetState.Cooldown)
			{
				if (this.OnGadgetReadyBooleanId > 0)
				{
					this.animator.SetBool(this.OnGadgetReadyBooleanId, false);
				}
				if (this.OnGadgetUsedTriggerId > 0)
				{
					this.animator.SetTrigger(this.OnGadgetUsedTriggerId);
				}
			}
			this.previousState = this.gadgetState.GadgetState;
		}

		public Animator animator;

		public string OnGadgetReadyBoolean = string.Empty;

		private int OnGadgetReadyBooleanId = -1;

		public string OnGadgetUsedTrigger = string.Empty;

		private int OnGadgetUsedTriggerId = -1;

		public string OnGadgetNitroBoolean = string.Empty;
	}
}
