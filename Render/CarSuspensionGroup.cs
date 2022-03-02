using System;
using HeavyMetalMachines.Car;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Infra.Context;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Render
{
	public class CarSuspensionGroup : GameHubBehaviour
	{
		public void Initialize()
		{
			if (GameHubBehaviour.Hub != null && GameHubBehaviour.Hub.Net.IsServer() && !GameHubBehaviour.Hub.Net.IsTest())
			{
				return;
			}
			Vector3 vector = this.frontLeftWheel.Target + this.frontRightWheel.Target + this.backLeftWheel.Target + this.backRightWheel.Target;
			vector /= 4f;
			this.originalOffset = this.carBody.transform.localPosition - this.carBody.transform.InverseTransformPoint(vector);
		}

		public CarMovement CarMovement
		{
			set
			{
				if (GameHubBehaviour.Hub != null && GameHubBehaviour.Hub.Net.IsServer() && !GameHubBehaviour.Hub.Net.IsTest())
				{
					return;
				}
				this.carMovement = value;
				CombatObject combat = this.carMovement.Combat;
				if (combat == null)
				{
					return;
				}
				this.gadgetState = combat.Combat.GadgetStates.GetGadgetState(GadgetSlot.BoostGadget);
				this.gadgetState.ListenToGadgetActivation += this.OnBoost;
			}
		}

		private void EnsureWheelsPosition()
		{
			if (GameHubBehaviour.Hub != null && GameHubBehaviour.Hub.Net.IsServer() && !GameHubBehaviour.Hub.Net.IsTest())
			{
				return;
			}
			if (this.backSensorGroundJump > 0f)
			{
				this.backSensorGroundJump -= Time.deltaTime;
				if (this.backSensorGroundJump < 0f)
				{
					Vector3 force;
					force..ctor(0f, 5f, 0f);
					this.backRightWheel.AddWheelForce(force);
					this.backLeftWheel.AddWheelForce(force);
				}
			}
			if (this.frontSensorGroundJump > 0f)
			{
				this.frontSensorGroundJump -= Time.deltaTime;
				if (this.frontSensorGroundJump < 0f)
				{
					Vector3 force2;
					force2..ctor(0f, 5f, 0f);
					this.frontRightWheel.AddWheelForce(force2);
					this.frontLeftWheel.AddWheelForce(force2);
				}
			}
			float wheelRadius = this.frontLeftWheel.wheelRadius;
			float num = this.frontLeftWheel.transform.localPosition.y - this.frontRightWheel.transform.localPosition.y;
			float num2 = (this.frontLeftWheel.transform.localPosition.y + this.frontRightWheel.transform.localPosition.y) * 0.5f;
			if (Mathf.Abs(num) > wheelRadius)
			{
				num = Mathf.Clamp(num, -wheelRadius, wheelRadius);
				Vector3 localPosition = this.frontLeftWheel.transform.localPosition;
				localPosition.y = num2 + num * 0.5f;
				this.frontLeftWheel.transform.localPosition = localPosition;
				localPosition = this.frontRightWheel.transform.localPosition;
				localPosition.y = num2 - num * 0.5f;
				this.frontRightWheel.transform.localPosition = localPosition;
				SuspensionConstraint.EqualizeWheelsSpeed(this.frontLeftWheel, this.frontRightWheel);
			}
			wheelRadius = this.backLeftWheel.wheelRadius;
			float num3 = this.backLeftWheel.transform.localPosition.y - this.backRightWheel.transform.localPosition.y;
			float num4 = (this.backLeftWheel.transform.localPosition.y + this.backRightWheel.transform.localPosition.y) * 0.5f;
			if (Mathf.Abs(num3) > wheelRadius)
			{
				num3 = Mathf.Clamp(num3, -wheelRadius, wheelRadius);
				Vector3 localPosition2 = this.backLeftWheel.transform.localPosition;
				localPosition2.y = num4 + num3 * 0.5f;
				this.backLeftWheel.transform.localPosition = localPosition2;
				localPosition2 = this.backRightWheel.transform.localPosition;
				localPosition2.y = num4 - num3 * 0.5f;
				this.backRightWheel.transform.localPosition = localPosition2;
				SuspensionConstraint.EqualizeWheelsSpeed(this.backLeftWheel, this.backRightWheel);
			}
			float num5 = num4 - num2;
			if (Mathf.Abs(num4 - num2) > wheelRadius)
			{
				float num6 = (num5 - Mathf.Clamp(num5, -wheelRadius, wheelRadius)) * 0.5f;
				Vector3 localPosition3 = this.backLeftWheel.transform.localPosition;
				localPosition3.y -= num6;
				this.backLeftWheel.transform.localPosition = localPosition3;
				localPosition3 = this.backRightWheel.transform.localPosition;
				localPosition3.y -= num6;
				this.backRightWheel.transform.localPosition = localPosition3;
				localPosition3 = this.frontLeftWheel.transform.localPosition;
				localPosition3.y += num6;
				this.frontLeftWheel.transform.localPosition = localPosition3;
				localPosition3 = this.frontRightWheel.transform.localPosition;
				localPosition3.y += num6;
				this.frontRightWheel.transform.localPosition = localPosition3;
				SuspensionConstraint.EqualizeWheelsSpeed(this.backLeftWheel, this.backRightWheel, this.frontLeftWheel, this.frontRightWheel);
			}
		}

		private void OnBoost()
		{
			if (GameHubBehaviour.Hub != null && GameHubBehaviour.Hub.Net.IsServer() && !GameHubBehaviour.Hub.Net.IsTest())
			{
				return;
			}
			this.frontLeftWheel.AddWheelForce(new Vector3(0f, 10f, 0f));
			this.frontRightWheel.AddWheelForce(new Vector3(0f, 10f, 0f));
		}

		private void LateUpdate()
		{
			this.ProcessSuspension();
		}

		private void Awake()
		{
			if (GameHubBehaviour.Hub)
			{
				GameHubBehaviour.Hub.BombManager.ListenToPhaseChange += this.OnPhaseChanged;
			}
		}

		private void OnPhaseChanged(BombScoreboardState obj)
		{
			if (obj == BombScoreboardState.Replay || obj == BombScoreboardState.Shop)
			{
				this.freezePhysics = false;
				this.freezeBodyPhysics = false;
				this.carBody.gameObject.SetActive(false);
				this.carBody.gameObject.SetActive(true);
			}
		}

		private void OnEnable()
		{
			if (GameHubBehaviour.Hub != null && GameHubBehaviour.Hub.Net.IsServer() && !GameHubBehaviour.Hub.Net.IsTest())
			{
				base.enabled = false;
				return;
			}
			this.frontLeftWheel.suspensionGroup = this;
			this.frontRightWheel.suspensionGroup = this;
			this.backLeftWheel.suspensionGroup = this;
			this.backRightWheel.suspensionGroup = this;
			this.m_stPreviousForward = base.transform.forward;
			this.originalPosition = this.carBody.transform.localPosition;
			this.ProcessSuspension();
		}

		private void ProcessSuspension()
		{
			if (!this._updateSuspension)
			{
				return;
			}
			if (GameHubBehaviour.Hub != null && GameHubBehaviour.Hub.Net.IsServer() && !GameHubBehaviour.Hub.Net.IsTest())
			{
				return;
			}
			if (this.freezePhysics)
			{
				return;
			}
			if (this.carBody == null)
			{
				CarSuspensionGroup.Log.ErrorFormat("[ProcessSuspension] carBody is null - {0}", new object[]
				{
					base.transform.parent.name
				});
				return;
			}
			Vector3 up = this.carBody.transform.parent.up;
			this.backLeftWheel.orientation = up;
			this.backRightWheel.orientation = up;
			this.frontLeftWheel.orientation = up;
			this.frontRightWheel.orientation = up;
			this.EnsureWheelsPosition();
			Vector3 zero = Vector3.zero;
			this.leftVector = (this.backLeftWheel.Target - this.frontLeftWheel.Target).normalized;
			this.frontVector = (this.frontRightWheel.Target - this.frontLeftWheel.Target).normalized;
			this.rightVector = (this.frontRightWheel.Target - this.backRightWheel.Target).normalized;
			this.backVector = (this.backLeftWheel.Target - this.backRightWheel.Target).normalized;
			Vector3 normalized = ((this.frontRightWheel.Target + this.frontLeftWheel.Target) * 0.5f - (this.backRightWheel.Target + this.backLeftWheel.Target) * 0.5f).normalized;
			this.normal1 = Vector3.Cross(this.frontVector, this.leftVector);
			this.normal2 = Vector3.Cross(this.backVector, this.rightVector);
			Vector3 vector = normalized + this.currentShake;
			Vector3 vector2 = (this.normal1 + this.normal2).normalized;
			if (vector.sqrMagnitude < 0.05f)
			{
				vector = this.m_stPreviousForward;
			}
			else
			{
				this.m_stPreviousForward = vector;
			}
			if (vector2.sqrMagnitude < 0.1f)
			{
				vector2 = this.m_stPreviousUp;
			}
			else
			{
				this.m_stPreviousUp = vector2;
			}
			if (!this.freezeBodyPhysics)
			{
				this.carBody.transform.rotation = Quaternion.LookRotation(vector, vector2);
			}
			this.backLeftWheel.AddForce(Vector3.up * (zero.x - zero.z));
			this.backRightWheel.AddForce(Vector3.up * (zero.x + zero.z));
			this.frontLeftWheel.AddForce(Vector3.up * (-zero.x - zero.z));
			this.frontRightWheel.AddForce(Vector3.up * (-zero.x + zero.z));
			Vector3 vector3 = this.frontLeftWheel.Target + this.frontRightWheel.Target + this.backLeftWheel.Target + this.backRightWheel.Target;
			vector3 /= 4f;
			if (!this.freezeBodyPhysics)
			{
				this.carBody.transform.position = vector3 + this.carBody.transform.parent.TransformDirection(this.originalOffset + this.offset);
			}
		}

		private void OnDestroy()
		{
			if (GameHubBehaviour.Hub == null || GameHubBehaviour.Hub.Net.IsServer())
			{
				return;
			}
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange -= this.OnPhaseChanged;
			if (this.gadgetState != null)
			{
				this.gadgetState.ListenToGadgetActivation -= this.OnBoost;
			}
		}

		private void OnDrawGizmos()
		{
			if (GameHubBehaviour.Hub != null && GameHubBehaviour.Hub.Net.IsServer() && !GameHubBehaviour.Hub.Net.IsTest())
			{
				return;
			}
			if (!this.backRightWheel || !this.frontLeftWheel)
			{
				return;
			}
			Gizmos.DrawRay(this.frontLeftWheel.Target, this.leftVector);
			Gizmos.DrawRay(this.frontLeftWheel.Target, this.frontVector);
			Gizmos.DrawRay(this.backRightWheel.Target, this.rightVector);
			Gizmos.DrawRay(this.backRightWheel.Target, this.backVector);
			Gizmos.color = Color.green;
			Gizmos.DrawRay(this.frontLeftWheel.Target, this.normal1);
			Gizmos.DrawRay(this.backRightWheel.Target, this.normal2);
		}

		public void DoCarHit(Vector3 direction, float force, bool ignoreHit = false)
		{
			if (GameHubBehaviour.Hub != null && GameHubBehaviour.Hub.Net.IsServer() && !GameHubBehaviour.Hub.Net.IsTest())
			{
				return;
			}
			if (!base.enabled)
			{
				return;
			}
			if (!base.gameObject.activeInHierarchy)
			{
				return;
			}
			if (direction == Vector3.zero)
			{
				return;
			}
			Vector3 vector;
			vector..ctor(direction.x, 0f, direction.z);
			direction = vector.normalized;
			if (!ignoreHit && this.nextAllowedHit > Time.time)
			{
				return;
			}
			if (this.carBody == null)
			{
				CarSuspensionGroup.Log.ErrorFormat("[DoCarHit] carBody is null - {0}", new object[]
				{
					base.transform.parent.name
				});
				return;
			}
			if (!ignoreHit)
			{
				this.nextAllowedHit = Time.time + 0.2f;
			}
			Vector3 vector2 = this.carBody.transform.InverseTransformDirection(Vector3.Cross(direction, Vector3.up)) * Vector3.Dot(direction * force, direction) * 0.4f;
			this.backLeftWheel.AddForce(Vector3.up * (vector2.x - vector2.z));
			this.backRightWheel.AddForce(Vector3.up * (vector2.x + vector2.z));
			this.frontLeftWheel.AddForce(Vector3.up * (-vector2.x - vector2.z));
			this.frontRightWheel.AddForce(Vector3.up * (-vector2.x + vector2.z));
			this.backLeftWheel.AddWheelForce(Vector3.up * (vector2.x - vector2.z));
			this.backRightWheel.AddWheelForce(Vector3.up * (vector2.x + vector2.z));
			this.frontLeftWheel.AddWheelForce(Vector3.up * (-vector2.x - vector2.z));
			this.frontRightWheel.AddWheelForce(Vector3.up * (-vector2.x + vector2.z));
		}

		public void Reset()
		{
			this.backLeftWheel.Reset();
			this.backRightWheel.Reset();
			this.frontLeftWheel.Reset();
			this.frontRightWheel.Reset();
			this.m_stPreviousUp = Vector3.up;
			this.m_stPreviousForward = base.transform.forward;
			this.carBody.transform.localRotation = Quaternion.identity;
			this.carBody.transform.localPosition = this.originalPosition;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(CarSuspensionGroup));

		public SuspensionConstraint frontLeftWheel;

		public SuspensionConstraint frontRightWheel;

		public SuspensionConstraint backLeftWheel;

		public SuspensionConstraint backRightWheel;

		public GameObject carBody;

		private Vector3 normal1;

		private Vector3 normal2;

		private Vector3 leftVector;

		private Vector3 frontVector;

		private Vector3 rightVector;

		private Vector3 backVector;

		public bool freezePhysics;

		public bool freezeBodyPhysics;

		public const float lifeBumpLimit = 0.15f;

		public Vector3 offset;

		private Vector3 originalOffset;

		public Vector3 originalPosition;

		[SerializeField]
		private bool _updateSuspension = true;

		private CarMovement carMovement;

		public float Shaking = 0.1f;

		private float lastVAxis;

		private float lastHAxis;

		private Vector3 currentShake = Vector3.zero;

		private Vector3 currentShakeVel;

		private float frontSensorGroundJump;

		private float backSensorGroundJump;

		private Vector3 m_stPreviousForward = Vector3.zero;

		private Vector3 m_stPreviousUp = Vector3.up;

		private float nextAllowedHit;

		private GadgetData.GadgetStateObject gadgetState;

		private const float allowedHitFrequency = 0.2f;
	}
}
