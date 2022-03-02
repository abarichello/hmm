using System;
using HeavyMetalMachines.Infra.Context;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Car
{
	public class TurretMovement : GameHubBehaviour, ITurretMovement
	{
		public float TurretAngle
		{
			get
			{
				return this._turretAngleNormalized;
			}
			set
			{
				this._turretAngleNormalized = value;
				this._turretDirection = Quaternion.AngleAxis(this._turretAngleNormalized * 57.29578f, Vector3.up) * Vector3.forward;
			}
		}

		public Vector3 TurretDirection
		{
			get
			{
				return this._turretDirection;
			}
		}

		public Vector3 GlobalTurretDirection
		{
			get
			{
				return base.transform.TransformDirection(this._turretDirection);
			}
		}

		public IIdentifiable Identifiable
		{
			get
			{
				return base.Id;
			}
		}

		private void Awake()
		{
			if (GameHubBehaviour.Hub.Net.IsClient() && !GameHubBehaviour.Hub.Net.IsTest())
			{
				base.enabled = false;
				return;
			}
			this._carInput = base.GetComponent<CarInput>();
		}

		private void OnEnable()
		{
			this.ResetRotation();
		}

		public void ResetRotation()
		{
			this._turretDirection = Vector3.forward;
			this._turretAngleNormalized = 0f;
			this._mouseDirection = base.transform.forward;
		}

		private void Update()
		{
			if (this._carInput.MouseDirection != Vector3.zero)
			{
				this._mouseDirection = this._carInput.MouseDirection;
			}
			Vector3 vector = base.transform.InverseTransformDirection(this._mouseDirection);
			float num = Vector3.SignedAngle(this._turretDirection, vector, Vector3.up) * 0.017453292f / 3.1415927f;
			float num2 = this.TurretConfiguration.MaxTurretSpeed * Time.deltaTime;
			float num3 = Mathf.Clamp(num, -num2, num2);
			this._turretAngleNormalized += num3;
			if (this._turretAngleNormalized > 1f)
			{
				this._turretAngleNormalized -= 2f;
			}
			else if (this._turretAngleNormalized < -1f)
			{
				this._turretAngleNormalized += 2f;
			}
			this._turretDirection = Quaternion.AngleAxis(this._turretAngleNormalized * 3.1415927f * 57.29578f, Vector3.up) * Vector3.forward;
			Debug.DrawLine(base.transform.position, base.transform.position + this.GlobalTurretDirection * 10f, Color.red);
			Debug.DrawLine(base.transform.position, base.transform.position + this._carInput.MouseDirection * 10f, Color.blue);
		}

		public TurretMovement.Configuration TurretConfiguration;

		private Vector3 _turretDirection;

		private float _turretAngleNormalized;

		private CarInput _carInput;

		private Vector3 _mouseDirection;

		[Serializable]
		public class Configuration
		{
			public float MaxTurretSpeed;
		}
	}
}
