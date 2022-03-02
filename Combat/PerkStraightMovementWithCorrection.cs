using System;
using System.Collections.Generic;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkStraightMovementWithCorrection : BasePerk, IPerkMovement
	{
		public override void PerkInitialized()
		{
			if (!GameHubBehaviour.Hub.Net || GameHubBehaviour.Hub.Net.IsClient())
			{
				base.enabled = false;
				return;
			}
			this._searchTimedUpdater = new TimedUpdater(this.SearchMillis, false, false);
			this._lastMillis = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			this._moveSpeed = this.Effect.Data.MoveSpeed;
			this._initialForward = Vector3.Scale(this.Effect.Data.Target - base._trans.position, new Vector3(1f, 0f, 1f)).normalized;
			this._initialRight = Vector3.Cross(this._initialForward, Vector3.up);
		}

		private Collider[] GetHits()
		{
			if (this.SearchRange == 0f)
			{
				return Physics.OverlapSphere(base._trans.position, this.Effect.Data.Range, 1077054464);
			}
			return Physics.OverlapSphere(base._trans.position, this.SearchRange, 1077054464);
		}

		private void SearchTarget()
		{
			Vector3 position = base._trans.position;
			Vector3 vector = position + this._initialRight;
			Vector4 vector2 = vector + Vector3.up;
			Plane plane;
			plane..ctor(position, vector, vector2);
			Collider[] hits = this.GetHits();
			Transform transform = null;
			float num = 0f;
			for (int i = 0; i < hits.Length; i++)
			{
				CombatObject combat = CombatRef.GetCombat(hits[i]);
				if (!PerkStraightMovementWithCorrection._objects.Contains(combat))
				{
					PerkStraightMovementWithCorrection._objects.Add(combat);
					if (combat)
					{
						if (this.Effect.CheckHit(combat))
						{
							if (this.IgnorePlaneCast || plane.GetSide(combat.Transform.position) == this.PositiveSide)
							{
								if (this.BombPriority && GameHubBehaviour.Hub.BombManager.IsCarryingBomb(combat.Id.ObjId))
								{
									transform = combat.Transform;
									break;
								}
								float num2 = Vector3.SqrMagnitude(Vector3.Scale(combat.Transform.position - base._trans.position, new Vector3(1f, 0f, 1f)));
								if (transform == null || num2 < num)
								{
									num = num2;
									transform = combat.Transform;
								}
							}
						}
					}
				}
			}
			PerkStraightMovementWithCorrection._objects.Clear();
			this._targetTransform = transform;
		}

		private void CheckTarget()
		{
			if (!this._targetTransform)
			{
				return;
			}
			if (this.IgnorePlaneCast)
			{
				return;
			}
			Vector3 position = base._trans.position;
			Vector3 vector = position + this._initialRight;
			Vector4 vector2 = vector + Vector3.up;
			Plane plane;
			plane..ctor(position, vector, vector2);
			if (plane.GetSide(this._targetTransform.position) != this.PositiveSide)
			{
				this._targetTransform = null;
			}
		}

		public virtual Vector3 UpdatePosition()
		{
			if (this._searchTimedUpdater.ShouldHalt())
			{
				this.CheckTarget();
			}
			else
			{
				this.SearchTarget();
			}
			float num = (float)((long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime() - this._lastMillis) * 0.001f;
			this._lastMillis = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			Vector3 forward = base._trans.forward;
			if (this._targetTransform)
			{
				Quaternion quaternion = Quaternion.LookRotation(forward, Vector3.up);
				Quaternion quaternion2 = Quaternion.LookRotation(this._targetTransform.position - base._trans.position);
				base._trans.rotation = Quaternion.RotateTowards(quaternion, quaternion2, this.MaxDegreesPerSecond * num);
			}
			else if (!this.IgnorePlaneCast)
			{
				base._trans.rotation = Quaternion.LookRotation(this._initialForward, Vector3.up);
			}
			Vector3 vector;
			vector..ctor(forward.x, 0f, forward.z);
			vector.Normalize();
			this.Effect.Data.Direction = vector;
			return base._trans.position + this._moveSpeed * vector * num;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PerkStraightMovementWithCorrection));

		public int SearchMillis = 200;

		public float SearchRange;

		public float MaxDegreesPerSecond = 10f;

		public bool PositiveSide = true;

		public bool IgnorePlaneCast;

		public bool BombPriority;

		private Vector3 _initialForward;

		private Vector3 _initialRight;

		private Transform _targetTransform;

		private TimedUpdater _searchTimedUpdater;

		private float _moveSpeed;

		private long _lastMillis;

		private static readonly HashSet<CombatObject> _objects = new HashSet<CombatObject>();
	}
}
