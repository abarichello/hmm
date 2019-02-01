using System;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	[RequireComponent(typeof(Rigidbody))]
	public class PerkBoomerang : PerkStraightMovement
	{
		public override void PerkInitialized()
		{
			this.goingBack = false;
			base.PerkInitialized();
			this._t = 0f;
		}

		public override Vector3 UpdatePosition()
		{
			this._t += Time.deltaTime;
			if (!this.goingBack && base._deltaTime >= base._endDeltaTime)
			{
				this.goingBack = true;
				float endDeltaTime = base._endDeltaTime;
				this._speed += this._accel * endDeltaTime;
				if (this._velocityType != PerkStraightMovement.MovementType.Constant)
				{
					this._speed = -this._speed;
					this._accel = this.Effect.Data.MoveSpeed / this._duration;
				}
				this._origin = base._trans.position;
			}
			if (!this.goingBack)
			{
				return base.UpdatePosition();
			}
			Vector3 origin = this._origin;
			float num;
			Vector3 vector = this.CalcNewPosition(out num);
			bool flag = PhysicsUtils.SphereIntersect(origin, vector, this.Effect.Data.SourceCombat.transform.position, 3f);
			if (flag)
			{
				this.Effect.TriggerDestroy(-1, vector, false, null, Vector3.zero, BaseFX.EDestroyReason.Default, false);
				vector = origin;
			}
			return vector;
		}

		protected override Vector3 CalcNewPosition(out float moved)
		{
			if (!this.goingBack)
			{
				return base.CalcNewPosition(out moved);
			}
			Vector3 position = this.Effect.Data.SourceCombat.transform.position;
			this._direction = (position - base._trans.position).normalized;
			this._speed += this._accel * Time.deltaTime;
			Vector3 a = this._direction * this._speed;
			Vector3 vector = base._trans.position + a * Time.deltaTime;
			this._end = position;
			moved = Vector3.SqrMagnitude(vector - this._origin);
			vector.y = base._trans.position.y;
			this._origin = vector;
			return vector;
		}

		private bool goingBack;

		private float _t;
	}
}
