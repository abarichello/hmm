using System;
using UnityEngine;

namespace HeavyMetalMachines.Extensions
{
	public static class TransformExtensions
	{
		public static void SetLocalScaleWidth(this Transform transform, float width)
		{
			Vector3 localScale = transform.localScale;
			localScale.x = width;
			transform.localScale = localScale;
		}
	}
}
