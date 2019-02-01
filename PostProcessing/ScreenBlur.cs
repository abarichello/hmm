using System;
using UnityEngine;

namespace HeavyMetalMachines.PostProcessing
{
	public struct ScreenBlur
	{
		[Range(0f, 1f)]
		public float Strength;

		[Range(1f, 4f)]
		public int Iterations;
	}
}
