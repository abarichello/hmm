using System;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Body
{
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
			IParameterTomate<float> parameterTomate = this._duration.ParameterTomate as IParameterTomate<float>;
			this._time = parameterTomate.GetValue(gadgetContext);
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
			IParameterTomate<float> parameterTomate = GadgetBodyStraightMovement._initialSpeedParameter.ParameterTomate as IParameterTomate<float>;
			this._initialSpeed = (this._speed = parameterTomate.GetValue(context));
			IParameterTomate<float> parameterTomate2 = GadgetBodyStraightMovement._rangeParameter.ParameterTomate as IParameterTomate<float>;
			float value = parameterTomate2.GetValue(context);
			this._end = this._origin + this._direction * value;
			this._accel = (value - this._speed * this._time) * 2f / (this._time * this._time);
			this._finalSpeed = this._initialSpeed + this._accel * this._time;
		}

		protected virtual void SetServerSpeedAccelAndRange(IGadgetContext context, IHMMEventContext eventContext)
		{
			float num = 0f;
			if (this._extraVelocity != null)
			{
				num = this.ExtraSpeedFromOwner(this._extraVelocity.GetValue<Vector3>(context));
			}
			IParameterTomate<float> parameterTomate = this._speedParameter.ParameterTomate as IParameterTomate<float>;
			this._initialSpeed = (this._finalSpeed = parameterTomate.GetValue(context) + num);
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
			IParameterTomate<float> parameterTomate2 = GadgetBodyStraightMovement._initialSpeedParameter.ParameterTomate as IParameterTomate<float>;
			parameterTomate2.SetValue(context, this._initialSpeed);
			IParameterTomate<float> parameterTomate3 = GadgetBodyStraightMovement._rangeParameter.ParameterTomate as IParameterTomate<float>;
			parameterTomate3.SetValue(context, num2);
			eventContext.SaveParameter(GadgetBodyStraightMovement._initialSpeedParameter);
			eventContext.SaveParameter(GadgetBodyStraightMovement._rangeParameter);
		}

		protected virtual float ExtraSpeedFromOwner(Vector3 carVelocity)
		{
			float num = Vector3.Dot(carVelocity, this._direction);
			float num2 = Mathf.Max(0f, num) * this._extraPositiveVelocityMultiplier;
			return num2 + Mathf.Min(0f, num) * this._extraNegativeVelocityMultiplier;
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

		private void OnDrawGizmosSelected()
		{
			Gizmos.DrawLine(this._origin, this._end);
		}

		[Header("Read")]
		[SerializeField]
		protected GadgetBodyStraightMovement.MovementType _velocityType;

		[SerializeField]
		protected BaseParameter _speedParameter;

		[SerializeField]
		protected BaseParameter _duration;

		[SerializeField]
		protected BaseParameter _extraVelocity;

		[Tooltip("If the car is moving in the SAME direction of the projectile when shooting it, how much of the speed should be added to the projectile ")]
		[SerializeField]
		protected float _extraPositiveVelocityMultiplier = 1f;

		[Tooltip("If the car is moving in the OPPOSITE direction of the projectile when shooting it, how much of the speed should be added to the projectile ")]
		[SerializeField]
		protected float _extraNegativeVelocityMultiplier = 1f;

		protected static BaseParameter _rangeParameter;

		protected static BaseParameter _initialSpeedParameter;

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
