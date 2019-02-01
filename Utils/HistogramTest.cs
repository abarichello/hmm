using System;
using UnityEngine;

namespace HeavyMetalMachines.Utils
{
	internal class HistogramTest : MonoBehaviour
	{
		private void Start()
		{
			this.histogramWindow = HistogramUtility.CreateHistogramWindow("Teste", 200f, 200f, 50, new string[]
			{
				"Test Layer"
			});
		}

		private void Update()
		{
			this.histogramWindow.AddSample(0, (float)((!Input.GetKey(KeyCode.A)) ? UnityEngine.Random.Range(0, 100) : 0));
		}

		private HistogramUtility.HistogramWindow histogramWindow;
	}
}
