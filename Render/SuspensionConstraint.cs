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
				return this.cachedTransform.position;
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
			this.cachedTransform = base.transform;
		}

		private void OnEnable()
		{
			if (GameHubBehaviour.Hub && GameHubBehaviour.Hub.Net.IsServer() && !GameHubBehaviour.Hub.Net.IsTest())
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
			if (this.source == null)
			{
				this.Initialize();
			}
			Vector3 vector = this.target - this.source.position;
			vector = this.orientation * Vector3.Dot(vector, this.orientation);
			float num = vector.magnitude + this.targetVelocity * Time.deltaTime;
			Vector3 vector2 = vector / num;
			if (num > this.maxStretch)
			{
				this.targetVelocity -= num - this.maxStretch;
				vector = vector2 * this.maxStretch;
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
				vector = -vector2 * this.minCompression;
				this.target = this.source.position + vector;
				num = this.minCompression;
			}
			if (num <= this.minCompression)
			{
				vector = vector2 * this.minCompression;
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
			Vector3 localPosition = this.cachedTransform.localPosition;
			Vector3 vector = localPosition;
			localPosition.y = this.wheelRadius + this.minHeight;
			if (vector.y < localPosition.y)
			{
				this.wheelVelocity.y = this.wheelVelocity.y + (localPosition.y - vector.y) * this.SpeedFactor;
			}
			else
			{
				this.wheelVelocity.y = this.wheelVelocity.y - this.gravity * Time.deltaTime;
			}
			vector += this.wheelVelocity * Time.deltaTime;
			if (vector.y < localPosition.y && this.wheelVelocity.y < 0f)
			{
				vector = localPosition;
				this.wheelVelocity.y = -this.wheelVelocity.y * this.wheelBounciness;
			}
			if (vector.y >= this.maxHeight && this.wheelVelocity.y > 0f)
			{
				this.wheelVelocity.y = 0f;
				vector..ctor(vector.x, this.maxHeight, vector.z);
			}
			this.cachedTransform.localPosition = vector;
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

		private Transform cachedTransform;

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
