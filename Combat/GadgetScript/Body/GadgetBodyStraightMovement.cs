using System;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Body
{
	[RequireComponent(typeof(Rigidbody))]
	public class GadgetBodyStraightMovement : MonoBehaviour, IGadgetBodyMovement
	{
		public Vector3 GetDirection()
		{
			return this._direction;
		}

		public virtual bool Finished { get; protected set; }

		private void Awake()
		{
			if (GadgetBodyStraightMovement._rangeParameter == null)
			{
				GadgetBodyStraightMovement._rangeParameter = ScriptableObject.CreateInstance<FloatParameter>();
				GadgetBodyStraightMovement._initialSpeedParameter = ScriptableObject.CreateInstance<FloatParameter>();
			}
		}

		public virtual void Initialize(IGadgetBody body, IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMGadgetContext ihmmgadgetContext = (IHMMGadgetContext)gadgetContext;
			this._speedParameter = (FloatParameter)BaseParameter.GetParameter(this._speedParameter.ContentId);
			this._duration = (FloatParameter)BaseParameter.GetParameter(this._duration.ContentId);
			this._time = this._duration.GetValue(gadgetContext);
			this._origin = body.Position;
			this._direction = body.Rotation * Vector3.forward;
			this._origin.y = 0f;
			if (ihmmgadgetContext.IsServer)
			{
				this.SetServerSpeedAccelAndRange(gadgetContext, (IHMMEventContext)eventContext);
			}
			else
			{
				this.SetClientSpeedAccelAndRange(gadgetContext, (IHMMEventContext)eventContext);
			}
		}

		protected virtual void SetClientSpeedAccelAndRange(IGadgetContext context, IHMMEventContext eventContext)
		{
			eventContext.LoadParameter(GadgetBodyStraightMovement._initialSpeedParameter);
			eventContext.LoadParameter(GadgetBodyStraightMovement._rangeParameter);
			this._initialSpeed = (this._speed = GadgetBodyStraightMovement._initialSpeedParameter.GetValue(context));
			float value = GadgetBodyStraightMovement._rangeParameter.GetValue(context);
			this._end = this._origin + this._direction * value;
			this._accel = (value - this._speed * this._time) * 2f / (this._time * this._time);
			this._finalSpeed = this._initialSpeed + this._accel * this._time;
		}

		protected virtual void SetServerSpeedAccelAndRange(IGadgetContext context, IHMMEventContext eventContext)
		{
			float num = 0f;
			if (this._extraVelocity != null)
			{
				this._extraVelocity = (Vector3Parameter)BaseParameter.GetParameter(this._extraVelocity.ContentId);
				num = this.ExtraSpeedFromOwner(this._extraVelocity.GetValue(context));
			}
			this._initialSpeed = (this._finalSpeed = this._speedParameter.GetValue(context) + num);
			if (this._velocityType == GadgetBodyStraightMovement.MovementType.Accelerated)
			{
				this._initialSpeed = num;
			}
			if (this._velocityType == GadgetBodyStraightMovement.MovementType.Deaccelerated)
			{
				this._finalSpeed = 0f;
			}
			this._speed = this._initialSpeed;
			this._accel = (this._finalSpeed - this._initialSpeed) / this._time;
			float num2 = this._speed * this._time + this._accel * this._time * this._time / 2f;
			this._end = this._origin + this._direction * num2;
			this._end.y = 0f;
			GadgetBodyStraightMovement._initialSpeedParameter.SetValue(context, this._initialSpeed);
			GadgetBodyStraightMovement._rangeParameter.SetValue(context, num2);
			eventContext.SaveParameter(GadgetBodyStraightMovement._initialSpeedParameter);
			eventContext.SaveParameter(GadgetBodyStraightMovement._rangeParameter);
		}

		protected virtual float ExtraSpeedFromOwner(Vector3 carVelocity)
		{
			float b = Vector3.Dot(carVelocity, this._direction);
			float num = Mathf.Max(0f, b) * this._extraPositiveVelocityMultiplier;
			return num + Mathf.Min(0f, b) * this._extraNegativeVelocityMultiplier;
		}

		public virtual Vector3 GetPosition(float elapsedTime)
		{
			if (elapsedTime > this._time)
			{
				this.Finished = true;
				return this._end;
			}
			return this._origin + this._direction * (this._speed * elapsedTime + this._accel * elapsedTime * elapsedTime / 2f);
		}

		public virtual void Destroy()
		{
			this.Finished = false;
		}

		[Header("Read")]
		[SerializeField]
		protected GadgetBodyStraightMovement.MovementType _velocityType;

		[SerializeField]
		protected FloatParameter _speedParameter;

		[SerializeField]
		protected FloatParameter _duration;

		[SerializeField]
		protected Vector3Parameter _extraVelocity;

		[SerializeField]
		[Tooltip("If the car is moving in the SAME direction of the projectile when shooting it, how much of the speed should be added to the projectile ")]
		protected float _extraPositiveVelocityMultiplier = 1f;

		[SerializeField]
		[Tooltip("If the car is moving in the OPPOSITE direction of the projectile when shooting it, how much of the speed should be added to the projectile ")]
		protected float _extraNegativeVelocityMultiplier = 1f;

		protected static FloatParameter _rangeParameter;

		protected static FloatParameter _initialSpeedParameter;

		protected float _time;

		protected Vector3 _direction;

		protected Vector3 _origin;

		protected Vector3 _end;

		protected float _accel;

		protected float _speed;

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
