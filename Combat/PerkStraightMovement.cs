using System;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkStraightMovement : BasePerk, IPerkMovement
	{
		protected float _deltaTime
		{
			get
			{
				if (GameHubBehaviour.Hub.Net.IsServer())
				{
					return Time.fixedTime - this._startTime;
				}
				return (float)GameHubBehaviour.Hub.GameTime.GetPlaybackTime() * 0.001f - this._startTime;
			}
		}

		protected float _deltaTimeRatio
		{
			get
			{
				return (this._startTime != this._endTime) ? (this._deltaTime / (this._endTime - this._startTime)) : 0f;
			}
		}

		protected float _endDeltaTime
		{
			get
			{
				return this._endTime - this._startTime;
			}
		}

		public Vector3 Direction
		{
			get
			{
				return this._direction;
			}
		}

		public override void PerkInitialized()
		{
			base.PerkInitialized();
			EffectEvent data = this.Effect.Data;
			bool flag = GameHubBehaviour.Hub.Net.IsServer();
			this._startTime = ((!flag) ? ((float)GameHubBehaviour.Hub.GameTime.GetPlaybackTime() * 0.001f) : Time.fixedTime);
			this._endTime = this._startTime + this._duration;
			this._direction = data.Direction;
			this._origin = this.Effect.Data.Origin;
			this._origin.y = 0f;
			if (flag)
			{
				this.SetServerSpeedAccelAndRange(data);
			}
			else
			{
				this.SetClientSpeedAccelAndRange(data);
			}
			if (!flag && this.Effect.Data.FirstPackageSent)
			{
				this.UpdateAfterVisibilityChange();
			}
		}

		protected virtual void SetClientSpeedAccelAndRange(EffectEvent data)
		{
			this._initialSpeed = (this._speed = data.MoveSpeed);
			this._end = this._origin + this._direction * data.Range;
			this._accel = (data.Range - this._speed * this._endDeltaTime) * 2f / (this._endDeltaTime * this._endDeltaTime);
			this._finalSpeed = this._initialSpeed + this._accel * this._endDeltaTime;
		}

		protected virtual void SetServerSpeedAccelAndRange(EffectEvent data)
		{
			float num = this.ExtraSpeedFromCar(data.SourceCombat.Movement.LastVelocity);
			this._initialSpeed = (this._finalSpeed = data.MoveSpeed + num);
			if (this._velocityType == PerkStraightMovement.MovementType.Accelerated)
			{
				this._initialSpeed = num;
			}
			if (this._velocityType == PerkStraightMovement.MovementType.Deaccelerated)
			{
				this._finalSpeed = 0f;
			}
			this._speed = this._initialSpeed;
			this._accel = (this._finalSpeed - this._initialSpeed) / this._duration;
			float num2 = this._speed * this._endDeltaTime + this._accel * this._endDeltaTime * this._endDeltaTime / 2f;
			this._end = this._origin + this._direction * num2;
			this._end.y = 0f;
			data.MoveSpeed = this._speed;
			data.Range = num2;
		}

		protected virtual float ExtraSpeedFromCar(Vector3 carVelocity)
		{
			float num = Vector3.Dot(carVelocity, this._direction);
			float num2 = Mathf.Max(0f, num) * this._extraPositiveVelocityMultiplier;
			return num2 + Mathf.Min(0f, num) * this._extraNegativeVelocityMultiplier;
		}

		protected virtual void UpdateAfterVisibilityChange()
		{
			this._startTime = (float)this.Effect.Data.EventTime * 0.001f;
			this._endTime = this._startTime + this._duration;
			float num;
			base._trans.position = this.CalcNewPosition(out num);
		}

		public virtual Vector3 UpdatePosition()
		{
			float num;
			Vector3 result = this.CalcNewPosition(out num);
			float num2 = (!GameHubBehaviour.Hub.Net.IsServer()) ? ((float)GameHubBehaviour.Hub.GameTime.GetPlaybackTime() * 0.001f) : Time.fixedTime;
			if (num2 > this._endTime)
			{
				result = this._end;
			}
			return result;
		}

		protected virtual Vector3 CalcNewPosition(out float moved)
		{
			float num = this._deltaTime;
			if (this._deltaTime > this._endDeltaTime)
			{
				num = this._endDeltaTime;
			}
			Vector3 vector = this._origin + this._direction * (this._speed * num + this._accel * num * num / 2f);
			moved = Vector3.SqrMagnitude(vector - this._origin);
			vector.y = base._trans.position.y;
			return vector;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(PerkStraightMovement));

		[SerializeField]
		protected PerkStraightMovement.MovementType _velocityType;

		[SerializeField]
		protected float _duration = 1f;

		[Tooltip("If the car is moving in the SAME direction of the projectile when shooting it, how much of the speed should be added to the projectile ")]
		[SerializeField]
		protected float _extraPositiveVelocityMultiplier = 1f;

		[Tooltip("If the car is moving in the OPPOSITE direction of the projectile when shooting it, how much of the speed should be added to the projectile ")]
		[SerializeField]
		protected float _extraNegativeVelocityMultiplier = 1f;

		protected Vector3 _direction;

		protected Vector3 _origin;

		protected Vector3 _end;

		protected float _accel;

		protected float _speed;

		protected float _startTime;

		protected float _endTime;

		private float _initialSpeed;

		private float _finalSpeed;

		public enum MovementType
		{
			Constant,
			Accelerated,
			Deaccelerated
		}
	}
}
