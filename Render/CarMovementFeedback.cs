using System;
using HeavyMetalMachines.Audio;
using HeavyMetalMachines.Car;
using HeavyMetalMachines.Combat;
using NewParticleSystem;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Render
{
	public class CarMovementFeedback : GameHubBehaviour, IObjectSpawnListener
	{
		public GroundType GroundType
		{
			get
			{
				return this.groundType;
			}
			set
			{
				this.groundType = value;
			}
		}

		public void ExternalToggle(bool enable)
		{
			this._enableParticles = enable;
			if (!enable)
			{
				this.StopAccelerationPS();
				this.StopBrakePS();
				this.StopDriftPS();
			}
		}

		private void OnEnable()
		{
			if (this.lowHPGO)
			{
				this.lowHPGO.SetActive(false);
				this.isLowHP = false;
			}
			this._trans = base.transform;
			if (GameHubBehaviour.Hub != null && GameHubBehaviour.Hub.Net.IsServer() && !GameHubBehaviour.Hub.Net.IsTest())
			{
				UnityEngine.Object.Destroy(this);
				return;
			}
			this.minDeltaAcceleration = 1.8f;
			this.minAccelerationTime = 0.3f;
			this.minDeltaBrake = 2.8f;
			this.minBrakeTime = 0.3f;
			this.minDriftCosAngle = 16f;
			this.minDriftTime = 0.3f;
			this.minSkidMarkTime = 0.3f;
			this.skidMarkEmitters = this._trans.GetComponentsInChildren<SkidMarkEmitter>();
			Vector3 zero = Vector3.zero;
			this._suspension = base.GetComponentInChildren<CarSuspensionGroup>();
			if (this._suspension != null)
			{
				zero.z = this._suspension.backLeftWheel.transform.localPosition.z;
			}
			if (this.prefabAccelerationPS && !this.accelerateParticleSystem)
			{
				this.accelerateParticleSystem = UnityEngine.Object.Instantiate<HoplonParticleSystem>(this.prefabAccelerationPS, this._trans);
				this.accelerateParticleSystem.transform.parent = this._trans;
				this.accelerateParticleSystem.transform.localPosition = zero;
				this.accelerateParticleSystem.transform.localRotation = Quaternion.identity;
			}
			if (this.prefabBrakePS && !this.brakeParticleSystem)
			{
				this.brakeParticleSystem = UnityEngine.Object.Instantiate<HoplonParticleSystem>(this.prefabBrakePS, this._trans);
				this.brakeParticleSystem.transform.parent = this._trans;
				this.brakeParticleSystem.transform.localPosition = zero;
				this.brakeParticleSystem.transform.localRotation = Quaternion.identity;
			}
			if (this.prefabDriftPS && !this.driftParticleSystem)
			{
				this.driftParticleSystem = UnityEngine.Object.Instantiate<HoplonParticleSystem>(this.prefabDriftPS, this._trans);
				this.driftParticleSystem.transform.parent = this._trans;
				this.driftParticleSystem.transform.localPosition = zero;
				this.driftParticleSystem.transform.localRotation = Quaternion.identity;
			}
		}

		private void Update()
		{
			if (this._carMovement == null)
			{
				return;
			}
			float num = (!(this._carMovement == null)) ? this._carMovement.SpeedZ : 0f;
			float num2 = num - this.lastZSpeed;
			if (this.lowHPGO)
			{
				if ((this._carMovement && this._carMovement.Combat.Data.HP / (float)this._carMovement.Combat.Data.HPMax < 0.15f) || this.forceLowHP)
				{
					if (!this.isLowHP)
					{
						this.lowHPGO.SetActive(true);
						this.isLowHP = true;
					}
				}
				else if (this.isLowHP)
				{
					this.lowHPGO.SetActive(false);
					this.isLowHP = false;
				}
			}
			this.accelPSactiveTime -= Time.deltaTime;
			if (num2 > this.minDeltaAcceleration)
			{
				this.PlayAccelerationPS();
				this.StartSkidMark();
			}
			if (this.accelPSactiveTime <= 0f)
			{
				this.StopAccelerationPS();
			}
			this.brakePSactiveTime -= Time.deltaTime;
			if (num2 < -this.minDeltaBrake)
			{
				this.PlayBrakePS();
				this.StartSkidMark();
			}
			if (this.brakePSactiveTime <= 0f)
			{
				this.StopBrakePS();
			}
			float num3 = Mathf.Clamp(this._carMovement.Combat.CarInput.TargetY / this._carMovement.Combat.CarInput.MaxSafeInputAngle, -1f, 1f);
			this.driftPSactiveTime -= Time.deltaTime;
			bool flag = this._carMovement.IsDrifting;
			if ((num != 0f || this._carMovement.LastVelocity.sqrMagnitude > 0f) && this._carMovement.Combat.CarInput.TargetH != 0f)
			{
				flag |= (Mathf.Abs(num3) >= 1f);
			}
			if (flag)
			{
				this.PlayDriftPS();
				this.StartSkidMark();
			}
			if (this.driftPSactiveTime <= 0f)
			{
				this.StopDriftPS();
			}
			this.skidMarkActiveTime -= Time.deltaTime;
			if (this.skidMarkActiveTime <= 0f)
			{
				this.StopSkidMark();
			}
			this._suspension.DoCarHit(this._trans.forward, num2 * (((double)num3 <= 0.1) ? this.BurnoutStrength : 0f), true);
			if (this._carMovement.LastVelocity.sqrMagnitude > 0f)
			{
				this._suspension.DoCarHit(-this._trans.right, Mathf.Sign(Vector3.Dot(this._trans.right, this._carMovement.LastVelocity)) * ((!flag) ? Mathf.Abs(num3) : 1f) * Time.deltaTime * this.SteeringSuspensionIntensity, true);
			}
			this.lastZSpeed = num;
		}

		private void PlayDriftPS()
		{
			this.driftPSactiveTime = this.minDriftTime;
			if (!this.driftPSactive && this.driftParticleSystem && this._enableParticles)
			{
				this.driftParticleSystem.Play();
				this.driftPSactive = true;
			}
		}

		private void StopDriftPS()
		{
			if (this.driftParticleSystem)
			{
				this.driftParticleSystem.Stop();
				this.driftPSactive = false;
			}
			this.driftPSactiveTime = 0f;
		}

		private void PlayAccelerationPS()
		{
			this.accelPSactiveTime = this.minAccelerationTime;
			if (!this.accelPSactive && this.accelerateParticleSystem && this._enableParticles)
			{
				this.accelerateParticleSystem.Play();
				this.accelPSactive = true;
			}
		}

		private void StopAccelerationPS()
		{
			if (this.accelerateParticleSystem)
			{
				this.accelerateParticleSystem.Stop();
				this.accelPSactive = false;
			}
			this.accelPSactiveTime = 0f;
		}

		private void PlayBrakePS()
		{
			this.brakePSactiveTime = this.minBrakeTime;
			if (!this.brakePSactive && this.brakeParticleSystem && this._enableParticles)
			{
				this.brakeParticleSystem.Play();
				this.brakePSactive = true;
			}
		}

		private void StopBrakePS()
		{
			if (this.brakeParticleSystem)
			{
				this.brakeParticleSystem.Stop();
				this.brakePSactive = false;
			}
			this.brakePSactiveTime = 0f;
		}

		private void StartSkidMark()
		{
			if (Math.Abs(base.transform.position.y) < 0.001f)
			{
				if (this._carAudioController)
				{
					this._carAudioController.PlayDriftAudio();
				}
				this.skidMarkActiveTime = this.minSkidMarkTime;
				if (!this.skidMarkActive && this.skidMarkEmitters != null)
				{
					for (int i = 0; i < this.skidMarkEmitters.Length; i++)
					{
						this.skidMarkEmitters[i].doEmission = true;
					}
				}
				this.skidMarkActive = true;
			}
		}

		private void StopSkidMark()
		{
			if (this.skidMarkActive)
			{
				if (this._carAudioController)
				{
					this._carAudioController.StopDriftAudio();
				}
				if (this.skidMarkEmitters != null)
				{
					for (int i = 0; i < this.skidMarkEmitters.Length; i++)
					{
						this.skidMarkEmitters[i].doEmission = false;
					}
					this.skidMarkActive = false;
				}
				this.skidMarkActiveTime = 0f;
			}
		}

		public void OnObjectUnspawned(UnspawnEvent evt)
		{
			this.StopAccelerationPS();
			this.StopBrakePS();
			this.StopDriftPS();
			this.StopSkidMark();
			this.lastZSpeed = 0f;
		}

		public void OnObjectSpawned(SpawnEvent evt)
		{
		}

		public static readonly BitLogger Log = new BitLogger(typeof(CarMovementFeedback));

		public HoplonParticleSystem prefabAccelerationPS;

		public HoplonParticleSystem prefabBrakePS;

		public HoplonParticleSystem prefabDriftPS;

		private HoplonParticleSystem accelerateParticleSystem;

		private HoplonParticleSystem brakeParticleSystem;

		private HoplonParticleSystem driftParticleSystem;

		public GameObject lowHPGO;

		private bool isLowHP;

		public float minDeltaAcceleration = 1.8f;

		public float minAccelerationTime = 0.3f;

		public float minDeltaBrake = 2.8f;

		public float minBrakeTime = 0.3f;

		public float minDriftCosAngle = 16f;

		public float minDriftTime = 0.3f;

		public float minSkidMarkTime = 0.3f;

		public float SteeringSuspensionIntensity = 100f;

		public float BurnoutStrength = 1f;

		[NonSerialized]
		public CarMovement _carMovement;

		[NonSerialized]
		public CarAudioController _carAudioController;

		private CarSuspensionGroup _suspension;

		private bool accelPSactive;

		private float accelPSactiveTime;

		private bool brakePSactive;

		private float brakePSactiveTime;

		private bool driftPSactive;

		private float driftPSactiveTime;

		private bool skidMarkActive;

		private float skidMarkActiveTime;

		private float lastZSpeed;

		private Transform _trans;

		private SkidMarkEmitter[] skidMarkEmitters;

		private GroundType groundType;

		private bool _enableParticles = true;

		public bool forceLowHP;
	}
}
