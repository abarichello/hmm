using System;
using UnityEngine;

namespace HeavyMetalMachines.Render
{
	internal class GadgetFeedbackPendulum : BaseGadgetFeedback
	{
		private new void Start()
		{
			base.Start();
			this.currentStatePosition = new Vector3(0f, -this.ropeLength, this.ropeLength / 2f);
			this.lastCarPosition = this.targetGameObject.transform.parent.position;
		}

		private Vector3 PendulumUpdate(Vector3 currentStatePosition, float dt)
		{
			this.gravityForce = this.mass * 9.8f;
			this.gravityDirection = this.forceDirection.normalized;
			this.currentVelocity += this.gravityDirection * this.gravityForce * dt;
			Vector3 vector = this.currentStatePosition;
			Vector3 b = this.currentVelocity * dt;
			float num = Vector3.Magnitude(vector + b);
			if (num > this.ropeLength || Mathf.Approximately(num, this.ropeLength))
			{
				this.tensionDirection = (-vector).normalized;
				this.pendulumSideDirection = Quaternion.Euler(0f, 90f, 0f) * this.tensionDirection;
				this.pendulumSideDirection.Scale(new Vector3(1f, 0f, 1f));
				this.pendulumSideDirection.Normalize();
				float num2 = Vector3.Angle(vector, this.gravityDirection);
				this.tensionForce = this.mass * this.gravityDirection.magnitude * Mathf.Cos(0.0174532924f * num2);
				float num3 = this.mass * Mathf.Pow(this.currentVelocity.magnitude, 2f) / this.ropeLength;
				this.tensionForce += num3 * this.swingDampening;
				this.currentVelocity += this.tensionDirection * this.tensionForce * dt;
			}
			Vector3 b2 = this.currentVelocity * dt;
			float num4 = Vector3.Magnitude(currentStatePosition + b2);
			return (currentStatePosition + b2).normalized * ((num4 > this.ropeLength) ? this.ropeLength : num4);
		}

		protected override void UpdateImpl()
		{
			if (this.targetGameObject == null)
			{
				return;
			}
			Vector3 position = this.targetGameObject.transform.parent.position;
			Vector3 a = (position - this.lastCarPosition) / Time.deltaTime;
			Vector3 a2 = this.forceMultiplier * (a - this.lastCarVelocity) / Time.deltaTime;
			this.lastCarPosition = position;
			this.lastCarVelocity = a;
			if (a2.magnitude > 1f)
			{
				a2.Normalize();
			}
			Vector3 vector = -a2 - Vector3.up;
			vector = this.targetGameObject.transform.parent.InverseTransformDirection(vector);
			this.forceDirection = Vector3.Lerp(this.forceDirection, vector, 0.2f);
			if (this.gadgetState == null || this.gadgetState.GadgetState == this.activateState)
			{
				float num = Time.deltaTime;
				float num2 = 0.01f;
				while (num >= num2)
				{
					this.previousStatePosition = this.currentStatePosition;
					this.currentStatePosition = this.PendulumUpdate(this.currentStatePosition, num2);
					num -= num2;
				}
				float num3 = num / num2;
				Vector3 forward = this.currentStatePosition * num3 + this.previousStatePosition * (1f - num3);
				forward.Normalize();
				this.targetGameObject.transform.localRotation = Quaternion.LookRotation(forward, Vector3.right);
			}
		}

		protected override void OnActivate()
		{
			base.OnActivate();
			if (this.targetGameObject)
			{
				this.targetGameObject.SetActive(true);
			}
		}

		protected override void OnDeactivate()
		{
			base.OnDeactivate();
			if (this.targetGameObject)
			{
				this.targetGameObject.SetActive(false);
			}
		}

		public GameObject targetGameObject;

		public float mass = 1f;

		public float ropeLength = 1f;

		public float swingDampening = 0.95f;

		private Vector3 currentAngle;

		private Vector3 currentVelocity;

		private Vector3 currentStatePosition;

		private Vector3 previousStatePosition;

		private Vector3 lastCarPosition;

		private Vector3 lastCarVelocity;

		public float forceMultiplier = 0.1f;

		public Vector3 forceDirection = new Vector3(0f, -1f, 0f);

		private Vector3 gravityDirection;

		private Vector3 tensionDirection;

		private Vector3 pendulumSideDirection;

		private float gravityForce;

		private float tensionForce;
	}
}
