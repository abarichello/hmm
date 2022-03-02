using System;
using System.Collections.Generic;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class GadgetActivatorVFXCtrl : GameHubBehaviour
	{
		private void Awake()
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				base.enabled = false;
			}
			this.activeInstanceIdList = new List<int>();
			this.isActive = false;
		}

		public void Activate(int instanceID)
		{
			if (!this.activeInstanceIdList.Contains(instanceID))
			{
				this.activeInstanceIdList.Add(instanceID);
			}
			this.CheckActivation();
		}

		public void Deactivate(int instanceID)
		{
			if (this.activeInstanceIdList.Contains(instanceID))
			{
				this.activeInstanceIdList.Remove(instanceID);
			}
			this.CheckActivation();
		}

		private void CheckActivation()
		{
			if (this.isActive && this.activeInstanceIdList.Count == 0)
			{
				this.isActive = false;
				this.SetAnimator(this.isActive);
			}
			else if (!this.isActive && this.activeInstanceIdList.Count > 0)
			{
				this.isActive = true;
				this.SetAnimator(this.isActive);
			}
		}

		private void SetAnimator(bool value)
		{
			if (!this.animator)
			{
				return;
			}
			this.animator.SetBool(this.triggerBolName, value);
		}

		private bool isActive;

		[SerializeField]
		private Animator animator;

		[SerializeField]
		private string triggerBolName;

		[SerializeField]
		private List<int> activeInstanceIdList;
	}
}
