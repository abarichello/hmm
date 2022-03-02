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
			if (this.scaleByEP)
			{
				float num = this.combatObject.Data.EP / (float)this.combatObject.Data.EPMax;
				this.percentageDebuger = num;
				this.targetGameObject.transform.localScale = Vector3.LerpUnclamped(this.minEPLocalScale, this.maxEPLocalScale, num);
			}
			if (this.rotateByEP)
			{
				float num2 = this.combatObject.Data.EP / (float)this.combatObject.Data.EPMax;
				this.percentageDebuger = num2;
				this.targetGameObject.transform.localEulerAngles = Vector3.LerpUnclamped(this.minEPLocalRotation, this.maxEPLocalRotation, num2);
			}
		}

		protected override void OnActivate()
		{
			base.OnActivate();
			this.currentAngle = this.targetGameObject.transform.localEulerAngles;
		}

		public GameObject targetGameObject;

		public Vector3 angularSpeed = Vector3.zero;

		[SerializeField]
		private Vector3 currentAngle;

		[Tooltip("Rotate according to EP")]
		[SerializeField]
		private bool rotateByEP;

		[SerializeField]
		private Vector3 minEPLocalRotation;

		[SerializeField]
		private Vector3 maxEPLocalRotation;

		[SerializeField]
		private float percentageDebuger;

		[SerializeField]
		private bool scaleByEP;

		[SerializeField]
		private Vector3 minEPLocalScale = Vector3.one;

		[SerializeField]
		private Vector3 maxEPLocalScale = Vector3.one;
	}
}
