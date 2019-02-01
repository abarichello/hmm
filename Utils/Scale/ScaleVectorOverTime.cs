using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Utils.Scale
{
	public class ScaleVectorOverTime : BaseScaleVector
	{
		public ScaleVectorOverTime(Vector3 from, Vector3 to, float duration) : base(from, to)
		{
			this.SetValuesAndReset(from, to, duration);
		}

		public bool Finished { get; private set; }

		public void SetValuesAndReset(Vector3 from, Vector3 to, float duration)
		{
			base.SetValuesAndReset(from, to);
			this._durationMillis = (int)(duration * 1000f);
			this.InternalReset();
		}

		public Vector3 Update()
		{
			if (this.Finished)
			{
				return base.GetTo();
			}
			int playbackTime = GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			int num = playbackTime - this._startedTimeMillis;
			float num2 = (float)num / (float)this._durationMillis;
			if (num2 >= 1f)
			{
				num2 = 1f;
				this.Finished = true;
			}
			return base.TransformVector(num2);
		}

		private void InternalReset()
		{
			this._startedTimeMillis = GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			this.Finished = false;
		}

		private int _durationMillis;

		private int _startedTimeMillis;
	}
}
