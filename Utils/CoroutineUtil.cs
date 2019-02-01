using System;
using System.Collections;
using UnityEngine;

namespace HeavyMetalMachines.Utils
{
	public static class CoroutineUtil
	{
		public static IEnumerator WaitForRealSeconds(float time)
		{
			float start = Time.realtimeSinceStartup;
			while (Time.realtimeSinceStartup < start + time)
			{
				yield return null;
			}
			yield break;
		}
	}
}
