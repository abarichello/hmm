using System;
using UnityEngine;

namespace HeavyMetalMachines
{
	internal class HMMMathUtils
	{
		public static bool BoxIntersectsLineSegment(Vector3 start, Vector3 end, Matrix4x4 stInverseColliderTransform, Vector3 stColliderSize)
		{
			Vector3 b = stInverseColliderTransform.MultiplyPoint(start);
			b.y = 0f;
			Vector3 vector = stInverseColliderTransform.MultiplyPoint((start + end) / 2f);
			vector.y = 0f;
			Vector3 a = vector - b;
			float magnitude = a.magnitude;
			Vector3 l = a / magnitude;
			return HMMMathUtils.AABB_LineSegmentOverlap(l, vector, magnitude, stColliderSize / 2f);
		}

		private static bool AABB_LineSegmentOverlap(Vector3 l, Vector3 mid, float hl, Vector3 stExtents)
		{
			Vector3 vector = -mid;
			if (Mathf.Abs(vector.x) > stExtents.x + hl * Mathf.Abs(l.x))
			{
				return false;
			}
			if (Mathf.Abs(vector.y) > stExtents.y + hl * Mathf.Abs(l.y))
			{
				return false;
			}
			if (Mathf.Abs(vector.z) > stExtents.z + hl * Mathf.Abs(l.z))
			{
				return false;
			}
			float num = stExtents.y * Mathf.Abs(l.z) + stExtents.z * Mathf.Abs(l.y);
			if (Mathf.Abs(vector.y * l.z - vector.z * l.y) > num)
			{
				return false;
			}
			num = stExtents.x * Mathf.Abs(l.z) + stExtents.z * Mathf.Abs(l.x);
			if (Mathf.Abs(vector.z * l.x - vector.x * l.z) > num)
			{
				return false;
			}
			num = stExtents.x * Mathf.Abs(l.y) + stExtents.y * Mathf.Abs(l.x);
			return Mathf.Abs(vector.x * l.y - vector.y * l.x) <= num;
		}

		internal static float DistancePtAndLineSegment(Vector3 v, Vector3 w, Vector3 p)
		{
			float sqrMagnitude = (w - v).sqrMagnitude;
			if ((double)sqrMagnitude == 0.0)
			{
				return (p - v).magnitude;
			}
			float num = Vector3.Dot(p - v, w - v) / sqrMagnitude;
			if ((double)num < 0.0)
			{
				return (p - v).magnitude;
			}
			if ((double)num > 1.0)
			{
				return (p - w).magnitude;
			}
			Vector3 b = v + num * (w - v);
			return (p - b).magnitude;
		}

		internal static Vector3 ProjectPtToLineSegment(Vector3 v, Vector3 w, Vector3 p)
		{
			float num = 0f;
			return HMMMathUtils.ProjectPtToLineSegment(v, w, p, ref num);
		}

		internal static Vector3 ProjectPtToLineSegment(Vector3 v, Vector3 w, Vector3 p, ref float t)
		{
			float sqrMagnitude = (w - v).sqrMagnitude;
			if ((double)sqrMagnitude == 0.0)
			{
				return v;
			}
			t = Vector3.Dot(p - v, w - v) / sqrMagnitude;
			if ((double)t < 0.0)
			{
				return v;
			}
			if ((double)t > 1.0)
			{
				return w;
			}
			return v + t * (w - v);
		}

		internal static float DistancePointLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
		{
			return Vector3.Magnitude(HMMMathUtils.ProjectPtToLine(point, lineStart, lineEnd) - point);
		}

		internal static Vector3 ProjectPtToLine(Vector3 p, Vector3 a, Vector3 b)
		{
			float num = Vector3.Dot(p - a, b - a);
			float num2 = Vector3.Dot(b - a, b - a);
			return a + num / num2 * (b - a);
		}

		private bool OBBOverlap(Vector3 a, Vector3 Pa, Vector3[] A, Vector3 b, Vector3 Pb, Vector3[] B)
		{
			Vector3 lhs = Pb - Pa;
			Vector3 vector = new Vector3(Vector3.Dot(lhs, A[0]), Vector3.Dot(lhs, A[1]), Vector3.Dot(lhs, A[2]));
			Vector3[] array = new Vector3[3];
			for (int i = 0; i < 3; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					array[i][j] = Vector3.Dot(A[i], B[j]);
				}
			}
			float num;
			float num2;
			float num3;
			for (int i = 0; i < 3; i++)
			{
				num = a[i];
				num2 = b[0] * Mathf.Abs(array[i][0]) + b[1] * Mathf.Abs(array[i][1]) + b[2] * Mathf.Abs(array[i][2]);
				num3 = Mathf.Abs(vector[i]);
				if (num3 > num + num2)
				{
					return false;
				}
			}
			for (int j = 0; j < 3; j++)
			{
				num = a[0] * Mathf.Abs(array[0][j]) + a[1] * Mathf.Abs(array[1][j]) + a[2] * Mathf.Abs(array[2][j]);
				num2 = b[j];
				num3 = Mathf.Abs(vector[0] * array[0][j] + vector[1] * array[1][j] + vector[2] * array[2][j]);
				if (num3 > num + num2)
				{
					return false;
				}
			}
			num = a[1] * Mathf.Abs(array[2][0]) + a[2] * Mathf.Abs(array[1][0]);
			num2 = b[1] * Mathf.Abs(array[0][2]) + b[2] * Mathf.Abs(array[0][1]);
			num3 = Mathf.Abs(vector[2] * array[1][0] - vector[1] * array[2][0]);
			if (num3 > num + num2)
			{
				return false;
			}
			num = a[1] * Mathf.Abs(array[2][1]) + a[2] * Mathf.Abs(array[1][1]);
			num2 = b[0] * Mathf.Abs(array[0][2]) + b[2] * Mathf.Abs(array[0][0]);
			num3 = Mathf.Abs(vector[2] * array[1][1] - vector[1] * array[2][1]);
			if (num3 > num + num2)
			{
				return false;
			}
			num = a[1] * Mathf.Abs(array[2][2]) + a[2] * Mathf.Abs(array[1][2]);
			num2 = b[0] * Mathf.Abs(array[0][1]) + b[1] * Mathf.Abs(array[0][0]);
			num3 = Mathf.Abs(vector[2] * array[1][2] - vector[1] * array[2][2]);
			if (num3 > num + num2)
			{
				return false;
			}
			num = a[0] * Mathf.Abs(array[2][0]) + a[2] * Mathf.Abs(array[0][0]);
			num2 = b[1] * Mathf.Abs(array[1][2]) + b[2] * Mathf.Abs(array[1][1]);
			num3 = Mathf.Abs(vector[0] * array[2][0] - vector[2] * array[0][0]);
			if (num3 > num + num2)
			{
				return false;
			}
			num = a[0] * Mathf.Abs(array[2][1]) + a[2] * Mathf.Abs(array[0][1]);
			num2 = b[0] * Mathf.Abs(array[1][2]) + b[2] * Mathf.Abs(array[1][0]);
			num3 = Mathf.Abs(vector[0] * array[2][1] - vector[2] * array[0][1]);
			if (num3 > num + num2)
			{
				return false;
			}
			num = a[0] * Mathf.Abs(array[2][2]) + a[2] * Mathf.Abs(array[0][2]);
			num2 = b[0] * Mathf.Abs(array[1][1]) + b[1] * Mathf.Abs(array[1][0]);
			num3 = Mathf.Abs(vector[0] * array[2][2] - vector[2] * array[0][2]);
			if (num3 > num + num2)
			{
				return false;
			}
			num = a[0] * Mathf.Abs(array[1][0]) + a[1] * Mathf.Abs(array[0][0]);
			num2 = b[1] * Mathf.Abs(array[2][2]) + b[2] * Mathf.Abs(array[2][1]);
			num3 = Mathf.Abs(vector[1] * array[0][0] - vector[0] * array[1][0]);
			if (num3 > num + num2)
			{
				return false;
			}
			num = a[0] * Mathf.Abs(array[1][1]) + a[1] * Mathf.Abs(array[0][1]);
			num2 = b[0] * Mathf.Abs(array[2][2]) + b[2] * Mathf.Abs(array[2][0]);
			num3 = Mathf.Abs(vector[1] * array[0][1] - vector[0] * array[1][1]);
			if (num3 > num + num2)
			{
				return false;
			}
			num = a[0] * Mathf.Abs(array[1][2]) + a[1] * Mathf.Abs(array[0][2]);
			num2 = b[0] * Mathf.Abs(array[2][1]) + b[1] * Mathf.Abs(array[2][0]);
			num3 = Mathf.Abs(vector[1] * array[0][2] - vector[0] * array[1][2]);
			return num3 <= num + num2;
		}

		public static Vector3 CalcDirectionXZ(Vector3 from, Vector3 to)
		{
			Vector3 vector = to - from;
			vector.y = 0f;
			if (vector.sqrMagnitude == 0f)
			{
				return Vector3.zero;
			}
			return vector.normalized;
		}

		public static bool PolygonContainsPoint(Vector2[] polyPoints, Vector2 p)
		{
			int num = polyPoints.Length - 1;
			bool flag = false;
			int i = 0;
			while (i < polyPoints.Length)
			{
				if (((polyPoints[i].y <= p.y && p.y < polyPoints[num].y) || (polyPoints[num].y <= p.y && p.y < polyPoints[i].y)) && p.x < (polyPoints[num].x - polyPoints[i].x) * (p.y - polyPoints[i].y) / (polyPoints[num].y - polyPoints[i].y) + polyPoints[i].x)
				{
					flag = !flag;
				}
				num = i++;
			}
			return flag;
		}

		internal const float Epsilon = 1E-05f;
	}
}
