using System;
using HeavyMetalMachines.Playback;
using Pocketverse;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HORTATimeController : ITimelineController
	{
		public HORTATimeController(HORTATime time, HORTAPlayback playback)
		{
			this.CurrentSpeedIndex = 2;
			this._playing = true;
			this._hortaTime = time;
			this._playback = playback;
			this._available = true;
			this._speedChangeSubject = new Subject<int>();
			this._availabilitySubject = new Subject<bool>();
		}

		public int TimelineSizeMillis
		{
			get
			{
				return this._hortaTime.LastFrameTime;
			}
		}

		public int CurrentSpeedIndex { get; private set; }

		public float[] AvailableSpeeds
		{
			get
			{
				return this._speeds;
			}
		}

		public bool IsPlaying
		{
			get
			{
				return this._playing;
			}
		}

		public void Pause()
		{
			if (!this._playing)
			{
				return;
			}
			this.TogglePause();
		}

		public void Play()
		{
			if (this._playing)
			{
				return;
			}
			this.TogglePause();
		}

		public int IncreaseSpeed()
		{
			this.CurrentSpeedIndex = Mathf.Min(this.CurrentSpeedIndex + 1, this._speeds.Length - 1);
			if (this._playing)
			{
				this.SetSpeed();
			}
			this._speedChangeSubject.OnNext(this.CurrentSpeedIndex);
			return this.CurrentSpeedIndex;
		}

		public int DecreaseSpeed()
		{
			this.CurrentSpeedIndex = Mathf.Max(0, this.CurrentSpeedIndex - 1);
			if (this._playing)
			{
				this.SetSpeed();
			}
			this._speedChangeSubject.OnNext(this.CurrentSpeedIndex);
			return this.CurrentSpeedIndex;
		}

		public void SetTime(int targetTime)
		{
			int synchTime = this._hortaTime.GetSynchTime();
			HORTATimeController.Log.DebugFormat("Setting time={0} currentTime={1}", new object[]
			{
				(float)targetTime / 1000f,
				(float)synchTime / 1000f
			});
			if (synchTime < targetTime)
			{
				this.DisableAvailability();
				this._hortaTime.RunTo(targetTime, new Action(this.OnRunEnded));
				return;
			}
			this._hortaTime.SetTime(targetTime);
			this._playback.FixTime(synchTime);
		}

		private void OnRunEnded()
		{
			this.EnableAvailability();
		}

		public IObservable<int> ObserveSpeedChange()
		{
			return this._speedChangeSubject;
		}

		public IObservable<bool> ObserveAvailability()
		{
			return Observable.StartWith<bool>(this._availabilitySubject, this._available);
		}

		public void DisableAvailability()
		{
			this._available = false;
			this._availabilitySubject.OnNext(false);
		}

		public void EnableAvailability()
		{
			this._available = true;
			this._availabilitySubject.OnNext(true);
		}

		private void SetSpeed()
		{
			this._hortaTime.SetTimeScale(this.AvailableSpeeds[this.CurrentSpeedIndex]);
		}

		private void TogglePause()
		{
			if (!this._playing)
			{
				this._hortaTime.SetTimeScale(this.AvailableSpeeds[this.CurrentSpeedIndex]);
				this._playing = true;
			}
			else
			{
				this._hortaTime.SetTimeScale(0f);
				this._playing = false;
			}
		}

		public void Reset()
		{
			this.CurrentSpeedIndex = 2;
			this._playing = true;
			this.SetSpeed();
		}

		public static readonly BitLogger Log = new BitLogger(typeof(HORTATimeController));

		private bool _playing;

		private readonly HORTATime _hortaTime;

		private readonly HORTAPlayback _playback;

		private readonly Subject<int> _speedChangeSubject;

		private readonly Subject<bool> _availabilitySubject;

		private bool _available;

		private readonly float[] _speeds = new float[]
		{
			0.25f,
			0.5f,
			1f,
			2f,
			4f
		};
	}
}
