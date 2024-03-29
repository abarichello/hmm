﻿using System;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkJumpMovement : PerkStraightMovement
	{
		private Vector3 TargetPosition
		{
			get
			{
				return this.Effect.Data.Target;
			}
		}

		public override void PerkInitialized()
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				Rigidbody component = base._trans.GetComponent<Rigidbody>();
				if (!component)
				{
					PerkJumpMovement.Log.ErrorFormat("This perk requires a RigidBody to work. Effect name: {0}", new object[]
					{
						base.gameObject.name
					});
					return;
				}
				this.Effect.Data.MoveSpeed = component.velocity.magnitude;
				this.Effect.Data.Range = this.Effect.Data.MoveSpeed * this.Effect.Data.LifeTime;
				this.Effect.Data.Target = this.Effect.Data.Origin + this.Effect.Data.Range * this.Effect.Data.Direction;
				this.validTarget = this.Effect.Gadget.GetValidPosition(this.Effect.Data.Origin, this.Effect.Data.Target);
				this._deltaTimeRatioToWallCollision = 1f;
				PerkJumpMovement.Log.DebugFormat("Effect currentVelocity:{0} speed:{1} range:{2} target:{3} validTarget:{4}", new object[]
				{
					component.velocity.magnitude,
					this.Effect.Data.MoveSpeed,
					this.Effect.Data.Range,
					this.Effect.Data.Target,
					this.validTarget
				});
			}
			base.PerkInitialized();
			this.CalcParabola();
			if (this.Effect.Data.CustomVar > 0)
			{
				float num = (float)this.Effect.Data.CustomVar / 255f;
				num *= this.Effect.Data.LifeTime;
				num = this._startTime - num;
				if (num > 0f)
				{
					this._startTime = num;
				}
				this.Effect.Data.EventTime = (int)(this._startTime * 1000f);
			}
		}

		protected override void UpdateAfterVisibilityChange()
		{
			base.UpdateAfterVisibilityChange();
			this.CalcParabola();
			this.UpdatePosition();
		}

		public override Vector3 UpdatePosition()
		{
			Vector3 position = base._trans.position;
			float num;
			Vector3 vector = (!this.UseTargetXZVelocity) ? ((base._deltaTimeRatio > this._deltaTimeRatioToWallCollision) ? this.validTarget : this.CalcNewPosition(out num)) : position;
			float num2 = Mathf.Clamp(base._deltaTimeRatio, 0f, 1f);
			if (this.useCurve)
			{
				vector.y = this._zeroHeight + Mathf.Lerp(Mathf.Lerp(this.Effect.Data.Origin.y - this._zeroHeight, this.TargetPosition.y - this._zeroHeight, num2), this.Effect.Data.EffectInfo.Height, this.animationCurve.Evaluate(num2));
			}
			else
			{
				vector.y = this._zeroHeight + (this._a * num2 * num2 + this._b * num2 + this._c);
			}
			if (float.IsNaN(vector.x) || float.IsNaN(vector.y) || float.IsNaN(vector.z))
			{
				PerkJumpMovement.Log.ErrorFormat("Invalid pos={0} or={1} dir={2} h={3} sp={4} dt={5} a={6} b={7} c={8} _startTime={9} EndTime={10} _trans.position={11}", new object[]
				{
					vector,
					this.Effect.Data.Origin,
					this.Effect.Data.Direction,
					this.Effect.Data.EffectInfo.Height,
					this.Effect.Data.MoveSpeed,
					base._deltaTime,
					this._a,
					this._b,
					this._c,
					this._startTime,
					this._endTime,
					position
				});
				return position;
			}
			return vector;
		}

		private void CalcParabola()
		{
			EffectEvent data = this.Effect.Data;
			this._zeroHeight = Mathf.Max(data.Origin.y, this.TargetPosition.y);
			float num = data.Origin.y - this._zeroHeight;
			float num2 = this.TargetPosition.y - this._zeroHeight;
			this._c = num;
			float num3 = 1f / (4f * (num - data.EffectInfo.Height));
			float num4 = num - num2;
			float num5 = (-1f + Mathf.Sqrt(1f - 4f * num3 * num4)) / (2f * num3);
			float num6 = (-1f - Mathf.Sqrt(1f - 4f * num3 * num4)) / (2f * num3);
			this._b = Mathf.Max(num5, num6);
			this._a = num2 - num - this._b;
		}

		private void OnDrawGizmos()
		{
			EffectEvent data = this.Effect.Data;
			if (data == null)
			{
				return;
			}
			Gizmos.DrawLine(data.Origin, data.Target);
		}

		public override void PerkDestroyed(DestroyEffectMessage destroyEffectMessage)
		{
			if (this.Effect.Attached != null)
			{
				if (GameHubBehaviour.Hub.Net.IsServer())
				{
					Rigidbody component = this.Effect.Attached.GetComponent<Rigidbody>();
					this.Effect.Attached.Transform.position = new Vector3(component.position.x, 0f, component.position.z);
				}
				else if (GameHubBehaviour.Hub.Net.IsClient())
				{
					Transform transform = this.Effect.Attached.Transform;
					this.Effect.Attached.Transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
				}
			}
		}

		private static readonly BitLogger Log = new BitLogger(typeof(PerkJumpMovement));

		private float _a;

		private float _b;

		private float _c;

		private float _zeroHeight;

		private float _deltaTimeRatioToWallCollision;

		private Vector3 validTarget;

		public bool useCurve;

		public AnimationCurve animationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		public bool UseTargetXZVelocity;
	}
}
