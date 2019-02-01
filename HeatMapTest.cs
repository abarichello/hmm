using System;
using System.Collections;
using UnityEngine;

namespace HeavyMetalMachines
{
	internal class HeatMapTest : MonoBehaviour
	{
		private void Start()
		{
			this.heatMap = new HeatMap("HeatMapTest", 100f, "127.0.0.1", 1298);
			base.StartCoroutine(this.SendDeaths());
		}

		private IEnumerator SendDeaths()
		{
			for (int i = 0; i < 2000; i++)
			{
				this.heatMap.RegisterEvent(HeatMap.EventType.Death, new Vector3((float)UnityEngine.Random.Range(0, 100), 0f, (float)UnityEngine.Random.Range(0, 100)));
				yield return this.waitDotOneSeconds;
			}
			yield break;
		}

		public HeatMap heatMap;

		private WaitForSeconds waitDotOneSeconds = new WaitForSeconds(0.1f);
	}
}
