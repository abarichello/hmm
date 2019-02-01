using System;
using FMod;
using HeavyMetalMachines.Car;
using HeavyMetalMachines.Character;
using HeavyMetalMachines.Combat;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Audio
{
	public class CarAudioController : GameHubBehaviour
	{
		private Transform RenderTransform
		{
			get
			{
				return this._carHub.RenderTransform;
			}
		}

		private CarMovement carMovement
		{
			get
			{
				return this._carHub.carMovement;
			}
		}

		private HeavyMetalMachines.Character.CharacterInfo characterInfo
		{
			get
			{
				return this._carHub.Player.Character;
			}
		}

		private CarInput carInput
		{
			get
			{
				return this._carHub.carInput;
			}
		}

		private CombatObject combat
		{
			get
			{
				return this._carHub.combatObject;
			}
		}

		private bool IsLocalPlayer
		{
			get
			{
				return this._carHub.combatObject.IsLocalPlayer;
			}
		}

		private void GridController_ListenToGridGameFinished(byte arg1, float arg2)
		{
			if (!this.IsLocalPlayer)
			{
				return;
			}
			if (this.engineAudioSrc == null || this.engineAudioSrc.IsInvalidated())
			{
				this.StartEngineAudio();
			}
			this.engineAudioSrc.SetParameter(CarAudioController.LimiterParam, 0f);
			this.engineState = CarAudioController.State.Moving;
			if (this.IsLocalPlayer && GameHubBehaviour.Hub.BombManager.GridController.CurrentPlayer != null)
			{
				GameHubBehaviour.Hub.BombManager.GridController.CurrentPlayer.OnValueChanged -= this.CurrentPlayer_OnValueChanged;
			}
		}

		private void CurrentPlayer_OnValueChanged(float value)
		{
			if (this.engineAudioSrc == null || this.engineAudioSrc.IsInvalidated())
			{
				this.StartEngineAudio();
			}
			this.targetRPM = this.characterInfo.carAudio.RpmGridCurve.Evaluate(value);
			this.rpm = Mathf.Clamp(Mathf.Lerp(this.rpm, this.targetRPM, this.characterInfo.carAudio.rpmGain), this.characterInfo.carAudio.idleRPM, this.characterInfo.carAudio.maxRPM);
			this.engineAudioSrc.SetParameter(CarAudioController.RPMParam, this.rpm);
		}

		private void GridController_ListenToGridGameStarted()
		{
			if (!this.IsLocalPlayer)
			{
				return;
			}
			if (this.engineAudioSrc == null || this.engineAudioSrc.IsInvalidated())
			{
				this.StartEngineAudio();
			}
			this.engineState = CarAudioController.State.OnGrid;
			this.engineAudioSrc.SetParameter(CarAudioController.LimiterParam, 1f);
			if (GameHubBehaviour.Hub.BombManager.GridController.CurrentPlayer != null)
			{
				GameHubBehaviour.Hub.BombManager.GridController.CurrentPlayer.OnValueChanged += this.CurrentPlayer_OnValueChanged;
			}
		}

		private void ActivateListeners()
		{
			if (!GameHubBehaviour.Hub.Match.LevelIsTutorial() && !GameHubBehaviour.Hub.Net.isTest && this.IsLocalPlayer)
			{
				GameHubBehaviour.Hub.BombManager.GridController.ListenToGridGameStarted += this.GridController_ListenToGridGameStarted;
				GameHubBehaviour.Hub.BombManager.GridController.ListenToGridGameFinished += this.GridController_ListenToGridGameFinished;
			}
		}

		private void DeactivateListeners()
		{
			GameHubBehaviour.Hub.BombManager.GridController.ListenToGridGameStarted -= this.GridController_ListenToGridGameStarted;
			GameHubBehaviour.Hub.BombManager.GridController.ListenToGridGameFinished -= this.GridController_ListenToGridGameFinished;
			if (this.carMovement)
			{
				this.combat.Feedback.OnCollisionEvent -= this.OnCarCollision;
			}
		}

		private void Cleanup()
		{
			if (this.engineAudioSrc != null)
			{
				this.engineAudioSrc.Stop();
				FMODAudioManager.Clean(this.engineAudioSrc);
			}
		}

		private void Awake()
		{
			this._isServer = (GameHubBehaviour.Hub.Net.IsServer() && !GameHubBehaviour.Hub.Net.isTest);
			if (this._isServer)
			{
				base.enabled = false;
			}
		}

		private void OnDestroy()
		{
			if (this._isServer)
			{
				return;
			}
			this.Cleanup();
			this.DeactivateListeners();
		}

		private void OnEnable()
		{
			if (this._isServer)
			{
				return;
			}
			if (this._carHub == null)
			{
				return;
			}
			this.StartEngineAudio();
		}

		private void OnDisable()
		{
			if (this._isServer)
			{
				return;
			}
			this.StopEngineAudio();
			this.StopDriftAudio();
		}

		public void StartEngineAudio()
		{
			if (this._carHub == null || this.carMovement == null || this.characterInfo == null || this.characterInfo.carAudio == null)
			{
				CarAudioController.Log.Warn("engine initialization failed");
				return;
			}
			if (this.engineAudioSrc != null)
			{
				this.ResetEngineParams();
				this.engineAudioSrc.ResetTimeline();
				return;
			}
			FMODAsset fmodasset = (!this.IsLocalPlayer) ? this.characterInfo.carAudio.Engine_Others : this.characterInfo.carAudio.Engine_Player;
			if (fmodasset == null)
			{
				CarAudioController.Log.ErrorFormat("null engine. Player: {0} IsLocalPlayer: {1}", new object[]
				{
					base.name,
					this.IsLocalPlayer
				});
				return;
			}
			this.engineAudioSrc = FMODAudioManager.PlayAtVolume(fmodasset, this.RenderTransform, 1f, true);
		}

		public void PlayDriftAudio()
		{
			if (this.carMovement == null || this.engineAudioSrc == null || this.engineAudioSrc.IsInvalidated())
			{
				return;
			}
			if (!this.IsLocalPlayer)
			{
				return;
			}
			this.engineAudioSrc.SetParameter(CarAudioController.DriftParam, (float)((Mathf.Abs(this.carMovement.SpeedZ) <= 0.1f) ? 0 : 1));
		}

		public void StopEngineAudio()
		{
			if (this.engineAudioSrc != null && !this.engineAudioSrc.IsInvalidated())
			{
				this.ResetEngineParams();
			}
		}

		public void StopDriftAudio()
		{
			if (this.carMovement == null || this.engineAudioSrc == null || this.engineAudioSrc.IsInvalidated())
			{
				return;
			}
			this.engineAudioSrc.SetParameter(CarAudioController.DriftParam, 0f);
		}

		private void OnCarCollision(Vector3 position, Vector3 direction, float intensity, byte otherLayer)
		{
			this.lastCollision = Time.timeSinceLevelLoad;
		}

		private void UpdateEngineAudio()
		{
			if (!this.carMovement)
			{
				return;
			}
			if (this.engineAudioSrc == null || this.engineAudioSrc.IsInvalidated())
			{
				this.StartEngineAudio();
				return;
			}
			float deltaTime = Time.deltaTime;
			if (this.gearShiftDelay > 0f)
			{
				this.gearShiftDelay -= deltaTime;
			}
			if (this.carMovement.IsDrifting)
			{
				this.timeDrifting += deltaTime;
				this.driftRPM = Mathf.Lerp(this.driftRPM, this.characterInfo.carAudio.MaxDriftRPMGain, this.characterInfo.carAudio.driftRPMGain);
				if (this.gearShiftDelay <= 0f && this.currentGear > 0)
				{
					this.GearShiftDown();
				}
				if (this.timeDrifting > this.characterInfo.carAudio.driftDelay)
				{
					this.timeDrifting = this.characterInfo.carAudio.driftDelay;
				}
			}
			else if (this.timeDrifting > 0f)
			{
				this.timeDrifting -= deltaTime;
				if (this.timeDrifting <= 0f && this.currentGear > 0)
				{
					this.timeDrifting = 0f;
					this.GearShiftDown();
				}
			}
			else
			{
				this.driftRPM = Mathf.Lerp(this.driftRPM, 0f, this.characterInfo.carAudio.driftRPMGain);
			}
			bool flag;
			if (this.combat.Id.IsOwner)
			{
				flag = (this.carInput.TargetV != 0f);
			}
			else
			{
				flag = (this.combat.IsAlive() && Mathf.Abs(this.carMovement.SpeedZ) > 0.1f);
			}
			if (flag)
			{
				this.fakeRPMSpeed += deltaTime;
				float time = Mathf.Clamp(this.fakeRPMSpeed, 0f, this.gearLengths[this.currentGear]);
				this.desiredRPM = this.characterInfo.carAudio.RpmGearCurves[this.currentGear].Evaluate(time) + (float)this.characterInfo.carAudio.rpmFloor;
				if (this.carInput.TargetV > 0f)
				{
					if (this.timeDrifting <= 0f && this.gearShiftDelay <= 0f && this.fakeRPMSpeed >= this.gearLengths[this.currentGear])
					{
						this.GearShiftUp();
					}
				}
				else
				{
					this.currentGear = 0;
				}
				int num = Mathf.FloorToInt(Mathf.Abs(this.carInput.TargetY));
				this.rpmLoss = this.characterInfo.carAudio.RpmTurningCurve.Evaluate((float)num);
				int num2 = Mathf.FloorToInt(Mathf.Abs(this.carMovement.SpeedZ));
				this.targetRPM = this.desiredRPM + this.driftRPM + this.characterInfo.carAudio.RpmGainBySpeedCurve.Evaluate((float)num2);
				if (this.fakeRPMSpeed > this.characterInfo.carAudio.releaseTime)
				{
					this.engineAudioSrc.SetParameter(CarAudioController.ReleaseParam, 0f);
				}
				if (this.castTimer <= 0f && this.IsLocalPlayer)
				{
					this.engineAudioSrc.SetParameter(CarAudioController.ShortCastParam, 1f);
					this.castTimer = 1f;
				}
				this.targetRPM = Mathf.Clamp(this.targetRPM, this.characterInfo.carAudio.idleRPM, this.characterInfo.carAudio.maxRPM);
			}
			else
			{
				if (!this.carInput.IsAccelerating && !this.carInput.IsReverse && this.IsLocalPlayer)
				{
					this.engineAudioSrc.SetParameter(CarAudioController.ReleaseParam, 1f);
				}
				if (this.castTimer > 0f)
				{
					this.castTimer -= Time.deltaTime;
					this.engineAudioSrc.SetParameter(CarAudioController.ShortCastParam, 0f);
				}
				this.currentGear = 0;
				this.fakeRPMSpeed = 0f;
				this.rpmLoss = 0f;
				this.targetRPM = this.characterInfo.carAudio.idleRPM;
			}
			this.rpm = Mathf.Clamp(Mathf.Lerp(this.rpm, this.targetRPM - this.rpmLoss, this.characterInfo.carAudio.rpmGain), this.characterInfo.carAudio.idleRPM, this.characterInfo.carAudio.maxRPM);
			this.engineAudioSrc.SetParameter(CarAudioController.RPMParam, this.rpm);
			this.engineAudioSrc.SetParameter(CarAudioController.ReverseParam, (float)((this.carInput.TargetV >= 0f) ? 0 : 1));
			this.lastSpeed = this.carMovement.SpeedZ;
		}

		private void GearShiftUp()
		{
			this.currentGear = 1;
			this.desiredRPM -= this.characterInfo.carAudio.gearShitRPMLoss * SysRandom.Float(0f, 1f);
			this.fakeRPMSpeed = 0f;
			this.gearShiftDelay = this.characterInfo.carAudio.gearUpDelay;
		}

		private void GearShiftDown()
		{
			this.currentGear = 0;
			this.fakeRPMSpeed = this.gearLengths[this.currentGear] * this.characterInfo.carAudio.rpmGearShiftPercent;
			this.gearShiftDelay = this.characterInfo.carAudio.gearDownDelay;
		}

		private void ResetEngineParams()
		{
			if (this.engineAudioSrc == null || this.engineAudioSrc.IsInvalidated())
			{
				return;
			}
			this.engineState = CarAudioController.State.Stopped;
			this.engineAudioSrc.SetParameter(CarAudioController.RPMParam, 0f);
			this.engineAudioSrc.SetParameter(CarAudioController.DriftParam, 0f);
			this.engineAudioSrc.SetParameter(CarAudioController.ReleaseParam, 0f);
			this.engineAudioSrc.SetParameter(CarAudioController.ShortCastParam, 0f);
			this.engineAudioSrc.SetParameter(CarAudioController.LimiterParam, 0f);
		}

		private void Update()
		{
			if (this.carMovement == null)
			{
				return;
			}
			if (GameHubBehaviour.Hub.Match.LevelIsTutorial())
			{
				this.UpdateEngineAudio();
				return;
			}
			switch (GameHubBehaviour.Hub.BombManager.CurrentBombGameState)
			{
			case BombScoreBoard.State.Warmup:
			case BombScoreBoard.State.Shop:
			case BombScoreBoard.State.EndGame:
				this.ResetEngineParams();
				return;
			case BombScoreBoard.State.PreBomb:
				return;
			case BombScoreBoard.State.BombDelivery:
			case BombScoreBoard.State.PreReplay:
			case BombScoreBoard.State.Replay:
				this.UpdateEngineAudio();
				break;
			}
		}

		public void Initialize(CarComponentHub carHub)
		{
			if (!GameHubBehaviour.Hub || (GameHubBehaviour.Hub.Net.IsServer() && !GameHubBehaviour.Hub.Net.isTest))
			{
				base.enabled = false;
				return;
			}
			this._carHub = carHub;
			this.gearLengths = new float[this.characterInfo.carAudio.RpmGearCurves.Length];
			for (int i = 0; i < this.gearLengths.Length; i++)
			{
				AnimationCurve animationCurve = this.characterInfo.carAudio.RpmGearCurves[i];
				this.gearLengths[i] = animationCurve.keys[animationCurve.length - 1].time;
			}
			this.ActivateListeners();
			this.characterInfo.carAudio.Preload();
		}

		private static readonly BitLogger Log = new BitLogger(typeof(CarAudioController));

		private static readonly byte[] RPMParam = FMODAudioManager.GetBytes("RPM");

		private static readonly byte[] DriftParam = FMODAudioManager.GetBytes("Drift");

		private static readonly byte[] ReleaseParam = FMODAudioManager.GetBytes("Release");

		private static readonly byte[] ShortCastParam = FMODAudioManager.GetBytes("ShortCast");

		private static readonly byte[] LimiterParam = FMODAudioManager.GetBytes("Limiter");

		private static readonly byte[] ReverseParam = FMODAudioManager.GetBytes("Reverse");

		private bool _isServer;

		public CarAudioController.State engineState;

		public FMODAudioManager.FMODAudio engineAudioSrc;

		private float lastCollision;

		private float lastRelease;

		private CarComponentHub _carHub;

		private float gearShiftDelay;

		private float timeDrifting;

		private float driftRPM;

		private int currentGear;

		private float fakeRPMSpeed;

		private float gearChangeTime;

		private float[] gearLengths;

		private float desiredRPM;

		private float rpm;

		private float rpmLoss;

		private float targetRPM;

		private float lastSpeed;

		private float castTimer;

		public enum State
		{
			Stopped,
			Moving,
			OnGrid
		}
	}
}
