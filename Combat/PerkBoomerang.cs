using System;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkBoomerang : PerkStraightMovement
	{
		public override void PerkInitialized()
		{
			this.goingBack = false;
			base.PerkInitialized();
		}

		public override Vector3 UpdatePosition()
		{
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
			Vector3 position2 = base._trans.position;
			this._direction = (position - position2).normalized;
			this._speed += this._accel * Time.deltaTime;
			Vector3 vector = this._direction * this._speed;
			Vector3 vector2 = position2 + vector * Time.deltaTime;
			this._end = position;
			moved = Vector3.SqrMagnitude(vector2 - this._origin);
			vector2.y = position2.y;
			this._origin = vector2;
			return vector2;
		}

		private bool goingBack;
	}
}
