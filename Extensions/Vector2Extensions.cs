using System;
using Hoplon.Math;
using UnityEngine;

namespace HeavyMetalMachines.Extensions
{
	public static class Vector2Extensions
	{
		public static Vector2 ToUnityVector2(this Vector2 v)
		{
			return new Vector2(v.x, v.y);
		}

		public static Vector2 ToHmmVector2(this Vector2 v)
		{
			Vector2 result = default(Vector2);
			result.x = v.x;
			result.y = v.y;
			return result;
		}
	}
}
