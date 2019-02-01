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
			return this._directionParameter.GetValue(this._hmmContext);
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
			this._velocity = this._directionParameter.GetValue(this._hmmContext) * (this._initialSpeedParameter.GetValue(this._hmmContext) + this.ExtraSpeed(this._extraVelocity.GetValue(this._hmmContext)));
			this._time = (float)this._hmmContext.CurrentTime;
			this._body = body;
		}

		protected virtual float ExtraSpeed(Vector3 extraVelocity)
		{
			float b = Vector3.Dot(extraVelocity, this._directionParameter.GetValue(this._hmmContext));
			float num = Mathf.Max(0f, b) * this._extraPositiveVelocityMultiplier;
			return num + Mathf.Min(0f, b) * this._extraNegativeVelocityMultiplier;
		}

		public virtual Vector3 GetPosition(float elapsedTime)
		{
			if (this._hmmContext.IsClient)
			{
				return base.transform.position;
			}
			float num = elapsedTime - this._time;
			this._time = elapsedTime;
			Vector3.MoveTowards(this._velocity, this._directionParameter.GetValue(this._hmmContext) * this._maxSpeedParameter.GetValue(this._hmmContext), this._accelerationParameter.GetValue(this._hmmContext) * num);
			return this._body.Position + this._velocity;
		}

		public virtual void Destroy()
		{
		}

		[Header("Read")]
		[SerializeField]
		protected FloatParameter _maxSpeedParameter;

		[SerializeField]
		protected FloatParameter _accelerationParameter;

		[SerializeField]
		protected Vector3Parameter _directionParameter;

		[SerializeField]
		protected FloatParameter _initialSpeedParameter;

		[SerializeField]
		protected Vector3Parameter _extraVelocity;

		[SerializeField]
		[Tooltip("If the car is moving in the SAME direction of the projectile when shooting it, how much of the speed should be added to the projectile ")]
		protected float _extraPositiveVelocityMultiplier = 1f;

		[SerializeField]
		[Tooltip("If the car is moving in the OPPOSITE direction of the projectile when shooting it, how much of the speed should be added to the projectile ")]
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
