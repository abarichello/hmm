using System;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HORTATime : IGameTime
	{
		public HORTATime()
		{
			this.MatchTimer = new TimeUtils.Chronometer(new Func<int>(this.GetPlaybackTime));
			this._playbackStartTime = -1L;
			this._paused = false;
			this._originalFixedDeltaTime = Time.fixedDeltaTime;
		}

		public long PlaybackStartTime
		{
			get
			{
				return this._playbackStartTime;
			}
		}

		private long TimeMillis
		{
			get
			{
				return (long)((int)(Time.realtimeSinceStartup * 1000f));
			}
		}

		public int GetSynchTime()
		{
			if (this._playbackStartTime < 0L)
			{
				return 0;
			}
			return (int)Math.Max(0L, this.TimeMillis - this._playbackStartTime);
		}

		public int GetPlaybackTime()
		{
			int num = (int)((float)(this.GetSynchTime() - this.LastSynchTimeScaleChange) * Time.timeScale);
			int num2 = this.RewindedTimeMillis + this.AccumulatedSynchDelay;
			return this.LastSynchTimeScaleChange + num - num2;
		}

		public float GetPlaybackUnityTime()
		{
			return Time.timeSinceLevelLoad - this._playbackUnityTime - (float)this.RewindedTimeMillis * 0.001f;
		}

		public void SetTimeScale(float timeScale)
		{
			int synchTime = this.GetSynchTime();
			float num = (float)(synchTime - this.LastSynchTimeScaleChange) * (1f - Time.timeScale);
			float num2 = num - (float)Math.Truncate((double)num) + this._accumulatedFloatError;
			if (num2 >= 1f)
			{
				this.AccumulatedSynchDelay += (int)(num + this._accumulatedFloatError);
				this._accumulatedFloatError = num2 - (float)Math.Truncate((double)num2);
			}
			else
			{
				this.AccumulatedSynchDelay += (int)num;
				this._accumulatedFloatError = num2;
			}
			this.LastSynchTimeScaleChange = synchTime;
			Time.timeScale = timeScale;
			Time.fixedDeltaTime = this._originalFixedDeltaTime * timeScale;
			this.LastSynchTimeScaleChange = this.GetSynchTime();
			Time.timeScale = timeScale;
		}

		public TimeUtils.Chronometer MatchTimer { get; private set; }

		public void SetTimeZero()
		{
			this._playbackStartTime = this.TimeMillis;
			this._playbackUnityTime = Time.timeSinceLevelLoad;
			this.AccumulatedSynchDelay = 0;
			this._accumulatedFloatError = 0f;
			this.LastSynchTimeScaleChange = this.GetSynchTime();
		}

		public void SetTimeZero(long playbackStartTime, int lastSynchTimeScaleChange, int accumulatedSynchDelay, float timeScale)
		{
			throw new NotImplementedException();
		}

		public int AccumulatedSynchDelay { get; private set; }

		public int LastSynchTimeScaleChange { get; private set; }

		public int RewindedTimeMillis { get; set; }

		public void TogglePause()
		{
			if (this._fast)
			{
				this.ToggleFastForward();
			}
			if (this._slow)
			{
				this.ToggleSlowMotion();
			}
			if (this._paused)
			{
				this.SetTimeScale(this._lastScale);
				this._paused = false;
			}
			else
			{
				this._lastScale = Time.timeScale;
				this.SetTimeScale(0f);
				this._paused = true;
			}
		}

		public void ToggleFastForward()
		{
			if (this._paused)
			{
				this.TogglePause();
			}
			if (this._slow)
			{
				this.ToggleSlowMotion();
			}
			if (this._fast)
			{
				this.SetTimeScale(this._lastScale);
				this._fast = false;
			}
			else
			{
				this._lastScale = Time.timeScale;
				this.SetTimeScale(3f);
				this._fast = true;
			}
		}

		public void ToggleSlowMotion()
		{
			if (this._paused)
			{
				this.TogglePause();
			}
			if (this._fast)
			{
				this.ToggleFastForward();
			}
			if (this._slow)
			{
				this.SetTimeScale(this._lastScale);
				this._slow = false;
			}
			else
			{
				this._lastScale = Time.timeScale;
				this.SetTimeScale(0.33f);
				this._slow = true;
			}
		}

		public void ToggleOffAny()
		{
			if (this._paused)
			{
				this.TogglePause();
			}
			if (this._fast)
			{
				this.ToggleFastForward();
			}
			if (this._slow)
			{
				this.ToggleSlowMotion();
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(HORTATime));

		private float _lastScale;

		private bool _paused;

		private bool _fast;

		private bool _slow;

		private float _playbackUnityTime = -1f;

		private long _playbackStartTime = -1L;

		private float _originalFixedDeltaTime;

		private float _accumulatedFloatError;
	}
}
