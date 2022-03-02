using System;
using System.Diagnostics;
using HeavyMetalMachines.Utils;
using Hoplon.Time;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HORTATime : IGameTime, ICurrentTime
	{
		public HORTATime()
		{
			this.MatchTimer = new TimeUtils.Chronometer(new Func<int>(this.GetPlaybackTime));
			this.Reset();
		}

		public float CurrentTime
		{
			get
			{
				return this._currentTime;
			}
		}

		public int LastFrameTime { get; private set; }

		public long PlaybackStartTime
		{
			get
			{
				return this._playbackStartTime;
			}
		}

		public void Update()
		{
			if (!this._timeSet)
			{
				return;
			}
			if (this.GetPlaybackTime() >= this.LastFrameTime)
			{
				return;
			}
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			this._currentTime += realtimeSinceStartup - this._lastRealtimeUpdate;
			this._lastRealtimeUpdate = realtimeSinceStartup;
			if (this._running && this._currentTime > this._runTarget)
			{
				this._currentTime = this._runTarget;
				this.StopRun();
			}
		}

		private long TimeMillis
		{
			get
			{
				return (long)((int)(this._currentTime * 1000f));
			}
		}

		private int TimeMillisSinceStartTime()
		{
			if (!this._timeSet)
			{
				return 0;
			}
			return (int)this.TimeMillis;
		}

		public int GetPlaybackTime()
		{
			if (!this._timeSet)
			{
				return 0;
			}
			int num = (int)((float)(this.TimeMillisSinceStartTime() - this.LastSynchTimeScaleChange) * Time.timeScale);
			int num2 = this.RewindedTimeMillis + this.AccumulatedSynchDelay;
			return this.LastSynchTimeScaleChange + num - num2;
		}

		public int GetSynchTime()
		{
			if (!this._timeSet)
			{
				return 0;
			}
			int num = (int)((float)(this.TimeMillisSinceStartTime() - this.LastSynchTimeScaleChange) * Time.timeScale);
			return this.LastSynchTimeScaleChange + num - this.AccumulatedSynchDelay;
		}

		public float GetPlaybackUnityTime()
		{
			return Time.timeSinceLevelLoad - this._playbackUnityTime - (float)this.RewindedTimeMillis * 0.001f;
		}

		public void SetTimeScale(float timeScale)
		{
			int num = this.TimeMillisSinceStartTime();
			float num2 = (float)(num - this.LastSynchTimeScaleChange) * (1f - Time.timeScale);
			float num3 = num2 - (float)Math.Truncate((double)num2) + this._accumulatedFloatError;
			if (num3 >= 1f)
			{
				this.AccumulatedSynchDelay += (int)(num2 + this._accumulatedFloatError);
				this._accumulatedFloatError = num3 - (float)Math.Truncate((double)num3);
			}
			else
			{
				this.AccumulatedSynchDelay += (int)num2;
				this._accumulatedFloatError = num3;
			}
			this.LastSynchTimeScaleChange = num;
			Time.timeScale = timeScale;
			Time.fixedDeltaTime = this._originalFixedDeltaTime * timeScale;
		}

		public TimeUtils.Chronometer MatchTimer { get; private set; }

		public void SetTimeZero()
		{
			this._lastRealtimeUpdate = Time.realtimeSinceStartup;
			this._timeSet = true;
			this._currentTime = 0f;
			this._running = false;
			this._playbackStartTime = this.TimeMillis;
			this._playbackUnityTime = Time.timeSinceLevelLoad;
			this.AccumulatedSynchDelay = 0;
			this._accumulatedFloatError = 0f;
			this.LastSynchTimeScaleChange = this.TimeMillisSinceStartTime();
		}

		public void SetTimeZero(long playbackStartTime, int lastSynchTimeScaleChange, int accumulatedSynchDelay, float timeScale)
		{
			throw new NotImplementedException();
		}

		public int AccumulatedSynchDelay { get; private set; }

		public int LastSynchTimeScaleChange { get; private set; }

		public int RewindedTimeMillis { get; set; }

		public DateTime Now()
		{
			return DateTime.Now;
		}

		public DateTime NowServerUtc()
		{
			return DateTime.UtcNow;
		}

		public void Reset()
		{
			this._timeSet = false;
			this._currentTime = 0f;
			this._playbackUnityTime = 0f;
			this._playbackStartTime = -1L;
			this._originalFixedDeltaTime = Time.fixedDeltaTime;
			this.LastFrameTime = -1;
		}

		public void SetLastFrameTime(int time)
		{
			this.LastFrameTime = time;
		}

		public void SetTime(int millis)
		{
			bool flag = false;
			float num = 1f;
			if (Time.timeScale > 0f)
			{
				num = Time.timeScale;
			}
			else
			{
				flag = true;
				this.SetTimeScale(num);
			}
			int synchTime = this.GetSynchTime();
			int num2 = millis - synchTime;
			this._currentTime += (float)num2 / num / 1000f;
			this._playbackUnityTime -= (float)num2 * 0.001f / num;
			if (flag)
			{
				this.SetTimeScale(0f);
			}
			HORTATime.Log.DebugFormat("Time set={0} st={1} pt={2}", new object[]
			{
				millis,
				this.GetSynchTime(),
				this.GetPlaybackTime()
			});
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private event Action _onRunEnded;

		public void RunTo(int millis, Action onRunEnded)
		{
			if (onRunEnded != null)
			{
				this._onRunEnded += onRunEnded;
			}
			if (this._running)
			{
				return;
			}
			this._running = true;
			this._currentScale = Time.timeScale;
			int num = millis - this.GetSynchTime();
			this._runTarget = this._currentTime + (float)num / 16f / 1000f;
			this.SetTimeScale(16f);
		}

		private void StopRun()
		{
			if (this._onRunEnded != null)
			{
				this._onRunEnded();
				this._onRunEnded = null;
			}
			this._running = false;
			this.SetTimeScale(this._currentScale);
		}

		public static readonly BitLogger Log = new BitLogger(typeof(HORTATime));

		private float _playbackUnityTime = -1f;

		private long _playbackStartTime = -1L;

		private float _lastRealtimeUpdate;

		private float _currentTime;

		private bool _timeSet;

		private float _originalFixedDeltaTime;

		private float _accumulatedFloatError;

		private const float FastForwardSpeed = 16f;

		private float _currentScale;

		private bool _running;

		private float _runTarget;
	}
}
