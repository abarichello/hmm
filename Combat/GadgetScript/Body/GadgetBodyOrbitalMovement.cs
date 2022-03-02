using System;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Body
{
	[RequireComponent(typeof(Rigidbody))]
	public class GadgetBodyOrbitalMovement : MonoBehaviour, IGadgetBodyMovement
	{
		public Vector3 GetPosition(float elapsedTime)
		{
			this._centerPosition = this._center.GetValue<Vector3>(this._context);
			Quaternion quaternion = Quaternion.SlerpUnclamped(Quaternion.identity, GadgetBodyOrbitalMovement.HalfRotation, this._speed * elapsedTime);
			return this._centerPosition + quaternion * this._radiusVector;
		}

		public Vector3 GetDirection()
		{
			return Vector3.zero;
		}

		public void Initialize(IGadgetBody body, IGadgetContext gadgetContext, IEventContext eventContext)
		{
			this._context = gadgetContext;
			this._centerPosition = this._center.GetValue<Vector3>(this._context);
			IParameterTomate<float> parameterTomate = this._radius.ParameterTomate as IParameterTomate<float>;
			this._radiusVector = parameterTomate.GetValue(this._context) * Vector3.forward;
			IParameterTomate<float> parameterTomate2 = this._degreesPerSecond.ParameterTomate as IParameterTomate<float>;
			this._speed = parameterTomate2.GetValue(this._context) / 180f;
			this.Finished = false;
		}

		public bool Finished { get; private set; }

		public void Destroy()
		{
			this.Finished = false;
		}

		[Header("Read")]
		[SerializeField]
		private BaseParameter _degreesPerSecond;

		[SerializeField]
		private BaseParameter _radius;

		[SerializeField]
		private BaseParameter _center;

		private static readonly Quaternion HalfRotation = Quaternion.AngleAxis(180f, Vector3.up);

		private IGadgetContext _context;

		private Vector3 _centerPosition;

		private Vector3 _radiusVector;

		private float _speed;
	}
}
