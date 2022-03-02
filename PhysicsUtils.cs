using System;
using Pocketverse;
using Pocketverse.Util;
using UnityEngine;

namespace HeavyMetalMachines
{
	public static class PhysicsUtils
	{
		private static void GetColliderInfo(Collider me, ref float myRadius, ref Vector3 stColliderSize, ref Matrix4x4 stWorldToLocalMatrix, ref bool boIsSphere)
		{
			BoxCollider boxCollider = me as BoxCollider;
			if (boxCollider != null)
			{
				Vector3 lossyScale = me.transform.lossyScale;
				float num = boxCollider.size.x * lossyScale.x * 0.5f;
				float num2 = boxCollider.size.z * lossyScale.z * 0.5f;
				myRadius = Mathf.Sqrt(num * num + num2 * num2);
				stColliderSize = boxCollider.size;
				stWorldToLocalMatrix = boxCollider.transform.worldToLocalMatrix;
			}
			SphereCollider sphereCollider = me as SphereCollider;
			if (sphereCollider != null)
			{
				float num3 = me.transform.lossyScale.MaxComponent();
				myRadius = sphereCollider.radius * num3;
				boIsSphere = true;
			}
		}

		public static float GetRadius(Collider collider)
		{
			float result = -1f;
			Vector3 zero = Vector3.zero;
			bool flag = false;
			Matrix4x4 identity = Matrix4x4.identity;
			PhysicsUtils.GetColliderInfo(collider, ref result, ref zero, ref identity, ref flag);
			return result;
		}

		public static float GetInternalRadius(Collider collider)
		{
			BoxCollider boxCollider = collider as BoxCollider;
			if (boxCollider != null)
			{
				Vector3 lossyScale = collider.transform.lossyScale;
				float num = boxCollider.size.x * lossyScale.x * 0.5f;
				float num2 = boxCollider.size.z * lossyScale.z * 0.5f;
				return Mathf.Min(num, num2);
			}
			return PhysicsUtils.GetRadius(collider);
		}

		public static bool GetFirstHit(Vector3 from, Vector3 to, float radius, out RaycastHit info, int raycastLayer)
		{
			Vector3 vector = to - from;
			return Physics.SphereCast(from, radius, vector.normalized, ref info, vector.magnitude, raycastLayer);
		}

		public static bool IsFacing(Vector3 forward, Vector3 normal)
		{
			float num = Vector3.Dot(-forward, normal);
			return num >= 0f;
		}

		public static bool IsInFront(Vector3 myCenter, Vector3 mStEdgeNormal, Vector3 p1)
		{
			Plane plane;
			plane..ctor(mStEdgeNormal, myCenter);
			return plane.GetSide(p1);
		}

		public static bool IsInsideAngle(Transform srcTransform, Transform targetTransform, float angle)
		{
			if (targetTransform.GetComponent<Collider>() == null)
			{
				PhysicsUtils.Log.ErrorFormat("Trying to check if inside angle with null collider! target:{0}", new object[]
				{
					targetTransform.name
				});
				return false;
			}
			return PhysicsUtils.IsInsideAngle(srcTransform.position, srcTransform.forward, targetTransform.GetComponent<Collider>(), angle);
		}

		public static bool IsInsideAngle(Vector3 srcPos, Vector3 srcForward, Collider targetCollider, float angle)
		{
			return PhysicsUtils.IsInsideAngleAndRange(srcPos, srcForward, targetCollider, angle, 0f);
		}

		public static bool IsInsideAngleAndRange(Vector3 srcPos, Vector3 srcForward, Collider targetCollider, float angle, float range)
		{
			Vector3 vector = HMMMathUtils.CalcDirectionXZ(srcPos, targetCollider.transform.position);
			float num = Vector3.Dot(srcForward, vector);
			Vector3 vector2 = Quaternion.Euler(0f, angle * 0.5f, 0f) * srcForward;
			Vector3 vector3 = Quaternion.Euler(0f, -angle * 0.5f, 0f) * srcForward;
			float num2 = Mathf.Cos(angle * 0.017453292f * 0.5f);
			if (num <= num2)
			{
				Plane plane;
				plane..ctor(srcPos, srcPos + vector2, srcPos + Vector3.up * 10f);
				Plane plane2;
				plane2..ctor(srcPos, srcPos + vector3, srcPos + Vector3.up * 10f);
				int num3 = targetCollider.PlaneCast(plane);
				int num4 = targetCollider.PlaneCast(plane2);
				return (num3 == 1 || num3 == 0) && (num4 == -1 || num4 == 0);
			}
			if (range <= 0f)
			{
				return true;
			}
			Ray ray;
			ray..ctor(srcPos, vector);
			RaycastHit raycastHit;
			return targetCollider.Raycast(ray, ref raycastHit, range);
		}

		public static RaycastHit2D CircleCast(Vector3 origin, float radius, Vector3 direction, float distance = float.PositiveInfinity, int layerMask = -5)
		{
			Vector2 vector;
			vector.x = origin.x;
			vector.y = origin.z;
			Vector2 vector2;
			vector2.x = direction.x;
			vector2.y = direction.z;
			return Physics2D.CircleCast(vector, radius, vector2, distance, layerMask);
		}

		public static RaycastHit2D Raycast(Vector3 origin, Vector3 direction, float distance = float.PositiveInfinity, int layerMask = -5)
		{
			Vector2 vector;
			vector.x = origin.x;
			vector.y = origin.z;
			Vector2 vector2;
			vector2.x = direction.x;
			vector2.y = direction.z;
			return Physics2D.Raycast(vector, vector2, distance, layerMask);
		}

		public static bool SphereIntersect(Vector3 i, Vector3 j, Vector3 center, float radius)
		{
			float num = radius * radius;
			Vector3 vector = center - i;
			if (vector.sqrMagnitude < num)
			{
				return true;
			}
			if ((center - j).sqrMagnitude < num)
			{
				return true;
			}
			Vector3 vector2 = j - i;
			float num2 = Vector3.Dot(vector, vector2) / Vector3.Dot(vector2, vector2);
			if (num2 > 1f || num2 < 0f)
			{
				return false;
			}
			Vector3 vector3 = num2 * vector2;
			return (vector - vector3).sqrMagnitude < num;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(PhysicsUtils));

		private static Vector3 _normal;
	}
}
