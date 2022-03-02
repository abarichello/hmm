using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines
{
	public class FramerateLog : MonoBehaviour
	{
		private void Update()
		{
			float num = (Time.realtimeSinceStartup - this._lastRealTime) * Time.timeScale;
			float num2 = Time.deltaTime + 0.0222f * Time.timeScale;
			if (num > num2 && Application.isPlaying && GameHubBehaviour.Hub)
			{
				FramerateLog.Log.WarnFormat("Freezed more than 22ms ...scaledRealDelta:{0}ms acceptedScaledDelta:{1}ms delta:{2}ms scale:{3}", new object[]
				{
					num * 1000f,
					num2 * 1000f,
					Time.deltaTime,
					Time.timeScale
				});
			}
			this._lastRealTime = Time.realtimeSinceStartup;
			long totalMemory = GC.GetTotalMemory(false);
			if (totalMemory < this._lastMemory)
			{
				FramerateLog.Log.InfoFormat("TotalMemory Lowered Current={0} Last={1}", new object[]
				{
					totalMemory,
					this._lastMemory
				});
			}
			this._lastMemory = totalMemory;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(FramerateLog));

		[NonSerialized]
		private float _lastRealTime;

		[NonSerialized]
		private long _lastMemory;
	}
}
