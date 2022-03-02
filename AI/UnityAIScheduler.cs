using System;
using System.Collections.Generic;
using Pocketverse.MuralContext;
using UnityEngine;

namespace HeavyMetalMachines.AI
{
	public class UnityAIScheduler : BaseMonoBehaviour, IAIScheduler, ICleanupListener
	{
		public void AddTask(IAITask task)
		{
			this._tasks.Add(task);
		}

		public void RemoveTask(IAITask task)
		{
			this._tasks.Remove(task);
		}

		public void OnCleanup(CleanupMessage msg)
		{
			this._tasks.Clear();
		}

		private void Update()
		{
			for (int i = 0; i < this._tasks.Count; i++)
			{
				this._tasks[i].Update(Time.deltaTime);
			}
		}

		private readonly List<IAITask> _tasks = new List<IAITask>();
	}
}
