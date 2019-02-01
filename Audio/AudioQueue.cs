using System;
using System.Collections.Generic;
using FMod;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Audio
{
	public class AudioQueue : GameHubBehaviour
	{
		public bool IsPlaying
		{
			get
			{
				return this._currentPlayingAudio != null && !this._currentPlayingAudio.IsInvalidated();
			}
		}

		private void Update()
		{
			if (this._currentPlayingAudio != null && !this._currentPlayingAudio.IsInvalidated())
			{
				return;
			}
			if (!this.queuePaused && this._queue.Count > 0)
			{
				if (this._queue.Count > 1)
				{
					this._queue.Sort(new Comparison<AudioQueue.QueuedAudioDescriptor>(this.QueueSortComparison));
				}
				AudioQueue.QueuedAudioDescriptor descriptor = this._queue[0];
				this._queue.RemoveAt(0);
				this.ConsumeAudio(descriptor);
			}
		}

		private void ConsumeAudio(AudioQueue.QueuedAudioDescriptor descriptor)
		{
			if (Time.timeSinceLevelLoad - descriptor.playTime < descriptor.timeout)
			{
				this._currentPlayingAudio = FMODAudioManager.PlayAtVolume(descriptor.asset, descriptor.target, this.volume, false);
				this.AddCooldown(descriptor.asset, descriptor.cooldown);
			}
		}

		public void OnDestroy()
		{
			this.StopAll();
			this.Cleanup();
		}

		public void StopAll()
		{
			if (this._currentPlayingAudio != null && !this._currentPlayingAudio.IsInvalidated())
			{
				this._currentPlayingAudio.Stop();
			}
			this._queue.Clear();
		}

		protected virtual void Cleanup()
		{
		}

		private void AddCooldown(FMODAsset asset, float cooldown)
		{
			if (cooldown > 0f)
			{
				AudioQueue.TimedAudioDescriptor item = default(AudioQueue.TimedAudioDescriptor);
				item.assetId = asset.idGUID;
				item.time = cooldown + Time.timeSinceLevelLoad;
				this._cooldownList.Add(item);
			}
		}

		public virtual void Initialize(CarComponentHub carHub)
		{
			if (!GameHubBehaviour.Hub.AudioSettings)
			{
				AudioQueue.Log.Warn("AudioSettings not found. exiting");
				base.enabled = false;
				return;
			}
			this.SetSettings(GameHubBehaviour.Hub.AudioSettings);
		}

		private bool IsCooldownReady(FMODVoiceOverAsset asset, out float cooldown)
		{
			cooldown = asset.Cooldown;
			for (int i = this._cooldownList.Count - 1; i >= 0; i--)
			{
				if (this._cooldownList[i].time <= Time.timeSinceLevelLoad)
				{
					this._cooldownList.RemoveAt(i);
				}
			}
			if (cooldown > 0f)
			{
				for (int j = 0; j < this._cooldownList.Count; j++)
				{
					if (this._cooldownList[j].assetId == asset.idGUID)
					{
						return false;
					}
				}
			}
			return true;
		}

		public void SetSettings(AudioSettings audioSettings)
		{
			this._audioSettings = audioSettings;
		}

		public void Enqueue(FMODVoiceOverAsset asset, Transform target)
		{
			float cooldown;
			if (!this.IsCooldownReady(asset, out cooldown))
			{
				return;
			}
			this.EnqueueAudio(asset, target, cooldown);
		}

		private void EnqueueAudio(FMODVoiceOverAsset asset, Transform target, float cooldown)
		{
			float timeout = asset.Timeout;
			int priority = asset.Priority;
			if (priority == 0)
			{
				this.StopAll();
				this._currentPlayingAudio = FMODAudioManager.PlayAtVolume(asset, target, this.volume, false);
				this.AddCooldown(asset, cooldown);
				return;
			}
			if (this._queue.Count > 7)
			{
				int index = this._queue.Count - 1;
				if (this._queue[index].priority <= priority)
				{
					return;
				}
				this._queue.RemoveAt(index);
			}
			AudioQueue.QueuedAudioDescriptor item = default(AudioQueue.QueuedAudioDescriptor);
			item.asset = asset;
			item.target = target;
			item.priority = priority;
			item.playTime = Time.timeSinceLevelLoad;
			item.timeout = timeout;
			item.cooldown = cooldown;
			this._queue.Add(item);
		}

		private int QueueSortComparison(AudioQueue.QueuedAudioDescriptor x, AudioQueue.QueuedAudioDescriptor y)
		{
			return x.priority.CompareTo(y.priority);
		}

		public void SetVolumeByIdentifiable(Identifiable id)
		{
			this.volume = this._audioSettings.GetVolume(id);
		}

		public void PauseQueue(bool pause)
		{
			this.queuePaused = pause;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(AudioQueue));

		public float volume = 1f;

		private List<AudioQueue.QueuedAudioDescriptor> _queue = new List<AudioQueue.QueuedAudioDescriptor>(10);

		private List<AudioQueue.TimedAudioDescriptor> _cooldownList = new List<AudioQueue.TimedAudioDescriptor>(10);

		private FMODAudioManager.FMODAudio _currentPlayingAudio;

		protected AudioSettings _audioSettings;

		private bool queuePaused;

		public struct QueuedAudioDescriptor
		{
			public FMODAsset asset;

			public float playTime;

			public Transform target;

			public int priority;

			public float timeout;

			public float delay;

			public float cooldown;
		}

		public struct TimedAudioDescriptor
		{
			public Guid assetId;

			public float time;
		}
	}
}
