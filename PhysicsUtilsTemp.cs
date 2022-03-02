using System;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines
{
	public static class PhysicsUtilsTemp
	{
		internal static Vector3 GetValidPosition(Vector3 from, Vector3 to, bool isCarryingBomb, TeamKind teamKind, float radius)
		{
			float magnitude = (to - from).magnitude;
			Vector3 vector = (to - from) / magnitude;
			int num = 512;
			if (isCarryingBomb)
			{
				num |= 524288;
			}
			if (teamKind != TeamKind.Red)
			{
				if (teamKind == TeamKind.Blue)
				{
					num |= 16777216;
				}
			}
			else
			{
				num |= 4194304;
			}
			RaycastHit raycastHit;
			bool flag = Physics.Raycast(from, vector, ref raycastHit, magnitude, num);
			if (flag)
			{
				return from + vector * (raycastHit.distance - radius);
			}
			bool flag2 = Physics.SphereCast(from, radius, vector, ref raycastHit, magnitude, num);
			if (!flag2)
			{
				return to;
			}
			float num2 = Vector3.Dot(raycastHit.point - from, vector);
			Vector3 vector2 = from + vector * num2;
			float magnitude2 = (raycastHit.point - vector2).magnitude;
			if (Mathf.Approximately(magnitude2, 0f))
			{
				return from + vector * (num2 - radius);
			}
			if (magnitude2 < radius)
			{
				float num3 = Mathf.Sqrt(radius * radius - magnitude2 * magnitude2);
				return from + vector * (num2 - num3);
			}
			return vector2;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(PhysicsUtilsTemp));
	}
}
