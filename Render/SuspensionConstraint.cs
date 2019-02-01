using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Render
{
	public class SuspensionConstraint : GameHubBehaviour
	{
		public Vector3 Target
		{
			get
			{
				return this.target;
			}
		}

		public Vector3 Position
		{
			get
			{
				return base.transform.position;
			}
		}

		public void Initialize()
		{
			this.initialized = true;
			if (this.source == null)
			{
				this.source = base.transform;
			}
			this.target = this.source.position + this.orientation * this.springSize;
			if (base.GetComponent<Renderer>() == null)
			{
				return;
			}
			this.wheelRadius = base.GetComponent<Renderer>().bounds.size.y * 0.5f;
		}

		private void Start()
		{
			this.skidMark = base.GetComponent<SkidMarkEmitter>();
		}

		private void OnEnable()
		{
			if (GameHubBehaviour.Hub.Net.IsServer() && !GameHubBehaviour.Hub.Net.IsTest())
			{
				base.enabled = false;
				return;
			}
			if (!this.initialized)
			{
				this.Initialize();
				return;
			}
			this.target = this.source.position + this.orientation * this.springSize;
		}

		private void OnDrawGizmos()
		{
			Gizmos.DrawWireSphere(base.transform.position, this.wheelRadius);
		}

		public void SetSuspensionInfo(SuspensionInfo suspensionInfo)
		{
			this.springSize = suspensionInfo.suspensionSize;
			this.minCompression = suspensionInfo.minSuspension;
			this.maxStretch = suspensionInfo.maxSuspension;
		}

		private void LateUpdate()
		{
			if (this.suspensionGroup && this.suspensionGroup.freezePhysics)
			{
				return;
			}
			this.UpdateWheelPhysics();
			Vector3 vector = this.target - this.source.position;
			vector = this.orientation * Vector3.Dot(vector, this.orientation);
			float num = vector.magnitude + this.targetVelocity * Time.deltaTime;
			Vector3 a = vector / num;
			if (num > this.maxStretch)
			{
				this.targetVelocity -= num - this.maxStretch;
				vector = a * this.maxStretch;
				this.target = this.source.position + vector;
				if (this.targetVelocity > 0f)
				{
					this.targetVelocity = 0f;
				}
				this.targetVelocity += num - this.maxStretch;
				num = this.maxStretch;
			}
			if (Vector3.Dot(vector, this.orientation) < 0f)
			{
				vector = -a * this.minCompression;
				this.target = this.source.position + vector;
				num = this.minCompression;
			}
			if (num <= this.minCompression)
			{
				vector = a * this.minCompression;
				this.target = this.source.position + vector;
				if (this.targetVelocity < 0f)
				{
					this.targetVelocity = -this.targetVelocity;
				}
				num = this.minCompression;
				if (this.OnCompressHit != null)
				{
					this.OnCompressHit();
				}
			}
			this.target = this.source.position + this.orientation * num;
		}

		private void UpdateWheelPhysics()
		{
			Vector3 localPosition = base.transform.localPosition;
			MapProjectedFakeHeight.PointData pointData = MapProjectedFakeHeight.GetPointData(base.transform.position);
			localPosition.y = pointData.height * this.wheelRadius * 0.5f + this.wheelRadius + this.minHeight;
			if (this.skidMark)
			{
				this.skidMark.GroundType = pointData.groundType;
			}
			if (base.transform.localPosition.y < localPosition.y)
			{
				this.wheelVelocity.y = this.wheelVelocity.y + (localPosition.y - base.transform.localPosition.y) * this.SpeedFactor;
			}
			else
			{
				this.wheelVelocity.y = this.wheelVelocity.y - this.gravity * Time.deltaTime;
			}
			base.transform.localPosition += this.wheelVelocity * Time.deltaTime;
			if (base.transform.localPosition.y < localPosition.y && this.wheelVelocity.y < 0f)
			{
				base.transform.localPosition = localPosition;
				this.wheelVelocity.y = -this.wheelVelocity.y * this.wheelBounciness;
			}
			if (base.transform.position.y >= this.maxHeight && this.wheelVelocity.y > 0f)
			{
				this.wheelVelocity.y = 0f;
			}
		}

		public void AddForce(Vector3 force)
		{
			this.targetVelocity += force.y / this.Weight;
		}

		public void AddWheelForce(Vector3 force)
		{
			this.wheelVelocity += force / this.Weight * this.Factor;
		}

		public void Reset()
		{
			this.wheelVelocity = Vector3.zero;
			this.targetVelocity = 0f;
			Vector3 localPosition = this.source.localPosition;
			localPosition.y = this.minHeight + this.wheelRadius;
			this.source.localPosition = localPosition;
			this.target = this.source.position + this.orientation * this.springSize;
		}

		public static void EqualizeWheelsSpeed(SuspensionConstraint suspensionA, SuspensionConstraint suspensionB)
		{
			Vector3 vector = (suspensionA.wheelVelocity + suspensionB.wheelVelocity) * 0.5f;
			suspensionA.wheelVelocity = (suspensionB.wheelVelocity = vector);
		}

		public static void EqualizeWheelsSpeed(SuspensionConstraint suspensionA, SuspensionConstraint suspensionB, SuspensionConstraint suspensionC, SuspensionConstraint suspensionD)
		{
			Vector3 vector = (suspensionA.wheelVelocity + suspensionB.wheelVelocity + suspensionC.wheelVelocity + suspensionD.wheelVelocity) * 0.25f;
			suspensionA.wheelVelocity = (suspensionB.wheelVelocity = (suspensionC.wheelVelocity = (suspensionD.wheelVelocity = vector)));
		}

		public static readonly BitLogger Log = new BitLogger(typeof(SuspensionConstraint));

		private Transform source;

		private Vector3 target;

		public float Weight = 1f;

		public float springForce = 50f;

		public float gravity = 25f;

		private float targetVelocity;

		private Vector3 constraint;

		public float springSize = 1f;

		public float minCompression = 0.5f;

		public float maxStretch = 2f;

		public SkidMarkEmitter skidMark;

		public Action OnCompressHit;

		public Vector3 lastSourcePosition;

		public Vector3 wheelVelocity;

		public Vector3 orientation = Vector3.up;

		public float wheelRadius;

		public float wheelBounciness = 0.8f;

		public float bounceness = 5f;

		public float minHeight;

		public float maxHeight = 3f;

		public float Factor = 1f;

		public float SpeedFactor = 40f;

		public CarSuspensionGroup suspensionGroup;

		private bool initialized;
	}
}
