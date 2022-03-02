using System;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Body
{
	public class GadgetBodySinMovement : GadgetBodyStraightMovement
	{
		public override void Initialize(IGadgetBody body, IGadgetContext gadgetContext, IEventContext eventContext)
		{
			base.Initialize(body, gadgetContext, eventContext);
			IParameterTomate<float> parameterTomate = this._amplitudeParameter.ParameterTomate as IParameterTomate<float>;
			this._amplitude = parameterTomate.GetValue(gadgetContext);
			this._amplitude *= ((this.BounceDirection != GadgetBodySinMovement.SinDirection.Left) ? 1f : -1f);
			IParameterTomate<float> parameterTomate2 = this._periodCountParameter.ParameterTomate as IParameterTomate<float>;
			float value = parameterTomate2.GetValue(gadgetContext);
			this._maxAngle = 6.2831855f * value;
			this._right = body.Rotation * Vector3.right;
		}

		public override Vector3 GetPosition(float elapsedTime)
		{
			Vector3 position = base.GetPosition(elapsedTime);
			float num = Mathf.Lerp(0f, this._maxAngle, elapsedTime / this._time);
			float num2 = Mathf.Sin(num);
			Vector3 vector = this._right * (num2 * this._amplitude);
			return position + vector;
		}

		[SerializeField]
		private BaseParameter _periodCountParameter;

		[SerializeField]
		private BaseParameter _amplitudeParameter;

		[SerializeField]
		private GadgetBodySinMovement.SinDirection BounceDirection;

		private float _amplitude;

		private float _maxAngle;

		private Vector3 _right;

		public enum SinDirection
		{
			Right,
			Left
		}
	}
}
