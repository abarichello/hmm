using System;
using UnityEngine;

namespace HeavyMetalMachines.PostProcessing
{
	public struct ScaledOverlayImage
	{
		[Range(0.0001f, 1f)]
		public float VerticalScale;

		[Range(0.0001f, 1f)]
		public float HorizontalScale;

		[Range(-20f, 20f)]
		public float Saturation;
	}
}
