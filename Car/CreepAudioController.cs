using System;
using FMod;
using Pocketverse;

namespace HeavyMetalMachines.Car
{
	public class CreepAudioController : GameHubBehaviour
	{
		public CarMovement CarMovement
		{
			get
			{
				return this.carMovement;
			}
			set
			{
				this.carMovement = value;
				this.StartEngineAudio();
			}
		}

		private void Awake()
		{
			if (!GameHubBehaviour.Hub || GameHubBehaviour.Hub.Net.IsServer())
			{
				base.enabled = false;
			}
		}

		public void StartEngineAudio()
		{
			if (!GameHubBehaviour.Hub || GameHubBehaviour.Hub.Net.IsServer() || GameHubBehaviour.Hub.AudioSettings == null)
			{
				return;
			}
			FMODAsset creepEngineAudioAsset = GameHubBehaviour.Hub.AudioSettings.CreepEngineAudioAsset;
			if (creepEngineAudioAsset == null)
			{
				return;
			}
			if (!this.m_boInitialized)
			{
				this.engineAudioSrc = FMODAudioManager.PlayAt(creepEngineAudioAsset, base.transform);
				this.m_boInitialized = true;
			}
		}

		public void StopEngineAudio()
		{
			if (this.engineAudioSrc != null)
			{
				this.engineAudioSrc.Stop();
			}
			this.engineAudioSrc = null;
			this.m_boInitialized = false;
		}

		private void OnEnable()
		{
			this.StartEngineAudio();
		}

		private void OnDisable()
		{
			this.StopEngineAudio();
		}

		private void Update()
		{
			if (this.CarMovement == null)
			{
				return;
			}
			if (this.engineAudioSrc == null || this.engineAudioSrc.IsInvalidated())
			{
				this.StopEngineAudio();
				this.StartEngineAudio();
			}
		}

		[NonSerialized]
		public FMODAudioManager.FMODAudio engineAudioSrc;

		private CarMovement carMovement;

		private bool m_boInitialized;
	}
}
