using System;
using UnityEngine;

namespace HeavyMetalMachines.PostProcessing
{
	public struct Exposure
	{
		[Range(0f, 20f)]
		public float Strength;

		[Range(-20f, 20f)]
		public float Shift;

		[Range(0.001f, 10f)]
		public float Gamma;
	}
}
