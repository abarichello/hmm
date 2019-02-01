using System;
using HeavyMetalMachines.Utils;
using UnityEngine;

namespace HeavyMetalMachines.Infra
{
	public class TeamController : MonoBehaviour
	{
		protected void Awake()
		{
			TeamController._timeoutToClearCacheInSec = this.TimeoutToClearCacheInSec;
			TeamController.ResetTimer();
		}

		public static void ResetTimer()
		{
			TeamController._timer = TeamController._timeoutToClearCacheInSec;
		}

		protected void Update()
		{
			TeamController._timer -= Time.deltaTime;
			if (TeamController._timer > 0f)
			{
				return;
			}
			TeamUtils.ClearCache();
			TeamController.ResetTimer();
		}

		public float TimeoutToClearCacheInSec = 30f;

		private static float _timeoutToClearCacheInSec;

		private static float _timer;
	}
}
