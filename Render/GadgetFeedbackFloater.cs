using System;
using HeavyMetalMachines.VFX;
using UnityEngine;

namespace HeavyMetalMachines.Render
{
	internal class GadgetFeedbackFloater : BaseGadgetFeedback
	{
		private new void Start()
		{
			base.Start();
			this.initialPosition = this.targetGameObject.transform.localPosition;
		}

		protected override void UpdateImpl()
		{
			if (this.targetGameObject != null && this.targetGameObject.activeSelf)
			{
				this.currentAngle.x = Mathf.Cos(Time.timeSinceLevelLoad * this.xAngleVelocity) * this.xAngleRange;
				this.currentAngle.z = Mathf.Sin(Time.timeSinceLevelLoad * this.zAngleVelocity) * this.zAngleRange;
				this.currentY = Mathf.Cos(Time.timeSinceLevelLoad * this.yVelocity) * this.yDistanceRange;
				this.targetGameObject.transform.localPosition = this.initialPosition + new Vector3(0f, this.currentY, 0f);
				this.targetGameObject.transform.localEulerAngles = this.currentAngle;
			}
		}

		protected override void OnActivate()
		{
			base.OnActivate();
			if (this.hideUnhideTargetGameObject)
			{
				this.targetGameObject.SetActive(true);
			}
			if (!this.baseVFX)
			{
				return;
			}
			if (this.activateDeactivateBaseVFX)
			{
				this.baseVFX.Activate(this.fakeTargetInfo);
			}
		}

		protected override void OnDeactivate()
		{
			base.OnDeactivate();
			if (this.hideUnhideTargetGameObject)
			{
				this.targetGameObject.SetActive(false);
			}
			if (!this.baseVFX)
			{
				return;
			}
			if (this.activateDeactivateBaseVFX)
			{
				this.baseVFX.SignalDeactivation();
			}
		}

		public GameObject targetGameObject;

		public float xAngleRange = 10f;

		public float zAngleRange = 10f;

		public float xAngleVelocity = 1f;

		public float zAngleVelocity = 1f;

		public float yDistanceRange = 0.1f;

		public float yVelocity = 1f;

		private Vector3 currentAngle;

		private Vector3 initialPosition;

		private float currentY;

		[SerializeField]
		private bool hideUnhideTargetGameObject = true;

		[SerializeField]
		private bool activateDeactivateBaseVFX;

		[SerializeField]
		private BaseVFX baseVFX;

		protected MasterVFX.TargetFXInfo fakeTargetInfo;
	}
}
