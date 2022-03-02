using System;
using UnityEngine;

namespace HeavyMetalMachines.Presenting.Unity
{
	public static class ColorExtensions
	{
		public static Color ChangeAlpha(this Color color, float alpha)
		{
			color.a = alpha;
			return color;
		}

		public static Color ToHmmColor(this Color unityColor)
		{
			Color result = default(Color);
			result.R = unityColor.r;
			result.G = unityColor.g;
			result.B = unityColor.b;
			result.A = unityColor.a;
			return result;
		}

		public static Color ToUnityColor(this Color hmmColor)
		{
			Color result = default(Color);
			result.r = hmmColor.R;
			result.g = hmmColor.G;
			result.b = hmmColor.B;
			result.a = hmmColor.A;
			return result;
		}
	}
}
