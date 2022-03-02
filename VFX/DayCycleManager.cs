using System;
using System.Collections.Generic;
using HeavyMetalMachines.GameCamera;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class DayCycleManager : GameHubBehaviour
	{
		private DayCycleManager.DayCycleStage CurrentStage
		{
			get
			{
				return this.Stages[this._currentStageIdx];
			}
		}

		private DayCycleManager.DayCycleStage PreviousStage
		{
			get
			{
				return this.Stages[this._previousStageIdx];
			}
		}

		private long NowMillis
		{
			get
			{
				return (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			}
		}

		private bool MustGoToNextStage
		{
			get
			{
				return this.NowMillis - this._currentStageStartMillis >= this.CurrentStage.DurationMillis;
			}
		}

		private float CurrentTransitionPercentage
		{
			get
			{
				return (float)(this.NowMillis - this._currentStageStartMillis) / (float)this.CurrentStage.TransitionTimeMillis;
			}
		}

		private void Start()
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				base.enabled = false;
				return;
			}
			DayCycleManager._singleton = this;
			this._cameraLight = this._gameCameraEngine.UnityCamera.GetComponentInChildren<Light>();
			this._cameraLightTransform = this._cameraLight.transform;
			this.ApplyCurrentStage(true);
		}

		private void OnDestroy()
		{
			DayCycleManager._singleton = null;
		}

		private void Update()
		{
			if (this._cameraLight.intensity != this.CurrentStage.LightIntensity)
			{
				this._cameraLight.intensity = Mathf.Lerp(this.PreviousStage.LightIntensity, this.CurrentStage.LightIntensity, this.CurrentTransitionPercentage);
			}
			if (RenderSettings.ambientLight != this.CurrentStage.AmbientLight)
			{
				RenderSettings.ambientLight = Color.Lerp(this.PreviousStage.AmbientLight, this.CurrentStage.AmbientLight, this.CurrentTransitionPercentage);
			}
			if (this._cameraLightTransform.rotation != this.CurrentStage.RotationQuaternion)
			{
				this._cameraLightTransform.rotation = Quaternion.Lerp(this.PreviousStage.RotationQuaternion, this.CurrentStage.RotationQuaternion, this.CurrentTransitionPercentage);
			}
			if (!this.IsDebug)
			{
				return;
			}
			if (Input.GetKeyDown(116))
			{
				this.GoToNextStage();
			}
			if (Input.GetKeyDown(114))
			{
				this.GoToPreviousStage();
			}
			if (Input.GetKeyDown(103))
			{
				this.GoToNextStageInstant();
			}
			if (Input.GetKeyDown(102))
			{
				this.GoToPreviousStageInstant();
			}
			if (this._timedUpdater.ShouldHalt())
			{
				return;
			}
			if (this.MustGoToNextStage)
			{
				this.GoToNextStage();
			}
		}

		private void ChangeState(bool forward, bool isInstant)
		{
			this._previousStageIdx = this._currentStageIdx;
			if (forward)
			{
				this._currentStageIdx++;
				if (this._currentStageIdx >= this.Stages.Length)
				{
					this._currentStageIdx = 0;
				}
			}
			else
			{
				this._currentStageIdx--;
				if (this._currentStageIdx < 0)
				{
					this._currentStageIdx = this.Stages.Length - 1;
				}
			}
			this.ApplyCurrentStage(isInstant);
		}

		private void ApplyCurrentStage(bool isInstant)
		{
			if (isInstant)
			{
				RenderSettings.ambientLight = this.CurrentStage.AmbientLight;
				this._cameraLight.intensity = this.CurrentStage.LightIntensity;
				this._cameraLightTransform.rotation = this.CurrentStage.RotationQuaternion;
			}
			this._currentStageStartMillis = this.NowMillis;
			for (int i = 0; i < this.FXs.Count; i++)
			{
				this.FXs[i].UpdateStates(this.CurrentStage.Id);
			}
		}

		private void GoToNextStage()
		{
			this.ChangeState(true, false);
		}

		private void GoToNextStageInstant()
		{
			this.ChangeState(true, true);
		}

		private void GoToPreviousStage()
		{
			this.ChangeState(false, false);
		}

		private void GoToPreviousStageInstant()
		{
			this.ChangeState(false, true);
		}

		public static void AddFX(DayCycleFX fx)
		{
			if (!DayCycleManager._singleton)
			{
				return;
			}
			DayCycleManager._singleton.FXs.Add(fx);
			fx.UpdateStates(DayCycleManager._singleton.CurrentStage.Id);
		}

		public static void RemoveFX(DayCycleFX fx)
		{
			if (!DayCycleManager._singleton)
			{
				return;
			}
			DayCycleManager._singleton.FXs.Remove(fx);
		}

		[InjectOnClient]
		private IGameCameraEngine _gameCameraEngine;

		public bool IsDebug;

		public DayCycleManager.DayCycleStage[] Stages;

		private int _currentStageIdx;

		private int _previousStageIdx;

		private long _currentStageStartMillis;

		private Light _cameraLight;

		private Transform _cameraLightTransform;

		private TimedUpdater _timedUpdater = new TimedUpdater
		{
			PeriodMillis = 100
		};

		private List<DayCycleFX> FXs = new List<DayCycleFX>();

		private static DayCycleManager _singleton;

		[Serializable]
		public class DayCycleStage
		{
			public long TransitionTimeMillis
			{
				get
				{
					return (long)this.TransitionTime * 1000L;
				}
			}

			public long DurationMillis
			{
				get
				{
					return (long)this.Duration * 1000L;
				}
			}

			public Quaternion RotationQuaternion
			{
				get
				{
					return Quaternion.Euler(this.Rotation);
				}
			}

			public string Name;

			public DayCycleManager.DayCycleStageId Id;

			public Color AmbientLight;

			public float LightIntensity;

			public float Duration;

			public float TransitionTime;

			public Vector3 Rotation;
		}

		public enum DayCycleStageId
		{
			Day,
			Sunset1,
			Sunset2,
			Night,
			Sunrise1,
			Sunrise2
		}
	}
}
