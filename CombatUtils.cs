using System;
using HeavyMetalMachines.Combat;
using UnityEngine;

namespace HeavyMetalMachines
{
	public static class CombatUtils
	{
		public static Vector3 GetTargetInterceptPosition(CombatObject shooter, float shotSpeed, CombatObject target)
		{
			Vector3 position = shooter.transform.position;
			Vector3 position2 = target.transform.position;
			Vector3 shooterVelocity;
			if (shooter.IsPlayer)
			{
				shooterVelocity = shooter.GetComponent<Rigidbody>().velocity;
			}
			else if (shooter.IsCreep)
			{
				shooterVelocity = shooter.Creep.Velocity;
			}
			else
			{
				shooterVelocity = Vector3.zero;
			}
			Vector3 targetVelocity;
			if (target.IsPlayer)
			{
				targetVelocity = target.GetComponent<Rigidbody>().velocity;
			}
			else if (target.IsCreep)
			{
				targetVelocity = target.Creep.Velocity;
			}
			else
			{
				targetVelocity = Vector3.zero;
			}
			return CombatUtils.GetTargetInterceptPosition(position, shooterVelocity, shotSpeed, position2, targetVelocity);
		}

		public static Vector3 GetTargetInterceptPosition(Vector3 shooterPosition, float shotSpeed, CombatObject target)
		{
			Vector3 position = target.transform.position;
			Vector3 zero = Vector3.zero;
			Vector3 targetVelocity;
			if (target.IsPlayer)
			{
				targetVelocity = target.GetComponent<Rigidbody>().velocity;
			}
			else if (target.IsCreep)
			{
				targetVelocity = target.Creep.Velocity;
			}
			else
			{
				targetVelocity = Vector3.zero;
			}
			return CombatUtils.GetTargetInterceptPosition(shooterPosition, zero, shotSpeed, position, targetVelocity);
		}

		private static Vector3 GetTargetInterceptPosition(Vector3 shooterPosition, Vector3 shooterVelocity, float shotSpeed, Vector3 targetPosition, Vector3 targetVelocity)
		{
			Vector3 vector = targetVelocity - shooterVelocity;
			Vector3 targetRelativePosition = targetPosition - shooterPosition;
			float d = CombatUtils.FirstOrderInterceptTime(shotSpeed, targetRelativePosition, vector);
			return targetPosition + d * vector;
		}

		private static float FirstOrderInterceptTime(float shotSpeed, Vector3 targetRelativePosition, Vector3 targetRelativeVelocity)
		{
			float sqrMagnitude = targetRelativeVelocity.sqrMagnitude;
			if (sqrMagnitude < 0.001f)
			{
				return 0f;
			}
			float num = sqrMagnitude - shotSpeed * shotSpeed;
			if (Mathf.Abs(num) < 0.001f)
			{
				float a = -targetRelativePosition.sqrMagnitude / (2f * Vector3.Dot(targetRelativeVelocity, targetRelativePosition));
				return Mathf.Max(a, 0f);
			}
			float num2 = 2f * Vector3.Dot(targetRelativeVelocity, targetRelativePosition);
			float sqrMagnitude2 = targetRelativePosition.sqrMagnitude;
			float num3 = num2 * num2 - 4f * num * sqrMagnitude2;
			if (num3 > 0f)
			{
				float num4 = (-num2 + Mathf.Sqrt(num3)) / (2f * num);
				float num5 = (-num2 - Mathf.Sqrt(num3)) / (2f * num);
				if (num4 <= 0f)
				{
					return Mathf.Max(num5, 0f);
				}
				if (num5 > 0f)
				{
					return Mathf.Min(num4, num5);
				}
				return num4;
			}
			else
			{
				if (num3 < 0f)
				{
					return 0f;
				}
				return Mathf.Max(-num2 / (2f * num), 0f);
			}
		}
	}
}
