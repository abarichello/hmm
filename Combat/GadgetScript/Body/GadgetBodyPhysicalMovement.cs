using System;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Body
{
	[RequireComponent(typeof(Rigidbody))]
	public class GadgetBodyPhysicalMovement : MonoBehaviour, IGadgetBodyMovement
	{
		public Vector3 GetDirection()
		{
			return this._directionParameter.GetValue<Vector3>(this._hmmContext);
		}

		public virtual bool Finished
		{
			get
			{
				return false;
			}
		}

		public virtual void Initialize(IGadgetBody body, IGadgetContext gadgetContext, IEventContext eventContext)
		{
			this._hmmContext = (IHMMGadgetContext)gadgetContext;
			IParameterTomate<float> parameterTomate = this._initialSpeedParameter.ParameterTomate as IParameterTomate<float>;
			this._velocity = this._directionParameter.GetValue<Vector3>(this._hmmContext) * (parameterTomate.GetValue(this._hmmContext) + this.ExtraSpeed(this._extraVelocity.GetValue<Vector3>(this._hmmContext)));
			this._time = (float)this._hmmContext.CurrentTime;
			this._body = body;
		}

		protected virtual float ExtraSpeed(Vector3 extraVelocity)
		{
			float num = Vector3.Dot(extraVelocity, this._directionParameter.GetValue<Vector3>(this._hmmContext));
			float num2 = Mathf.Max(0f, num) * this._extraPositiveVelocityMultiplier;
			return num2 + Mathf.Min(0f, num) * this._extraNegativeVelocityMultiplier;
		}

		public virtual Vector3 GetPosition(float elapsedTime)
		{
			if (this._hmmContext.IsClient)
			{
				return base.transform.position;
			}
			float num = elapsedTime - this._time;
			this._time = elapsedTime;
			IParameterTomate<float> parameterTomate = this._maxSpeedParameter.ParameterTomate as IParameterTomate<float>;
			IParameterTomate<float> parameterTomate2 = this._accelerationParameter.ParameterTomate as IParameterTomate<float>;
			Vector3.MoveTowards(this._velocity, this._directionParameter.GetValue<Vector3>(this._hmmContext) * parameterTomate.GetValue(this._hmmContext), parameterTomate2.GetValue(this._hmmContext) * num);
			return this._body.Position + this._velocity;
		}

		public virtual void Destroy()
		{
		}

		[Header("Read")]
		[SerializeField]
		protected BaseParameter _maxSpeedParameter;

		[SerializeField]
		protected BaseParameter _accelerationParameter;

		[SerializeField]
		protected BaseParameter _directionParameter;

		[SerializeField]
		protected BaseParameter _initialSpeedParameter;

		[SerializeField]
		protected BaseParameter _extraVelocity;

		[Tooltip("If the car is moving in the SAME direction of the projectile when shooting it, how much of the speed should be added to the projectile ")]
		[SerializeField]
		protected float _extraPositiveVelocityMultiplier = 1f;

		[Tooltip("If the car is moving in the OPPOSITE direction of the projectile when shooting it, how much of the speed should be added to the projectile ")]
		[SerializeField]
		protected float _extraNegativeVelocityMultiplier = 1f;

		protected float _time;

		protected Vector3 _velocity;

		protected IHMMGadgetContext _hmmContext;

		protected IGadgetBody _body;

		public enum MovementType
		{
			Constant,
			Accelerated,
			Deaccelerated
		}
	}
}
