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
			else
			{
				shooterVelocity = Vector3.zero;
			}
			Vector3 targetVelocity;
			if (target.IsPlayer)
			{
				targetVelocity = target.GetComponent<Rigidbody>().velocity;
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
			float num = CombatUtils.FirstOrderInterceptTime(shotSpeed, targetRelativePosition, vector);
			return targetPosition + num * vector;
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
				float num2 = -targetRelativePosition.sqrMagnitude / (2f * Vector3.Dot(targetRelativeVelocity, targetRelativePosition));
				return Mathf.Max(num2, 0f);
			}
			float num3 = 2f * Vector3.Dot(targetRelativeVelocity, targetRelativePosition);
			float sqrMagnitude2 = targetRelativePosition.sqrMagnitude;
			float num4 = num3 * num3 - 4f * num * sqrMagnitude2;
			if (num4 > 0f)
			{
				float num5 = (-num3 + Mathf.Sqrt(num4)) / (2f * num);
				float num6 = (-num3 - Mathf.Sqrt(num4)) / (2f * num);
				if (num5 <= 0f)
				{
					return Mathf.Max(num6, 0f);
				}
				if (num6 > 0f)
				{
					return Mathf.Min(num5, num6);
				}
				return num5;
			}
			else
			{
				if (num4 < 0f)
				{
					return 0f;
				}
				return Mathf.Max(-num3 / (2f * num), 0f);
			}
		}
	}
}
