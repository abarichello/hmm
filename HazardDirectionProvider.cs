using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines
{
	public class HazardDirectionProvider
	{
		public HazardDirectionProvider(HazardDirectionProvider.EDirection directionType, Transform hazard, Transform transform)
		{
			this._directionType = directionType;
			this._hazardPos = hazard.position;
			switch (directionType)
			{
			case HazardDirectionProvider.EDirection.None:
			case HazardDirectionProvider.EDirection.TargetPositionDiff:
			case HazardDirectionProvider.EDirection.TargetDirection:
			case HazardDirectionProvider.EDirection.CollisionNormal:
				this._staticDirection = Vector3.zero;
				break;
			case HazardDirectionProvider.EDirection.TransformPositionDiff:
				this._staticDirection = (transform.position - hazard.position).normalized;
				break;
			case HazardDirectionProvider.EDirection.TransformForward:
				this._staticDirection = transform.forward;
				break;
			case HazardDirectionProvider.EDirection.HazardForward:
				this._staticDirection = hazard.forward;
				break;
			case HazardDirectionProvider.EDirection.HazardBackward:
				this._staticDirection = -hazard.forward;
				break;
			case HazardDirectionProvider.EDirection.HazardRight:
				this._staticDirection = hazard.right;
				break;
			case HazardDirectionProvider.EDirection.HazardLeft:
				this._staticDirection = -hazard.right;
				break;
			default:
				HazardDirectionProvider.Log.ErrorFormat("EDirection not implemented: {0}", new object[]
				{
					directionType
				});
				break;
			}
		}

		public Vector3 GetUpdatedDirection(bool invertDirectionIfOppositeVelocity, Vector3 targetPos, Vector3 targetVelocity, Vector3 collisionNormal = default(Vector3))
		{
			Vector3 vector;
			switch (this._directionType)
			{
			case HazardDirectionProvider.EDirection.TargetPositionDiff:
				vector = (targetPos - this._hazardPos).normalized;
				goto IL_63;
			case HazardDirectionProvider.EDirection.TargetDirection:
				vector = targetVelocity.normalized;
				goto IL_63;
			case HazardDirectionProvider.EDirection.CollisionNormal:
				vector = collisionNormal;
				goto IL_63;
			}
			vector = this._staticDirection;
			IL_63:
			if (invertDirectionIfOppositeVelocity && Vector3.Dot(vector, targetVelocity) < 0f)
			{
				return -vector;
			}
			return vector;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(HazardDirectionProvider));

		private Vector3 _staticDirection;

		private Vector3 _hazardPos;

		private HazardDirectionProvider.EDirection _directionType;

		public enum EDirection
		{
			None,
			TargetPositionDiff,
			TransformPositionDiff,
			TransformForward,
			TargetDirection,
			CollisionNormal,
			HazardForward,
			HazardBackward,
			HazardRight,
			HazardLeft
		}
	}
}
