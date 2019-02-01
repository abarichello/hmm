using System;
using UnityEngine;

namespace HeavyMetalMachines.Scene
{
	[Serializable]
	public class Activation
	{
		public void Activate(bool enable, int causer)
		{
			if (this.TargetObject != null)
			{
				this.TargetObject.SetActive(enable);
			}
			if (this.TargetBehaviour != null)
			{
				IActivatable activatable = this.TargetBehaviour as IActivatable;
				if (activatable != null)
				{
					activatable.Activate(enable, causer);
				}
				else
				{
					this.TargetBehaviour.enabled = enable;
				}
			}
		}

		public GameObject TargetObject;

		public MonoBehaviour TargetBehaviour;

		public ActionType Action;

		public bool ServerOnly;
	}
}
