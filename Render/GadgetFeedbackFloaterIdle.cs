using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Render
{
	internal class GadgetFeedbackFloaterIdle : GameHubBehaviour
	{
		private void Start()
		{
			if (GameHubBehaviour.Hub.Net.IsServer() && !GameHubBehaviour.Hub.Net.IsTest())
			{
				return;
			}
			this.initialPosition = this.targetGameObject.transform.localPosition;
		}

		public void Update()
		{
			if (GameHubBehaviour.Hub.Net.IsServer() && !GameHubBehaviour.Hub.Net.IsTest())
			{
				return;
			}
			if (this.targetGameObject != null && this.targetGameObject.activeSelf)
			{
				this.currentAngle.x = Mathf.Cos(Time.timeSinceLevelLoad * this.xAngleVelocity) * this.xAngleRange;
				this.currentAngle.z = Mathf.Sin(Time.timeSinceLevelLoad * this.zAngleVelocity) * this.zAngleRange;
				this.currentY = Mathf.Cos(Time.timeSinceLevelLoad * this.yVelocity) * this.yDistanceRange;
				this.targetGameObject.transform.localPosition = this.initialPosition + new Vector3(0f, this.currentY, 0f);
				this.targetGameObject.transform.localEulerAngles = this.currentAngle;
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
	}
}
