using System;
using HeavyMetalMachines.Characters;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Car
{
	public class CarMovement : CombatMovement
	{
		public CarInfo CarInfo
		{
			get
			{
				return this.Char.Car;
			}
		}

		public override MovementInfo Info
		{
			get
			{
				return this.Char.Car;
			}
		}

		public float MaxLinearSpeed
		{
			get
			{
				if (Mathf.Approximately(this._maxSpeed, 0f))
				{
					this._maxSpeed = this.GetMaxSpeed();
				}
				return this._maxSpeed;
			}
		}

		public float MaxSafeAngularSpeed
		{
			get
			{
				return this.MaxLinearSpeed / ((this.CarInfo.TurningRadius + this.Combat.Attributes.TurningRadius) * (1f + this.Combat.Attributes.TurningRadiusPct)) * 57.29578f;
			}
		}

		public Vector3 LastAcceleration
		{
			get
			{
				return this._lastAccel * this._trans.forward;
			}
		}

		public bool IsDrifting
		{
			get
			{
				return (!GameHubBehaviour.Hub.Net.IsServer()) ? this._isDrifting : (this._lastSpeed > 0f && (this.Combat.Attributes.CurrentStatus.HasFlag(StatusKind.Oiled) || this._driftDirection < this.CarInfo.DriftDifferenceForRecovery * this._lastSpeed));
			}
			set
			{
				this._isDrifting = value;
			}
		}

		protected override void Awake()
		{
			base.Awake();
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			this._trans = base.transform;
			if (!this._carInput)
			{
				this._carInput = base.GetComponent<CarInput>();
			}
		}

		protected override void Start()
		{
			base.Start();
			if (GameHubBehaviour.Hub.MatchMan.ArenaColliders != null && GameHubBehaviour.Hub.MatchMan.ArenaColliders.Length > 0)
			{
				this._shouldValidatePosition = true;
				this._updater = new TimedUpdater(1000, true, false);
			}
		}

		private void OnDestroy()
		{
			this.Char = null;
		}

		private void GetInput()
		{
			if (GameHubBehaviour.Hub.Global.LockAllPlayers || this.Combat.Attributes.CurrentStatus.HasFlag(StatusKind.Immobilized))
			{
				this.VAxis = (this.HAxis = 0f);
			}
			else
			{
				this.HAxis = this._carInput.TargetH;
				this.VAxis = this._carInput.TargetV;
			}
		}

		private bool DoesTurnCausesDrift(float angVel)
		{
			float num = this._lastSpeed * this._lastSpeed / ((this.CarInfo.TurningRadius + this.Combat.Attributes.TurningRadius) * (1f + this.Combat.Attributes.TurningRadiusPct));
			float num2 = this._lastSpeed * Mathf.Abs(angVel);
			return num2 > num;
		}

		private float SetAngularVelocity()
		{
			float num = 0f;
			if (this._carInput.IsThrottling)
			{
				num = this.HAxis * this._body.maxAngularVelocity;
			}
			if (Mathf.Approximately(this.Combat.Attributes.ForcedAngularPush, 0f))
			{
				this._lastAngularVelocity.y = num;
				this._body.angularVelocity = this._lastAngularVelocity;
			}
			else
			{
				Vector3 vector = this._body.angularVelocity + Vector3.up * (this.Combat.Attributes.ForcedAngularPush * 0.017453292f);
				this._body.angularVelocity = vector;
				this._lastAngularVelocity = vector;
			}
			return num;
		}

		protected override void MovementFixedUpdate()
		{
			base.MovementFixedUpdate();
			this._lastSpeedFraction = ((this.MaxLinearSpeed <= 0f) ? 0f : (this._lastSpeed / this.MaxLinearSpeed));
			if (this._shouldValidatePosition && !this._updater.ShouldHalt() && !MatchController.IsValidPoint(this._trans.position, new float[]
			{
				this.Info.DepthOfMeshValidators[0] + (float)this.Combat.Team
			}))
			{
				this._trans.position = base.GetClosestValidPosition(this._trans.position, true);
			}
			this.GetInput();
			float angVel = this.SetAngularVelocity();
			float num = Vector3.Dot(this._trans.forward, this._body.velocity);
			float num2 = Vector3.Dot(this._trans.right, this._body.velocity);
			this._driftDirection = Vector3.Dot(this._body.velocity, this._trans.forward * Mathf.Sign(this.VAxis));
			bool flag = (!Mathf.Approximately(this.VAxis, 0f) && Mathf.Sign(this.VAxis) != Mathf.Sign(num)) || this.IsDrifting || this.DoesTurnCausesDrift(angVel);
			if (!flag)
			{
				this._body.AddRelativeForce(num2 * -1f * Vector3.right, 2);
				this._body.AddRelativeForce((this._lastSpeed - Mathf.Abs(num)) * Mathf.Sign(this.VAxis) * Vector3.forward, 2);
				num2 = 0f;
				num = this._lastSpeed * Mathf.Sign(this.VAxis);
			}
			float num3;
			if (this.VAxis > 0f)
			{
				num3 = this.VAxis * Mathf.Max(0f, (this.CarInfo.ForwardAcceleration + this.Combat.Attributes.AccelerationMod) * (1f + this.Combat.Attributes.AccelerationModPct));
			}
			else
			{
				num3 = this.VAxis * Mathf.Max(0f, (this.CarInfo.BackwardAcceleration + this.Combat.Attributes.BackAccelMod) * (1f + this.Combat.Attributes.BackAccelModPct));
			}
			if (!flag)
			{
				num3 += Mathf.Max(0f, ((this.VAxis <= 0f) ? this.Combat.Attributes.GripExtraBackAccelMod : this.Combat.Attributes.GripExtraFwdAccelMod) * (float)Math.Sign(this.VAxis)) * Mathf.Max(0f, 1f + ((this.VAxis <= 0f) ? this.Combat.Attributes.GripExtraBackAccelModPct : this.Combat.Attributes.GripExtraFwdAccelModPct));
			}
			this._lastAccel = num3;
			if (!Mathf.Approximately(num3, 0f))
			{
				this._body.AddRelativeForce(num3 * Vector3.forward, 5);
			}
			else
			{
				float num4 = Mathf.Max(0f, (this.CarInfo.BrakeAcceleration + this.Combat.Attributes.BrakeAccelMod) * (1f + this.Combat.Attributes.BrakeAccelModPct));
				if (this._lastSpeed < num4 * Time.deltaTime)
				{
					this._body.velocity = Vector3.zero;
					num2 = 0f;
				}
				else
				{
					this._body.AddRelativeForce(Mathf.Sign(num) * num4 * -1f * Vector3.forward, 5);
				}
			}
			float num5 = Mathf.Max(0f, Math.Max(0f, (this.CarInfo.LateralFriction + this.Combat.Attributes.LateralFriction) * (1f + this.Combat.Attributes.LateralFrictionPct)));
			if (Mathf.Abs(num2) < num5 * Time.deltaTime)
			{
				this._body.AddRelativeForce(num2 * -1f * Vector3.right, 2);
			}
			else
			{
				this._body.AddRelativeForce(Mathf.Sign(num2) * num5 * -1f * Vector3.right, 5);
			}
		}

		protected override void ApplyDrag(AnimationCurve dragCurve, float dragMod, float dragModPct)
		{
			if (this.IsDrifting)
			{
				base.ApplyDrag(this.CarInfo.DriftDrag, this.Combat.Attributes.DriftDragMod, this.Combat.Attributes.DriftDragModPct);
			}
			else
			{
				base.ApplyDrag(dragCurve, dragMod, dragModPct);
			}
		}

		public void Boost(float amount, bool pct)
		{
			if (!base.enabled || !base.CanMove)
			{
				return;
			}
			float num;
			if (pct)
			{
				num = base.LastVelocity.magnitude * amount;
			}
			else
			{
				num = amount;
			}
			this._body.AddRelativeForce(num * Vector3.forward, 2);
		}

		public override void OnObjectUnspawned(UnspawnEvent msg)
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			base.OnObjectUnspawned(msg);
			this.HAxis = 0f;
			this.VAxis = 0f;
		}

		public override void ResetImpulseAndVelocity()
		{
			base.ResetImpulseAndVelocity();
			this._driftDirection = 0f;
			this.HAxis = 0f;
			this.VAxis = 0f;
		}

		private float GetMaxSpeed()
		{
			if (Mathf.Approximately(this.CarInfo.Drag[this.CarInfo.Drag.length - 1].value, this.CarInfo.ForwardAcceleration))
			{
				return this.CarInfo.Drag[this.CarInfo.Drag.length - 1].time;
			}
			float num = 0f;
			float num2 = 0f;
			while (num < this.CarInfo.ForwardAcceleration)
			{
				num = CombatMovement.GetDrag(num2, this.CarInfo.Drag, this.Combat.Attributes.DragMod, this.Combat.Attributes.DragModPct);
				num2 += 1f;
			}
			return num2;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(CarMovement));

		public CharacterInfo Char;

		public float HAxis;

		public float VAxis;

		private const int VALIDATION_TIME = 1000;

		private bool _shouldValidatePosition;

		private TimedUpdater _updater;

		private float _maxSpeed;

		private float _driftDirection;

		private bool _isDrifting;

		private float _lastSpeedFraction;

		private CarInput _carInput;

		private float _lastAccel;
	}
}
