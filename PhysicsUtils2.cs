using System;
using Pocketverse.Util;
using UnityEngine;

namespace HeavyMetalMachines
{
	internal static class PhysicsUtils2
	{
		public static PhysicsUtils2.SimpleHit GetHit(Vector3 position, Collider other)
		{
			SphereCollider sphereCollider = other as SphereCollider;
			BoxCollider boxCollider = other as BoxCollider;
			CapsuleCollider capsuleCollider = other as CapsuleCollider;
			if (sphereCollider != null)
			{
				return PhysicsUtils2.GetHitWithSphere(position, sphereCollider);
			}
			if (boxCollider != null)
			{
				return PhysicsUtils2.GetHitWithBox(position, boxCollider);
			}
			if (capsuleCollider != null)
			{
				return PhysicsUtils2.GetHitWithCapsule(position, capsuleCollider);
			}
			throw new NotImplementedException(string.Concat(new object[]
			{
				"Collision with ",
				other.name,
				" not implemented. Type:",
				other.GetType()
			}));
		}

		private static PhysicsUtils2.SimpleHit GetHitWithSphere(Vector3 myPos, SphereCollider other)
		{
			Transform transform = other.transform;
			Vector3 position = transform.position;
			float num = transform.lossyScale.MaxComponent();
			PhysicsUtils2.SimpleHit result;
			result.normal = (myPos - position).normalized;
			result.point = position + result.normal * (other.radius * num);
			return result;
		}

		private static PhysicsUtils2.SimpleHit GetHitWithBox(Vector3 myPos, BoxCollider box)
		{
			Transform transform = box.transform;
			Vector3 vector = transform.worldToLocalMatrix.MultiplyPoint(myPos);
			Vector3 zero = Vector3.zero;
			Vector3 zero2 = Vector3.zero;
			Vector3 size = box.size;
			zero2.Set(size[0] * 0.5f, size[1] * 0.5f, size[2] * 0.5f);
			Vector3 center = box.center;
			Vector3 zero3 = Vector3.zero;
			for (int i = 0; i < 3; i++)
			{
				if (i != 1)
				{
					if (vector[i] >= center[i] + zero2[i])
					{
						zero[i] = center[i] + zero2[i];
						zero3[i] = 1f;
					}
					else if (vector[i] <= center[i] - zero2[i])
					{
						zero[i] = center[i] - zero2[i];
						zero3[i] = -1f;
					}
					else
					{
						zero[i] = vector[i];
						zero3[i] = 0f;
					}
				}
			}
			zero3.y = 0f;
			PhysicsUtils2.SimpleHit result;
			result.normal = transform.TransformDirection(zero3).normalized;
			result.point = transform.localToWorldMatrix.MultiplyPoint(zero);
			return result;
		}

		private static PhysicsUtils2.SimpleHit GetHitWithCapsule(Vector3 myPos, CapsuleCollider other)
		{
			if (other.direction != 2)
			{
				throw new Exception(string.Format("PhysicsUtils2 only works with capsule in Z-Axis direction!!! Fix CapsuleCollider:{0}", other.name));
			}
			Transform transform = other.transform;
			Vector3 lossyScale = transform.lossyScale;
			if (Math.Abs(lossyScale.x - lossyScale.y) >= 1E-05f || Math.Abs(lossyScale.y - lossyScale.z) >= 1E-05f)
			{
				throw new Exception(string.Format("PhysicsUtils2 only works with same scale on (X1: {0} ; Y1: {1} ; Z1: {2}) for capsules!!! Fix CapsuleCollider:{3}", new object[]
				{
					lossyScale.x,
					lossyScale.y,
					lossyScale.z,
					other.name
				}));
			}
			Vector3 vector = transform.worldToLocalMatrix.MultiplyPoint(myPos);
			Vector3 zero = Vector3.zero;
			Vector3 zero2 = Vector3.zero;
			zero2.Set(other.radius, 0f, other.height * 0.5f);
			Vector3 center = other.center;
			Vector3 zero3 = Vector3.zero;
			for (int i = 0; i < 3; i++)
			{
				if (i != 1)
				{
					if (vector[i] >= center[i] + zero2[i])
					{
						zero[i] = center[i] + zero2[i];
						zero3[i] = 1f;
					}
					else if (vector[i] <= center[i] - zero2[i])
					{
						zero[i] = center[i] - zero2[i];
						zero3[i] = -1f;
					}
					else
					{
						zero[i] = vector[i];
						zero3[i] = 0f;
					}
				}
			}
			zero3.y = 0f;
			PhysicsUtils2.SimpleHit result;
			result.normal = transform.TransformDirection(zero3).normalized;
			result.point = transform.localToWorldMatrix.MultiplyPoint(zero);
			return result;
		}

		public struct SimpleHit
		{
			public Vector3 point;

			public Vector3 normal;
		}
	}
}
