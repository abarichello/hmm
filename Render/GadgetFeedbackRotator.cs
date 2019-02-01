using System;
using UnityEngine;

namespace HeavyMetalMachines.Render
{
	internal class GadgetFeedbackRotator : BaseGadgetFeedback
	{
		protected override void UpdateImpl()
		{
			if (this.gadgetState == null || this.targetGameObject == null)
			{
				return;
			}
			if (this.gadgetState.GadgetState == this.activateState)
			{
				this.currentAngle += this.angularSpeed * Time.deltaTime;
				this.targetGameObject.transform.localEulerAngles = this.currentAngle;
			}
		}

		protected override void OnActivate()
		{
			base.OnActivate();
			this.currentAngle = this.targetGameObject.transform.localEulerAngles;
		}

		public GameObject targetGameObject;

		public Vector3 angularSpeed = Vector3.zero;

		private Vector3 currentAngle;
	}
}
