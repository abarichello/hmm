using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public class CreepPositionUpdater : GameHubBehaviour
	{
		private void Start()
		{
			this._timeMillis = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			this._timer = (float)this._timeMillis;
		}

		private void Update()
		{
			this._timeMillis = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			if (this._timer + 1000f > (float)this._timeMillis)
			{
				return;
			}
			UnityUtils.SnapToGroundPlane(base.transform, this.Height);
		}

		public float Height = 3f;

		private const float TimerInterval = 1000f;

		private float _timer;

		private long _timeMillis;
	}
}
