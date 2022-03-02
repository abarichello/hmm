using System;

namespace HeavyMetalMachines.Playback
{
	public class NoQueueProcessContext : IFrameProcessContext
	{
		public NoQueueProcessContext(Func<int> getTimeFunc)
		{
			this._getTime = getTimeFunc;
		}

		public int PlaybackTime
		{
			get
			{
				return this._getTime();
			}
		}

		public void AddToExecutionQueue(int frameId)
		{
		}

		public bool RemoveFromExecutionQueue(int frameId)
		{
			return false;
		}

		public void EnqueueAction(int time, Action action)
		{
		}

		private Func<int> _getTime;
	}
}
