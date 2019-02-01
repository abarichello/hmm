using System;
using UnityEngine;

namespace HeavyMetalMachines.Utils
{
	public static class ReallyFastMath
	{
		public static void FastNormalize(ref Vector3 v0)
		{
			float num = 1f / v0.magnitude;
			v0.x *= num;
			v0.y *= num;
			v0.z *= num;
		}

		public static void FastCross(ref Vector3 v0, ref Vector3 v1, ref Vector3 dst)
		{
			dst.x = v0.y * v1.z - v0.z * v1.y;
			dst.y = v0.z * v1.x - v0.x * v1.z;
			dst.z = v0.x * v1.y - v0.y * v1.x;
		}
	}
}
