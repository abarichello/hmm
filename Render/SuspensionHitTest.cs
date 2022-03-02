using System;
using UnityEngine;

namespace HeavyMetalMachines.Render
{
	internal class SuspensionHitTest : MonoBehaviour
	{
		private void Awake()
		{
			this.rb = base.gameObject.AddComponent<Rigidbody>();
			this.rb.constraints = 84;
			this.rb.angularDrag = 10f;
			this.cc = base.gameObject.AddComponent<CapsuleCollider>();
			this.cc.direction = 2;
			this.cc.radius = 2f;
			this.cc.height = 7f;
			this.cf = base.gameObject.AddComponent<ConstantForce>();
		}

		private void DoHit()
		{
			Vector3 vector = this.target.carBody.transform.InverseTransformDirection(Vector3.Cross(this.lastDir, Vector3.up)) * Vector3.Dot(this.lastDir * this.force, this.lastDir) * 0.4f;
			this.target.backLeftWheel.AddForce(Vector3.up * (vector.x - vector.z));
			this.target.backRightWheel.AddForce(Vector3.up * (vector.x + vector.z));
			this.target.frontLeftWheel.AddForce(Vector3.up * (-vector.x - vector.z));
			this.target.frontRightWheel.AddForce(Vector3.up * (-vector.x + vector.z));
			this.target.backLeftWheel.AddWheelForce(Vector3.up * Mathf.Max(0f, vector.x - vector.z));
			this.target.backRightWheel.AddWheelForce(Vector3.up * Mathf.Max(0f, vector.x + vector.z));
			this.target.frontLeftWheel.AddWheelForce(Vector3.up * Mathf.Max(0f, -vector.x - vector.z));
			this.target.frontRightWheel.AddWheelForce(Vector3.up * Mathf.Max(0f, -vector.x + vector.z));
		}

		private void Update()
		{
			if (this.boost)
			{
				this.target.frontLeftWheel.AddWheelForce(new Vector3(0f, 10f, 0f));
				this.target.frontRightWheel.AddWheelForce(new Vector3(0f, 10f, 0f));
				this.boost = false;
			}
			if (this.timer > 0f)
			{
				this.timer -= Time.deltaTime;
				return;
			}
			if (this.driveMode)
			{
				this.timer = 5f;
				this.cf.relativeForce = new Vector3(0f, 0f, (float)((!this.dir) ? (-(float)Random.Range(10, 40)) : Random.Range(10, 40)));
				this.cf.torque = new Vector3(0f, (float)((Random.Range(0, 10) <= 5) ? (-(float)Random.Range(2, 60)) : Random.Range(2, 60)), 0f);
				this.dir = !this.dir;
				return;
			}
			this.timer = 2f;
			this.lastDir = Random.onUnitSphere;
			this.lastDir.y = 0f;
			this.lastDir.Normalize();
			this.DoHit();
		}

		private void OnCollisionEnter(Collision col)
		{
			if (!this.driveMode)
			{
				return;
			}
			if (Time.time - this.lastWallHit < 0.3f)
			{
				return;
			}
			this.lastWallHit = Time.time;
			this.lastDir = col.contacts[0].normal;
			this.DoHit();
		}

		private void OnDrawGizmos()
		{
			Gizmos.DrawSphere(base.transform.position + this.lastDir * 3f, 0.5f);
		}

		public CarSuspensionGroup target;

		public float timer;

		public float force = 1f;

		private Vector3 lastDir;

		private Rigidbody rb;

		private CapsuleCollider cc;

		private ConstantForce cf;

		public bool driveMode;

		public bool boost;

		private bool dir;

		private float lastWallHit;
	}
}
