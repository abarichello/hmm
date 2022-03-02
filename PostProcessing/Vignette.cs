using System;
using UnityEngine;

namespace HeavyMetalMachines.PostProcessing
{
	[Serializable]
	public struct Vignette
	{
		[Range(0.15f, 6f)]
		public float Ratio;

		[Range(0f, 5f)]
		public float Radius;

		[Range(2f, 16f)]
		public float Slope;

		[Range(-10f, 0f)]
		public float Amount;
	}
}
