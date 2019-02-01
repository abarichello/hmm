using System;
using HeavyMetalMachines.Combat;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Car
{
	public class CarInput : GameHubBehaviour, IObjectSpawnListener
	{
		public Vector3 MousePosition
		{
			get
			{
				return this.LastInputs.MousePos;
			}
		}

		private void Awake()
		{
			this._trans = base.transform;
			this._movement = base.GetComponent<CarMovement>();
			this.SetAngle(this.DirectionalAngle);
		}

		public CarInput.DrivingStyleKind CurrentDrivingStyle
		{
			get
			{
				return this.LastInputs.DrivingStyle;
			}
		}

		public bool InverseReverse
		{
			get
			{
				return this.LastInputs.InverseReverse;
			}
		}

		public bool IsReverse
		{
			get
			{
				switch (this.CurrentDrivingStyle)
				{
				case CarInput.DrivingStyleKind.Simulator:
				case CarInput.DrivingStyleKind.FollowMouse:
					return this.LastInputs.Down && !this.LastInputs.Up;
				case CarInput.DrivingStyleKind.Controller:
					return this.LastInputs.ReverseGear;
				default:
					return false;
				}
			}
		}

		public bool IsAccelerating
		{
			get
			{
				return this.LastInputs.Up;
			}
		}

		public float MaxSafeInputAngle
		{
			get
			{
				return this._movement.MaxSafeAngularSpeed / this._movement.MaxAngularSpeed * this.ControllerAngleForMaxAngleSpeed;
			}
		}

		public void BotInput(float hor, float ver, bool drift)
		{
			if (this.DebugStopUpdating)
			{
				return;
			}
			this.LastInputs.DrivingStyle = CarInput.DrivingStyleKind.Bot;
			this.LastInputs.Dir = new Vector2(hor, ver);
			this.UpdateInput();
		}

		public void Input(PlayerController.InputMap inputs, bool immobilized)
		{
			if (this.DebugStopUpdating)
			{
				return;
			}
			if (immobilized)
			{
				this.LastInputs.Clear();
				this.TargetH = 0f;
				this.TargetV = 0f;
				return;
			}
			this.LastInputs.Copy(inputs);
			this.UpdateInput();
		}

		public void SetAngle(float angle)
		{
			this.DirectionalAngle = angle;
			this._angleRad = angle * 0.0174532924f;
			this._angleCos = Mathf.Cos(this._angleRad);
			this._angleSin = Mathf.Sin(this._angleRad);
		}

		public Vector2 InverseConvert(Vector2 worldAim)
		{
			Vector2 result = new Vector2(worldAim.x * this._angleCos + worldAim.y * this._angleSin, worldAim.y * this._angleCos - worldAim.x * this._angleSin);
			return result;
		}

		public Vector2 ConvertAim(Vector2 screenAim)
		{
			Vector2 result = new Vector2(screenAim.x * this._angleCos - screenAim.y * this._angleSin, screenAim.x * this._angleSin + screenAim.y * this._angleCos);
			return result;
		}

		public void ForceDrivingStyle(CarInput.DrivingStyleKind drivingStyleKind)
		{
			this.LastInputs.DrivingStyle = drivingStyleKind;
		}

		private void UpdateInput()
		{
			switch (this.LastInputs.DrivingStyle)
			{
			case CarInput.DrivingStyleKind.Simulator:
			{
				this.TargetH = this.LastInputs.Dir.x;
				this.TargetV = this.LastInputs.Dir.y;
				bool flag = !Mathf.Approximately(this.TargetH, 0f);
				if (flag)
				{
					this._keyboardPressedTime += Time.deltaTime;
					this._keyboardReleasedTime = 0f;
				}
				else
				{
					this._keyboardPressedTime = 0f;
					this._keyboardReleasedTime += Time.deltaTime;
				}
				if ((flag && this._keyboardPressedTime < this.KeyboardMaxAngSpeedTime) || (!flag && this._keyboardReleasedTime < this.KeyboardAngRecoveryTime))
				{
					float num = (!flag) ? Mathf.Sign(this._movement.LastAngularVelocity) : Mathf.Sign(this.TargetH);
					this.TargetH = this._movement.MaxSafeAngularSpeed / this._movement.MaxAngularSpeed * num * Mathf.Sign(this.TargetV);
				}
				break;
			}
			case CarInput.DrivingStyleKind.Controller:
			case CarInput.DrivingStyleKind.FollowMouse:
				this.FixControllerAxis(this.LastInputs.DrivingStyle);
				break;
			case CarInput.DrivingStyleKind.Bot:
				this.TargetH = this.LastInputs.Dir.x;
				this.TargetV = this.LastInputs.Dir.y;
				break;
			}
		}

		public Vector2 FixDirection(Vector2 input)
		{
			if (input.sqrMagnitude < 0.0004f)
			{
				return Vector2.zero;
			}
			Vector2 normalized = input.normalized;
			Vector2 zero = Vector2.zero;
			zero.x = normalized.x * this._angleCos - normalized.y * this._angleSin;
			zero.y = normalized.x * this._angleSin + normalized.y * this._angleCos;
			return zero;
		}

		private void FixControllerAxis(CarInput.DrivingStyleKind style)
		{
			if (style == CarInput.DrivingStyleKind.Controller)
			{
				this.TargetV = Mathf.Abs(this.LastInputs.Speed) * (float)((!this.LastInputs.ReverseGear) ? 1 : -1);
				if (this.LastInputs.ReverseGear)
				{
					this._lastReverse = Time.time;
				}
				else if (this.TargetV > 0f)
				{
					this._lastReverse = 0f;
				}
			}
			else
			{
				this.TargetV = ((!this.LastInputs.Up) ? ((float)((!this.LastInputs.Down) ? 0 : -1)) : (1f - ((!this.LastInputs.Down) ? 0f : this.MouseReverseSpeedReduction)));
				if (this.LastInputs.Down && !this.LastInputs.Up)
				{
					this._lastReverse = Time.time;
				}
				else if (this.LastInputs.Up)
				{
					this._lastReverse = 0f;
				}
			}
			if (this.LastInputs.Dir == Vector2.zero)
			{
				this.TargetH = 0f;
				this.TargetY = 0f;
				return;
			}
			bool flag = this.TargetV < 0f || (this._lastReverse > 0f && 1f > Time.time - this._lastReverse);
			float num = (!flag) ? this._trans.rotation.eulerAngles.y : (this._trans.rotation.eulerAngles.y - 180f);
			if (num > 180f)
			{
				num -= 360f;
			}
			else if (num < -180f)
			{
				num += 360f;
			}
			float num2 = Vector2.Angle(Vector2.up, this.LastInputs.Dir) * Mathf.Sign(this.LastInputs.Dir.x);
			this.TargetY = num2 - num;
			if (this.TargetY > 180f)
			{
				this.TargetY -= 360f;
			}
			else if (this.TargetY < -180f)
			{
				this.TargetY += 360f;
			}
			this.TargetH = Mathf.Min(Mathf.Abs(this.TargetY) / this.ControllerAngleForMaxAngleSpeed, 1f) * Mathf.Sign(this.TargetY);
			if (this._movement.CarInfo.GirosFlingHelper)
			{
				if ((this.TargetV != 0f || flag) && Mathf.Abs(this.TargetY) > 90f)
				{
					this.TargetV = (Mathf.Abs(this.TargetY) - 90f) / 90f * (float)((!flag) ? -1 : 1);
					if (this._movement.LastAngularVelocity != 0f)
					{
						this.TargetH = Mathf.Abs(this.TargetH) * Mathf.Sign(this._movement.LastAngularVelocity);
					}
				}
			}
			else if (!Mathf.Approximately(0f, this._movement.LastAngularVelocity) && Mathf.Abs(this.TargetY) > 90f)
			{
				this.TargetH = Mathf.Abs(this.TargetH) * Mathf.Sign(this._movement.LastAngularVelocity);
			}
		}

		public void OnObjectSpawned(SpawnEvent msg)
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			this.TargetV = 0f;
			this.TargetH = 0f;
			base.enabled = true;
		}

		public void OnObjectUnspawned(UnspawnEvent msg)
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			base.enabled = false;
			this.TargetV = 0f;
			this.TargetH = 0f;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(CarInput));

		public bool DebugStopUpdating;

		public float ControllerAngleForMaxAngleSpeed;

		public float ControllerMaxAccelerationAngle;

		public float KeyboardMaxAngSpeedTime;

		public float KeyboardAngRecoveryTime;

		public float MouseReverseSpeedReduction;

		public float TurretAngle;

		public float TargetH;

		public float TargetV;

		public float TargetY;

		public float DirectionalAngle = -45f;

		public CarIndicator.IndicatorRayConfig RayConfig;

		private float _angleRad;

		private float _angleCos;

		private float _angleSin;

		private Transform _trans;

		private CarMovement _movement;

		private float _keyboardPressedTime;

		private float _keyboardReleasedTime;

		private PlayerController.InputMap LastInputs = new PlayerController.InputMap();

		private const float ReverseLingerTime = 1f;

		private float _lastReverse;

		public enum DrivingStyleKind
		{
			Simulator,
			Controller,
			FollowMouse,
			Bot
		}
	}
}
