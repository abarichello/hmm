using System;
using UnityEngine;

namespace HeavyMetalMachines.Fog
{
	internal static class QuaternionExtension
	{
		public static void Normalize(this Quaternion q)
		{
			float num = 1f / Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
			q.x *= num;
			q.y *= num;
			q.z *= num;
			q.w *= num;
		}
	}
}
