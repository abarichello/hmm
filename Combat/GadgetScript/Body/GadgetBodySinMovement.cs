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
			this._bounceCount = this._bounceCountParameter.GetValue(gadgetContext);
			this._bounceRadius = this._bounceRadiusParameter.GetValue(gadgetContext);
			this._toRad = 0.0174532924f;
			this._maxAngle = 180 * this._bounceCount;
			GadgetBodySinMovement.SinDirection bounceDirection = this.BounceDirection;
			if (bounceDirection != GadgetBodySinMovement.SinDirection.Right)
			{
				if (bounceDirection == GadgetBodySinMovement.SinDirection.Left)
				{
					this._finalDirection = -(body.Rotation * Vector3.right);
				}
			}
			else
			{
				this._finalDirection = body.Rotation * Vector3.right;
			}
		}

		public override Vector3 GetPosition(float elapsedTime)
		{
			Vector3 position = base.GetPosition(elapsedTime);
			float num = Mathf.Lerp(0f, (float)this._maxAngle, elapsedTime / this._time);
			float num2 = Mathf.Sin(this._toRad * num);
			Vector3 b = this._finalDirection * (num2 * this._bounceRadius);
			return position + b;
		}

		[SerializeField]
		private IntParameter _bounceCountParameter;

		[SerializeField]
		private FloatParameter _bounceRadiusParameter;

		[SerializeField]
		private GadgetBodySinMovement.SinDirection BounceDirection;

		private Vector3 _finalDirection = Vector3.zero;

		private int _bounceCount;

		private float _bounceRadius;

		private int _maxAngle;

		private float _toRad;

		public enum SinDirection
		{
			Right,
			Left
		}
	}
}
