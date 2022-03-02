using System;
using System.Collections.Generic;

namespace HeavyMetalMachines.Playback
{
	public class DefaultProcessContext : IFrameProcessContext
	{
		public DefaultProcessContext(Func<int> getTimeFunc)
		{
			this._getTime = getTimeFunc;
			this._executionQueue = new List<int>();
			this._actionQueue = new SortedList<int, Action>();
		}

		public int PlaybackTime
		{
			get
			{
				return this._getTime();
			}
		}

		public List<int> ExecutionQueue
		{
			get
			{
				return this._executionQueue;
			}
		}

		public SortedList<int, Action> ActionQueue
		{
			get
			{
				return this._actionQueue;
			}
		}

		public void AddToExecutionQueue(int frameId)
		{
			if (!this._executionQueue.Contains(frameId))
			{
				this._executionQueue.Add(frameId);
			}
		}

		public bool RemoveFromExecutionQueue(int frameId)
		{
			return this._executionQueue.Remove(frameId);
		}

		public void ClearQueue()
		{
			this._executionQueue.Clear();
			this._actionQueue.Clear();
		}

		public void EnqueueAction(int time, Action action)
		{
			if (this._actionQueue.ContainsKey(time))
			{
				this.EnqueueAction(time + 1, action);
				return;
			}
			this._actionQueue[time] = action;
		}

		private Func<int> _getTime;

		private List<int> _executionQueue;

		private SortedList<int, Action> _actionQueue;
	}
}
