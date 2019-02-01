using System;
using Hoplon.GadgetScript;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Body
{
	public class GadgetBodyBoomerangMovement : GadgetBodyStraightMovement
	{
		public override void Initialize(IGadgetBody body, IGadgetContext gadgetContext, IEventContext eventContext)
		{
			base.Initialize(body, gadgetContext, eventContext);
			this._isGoingBack = false;
			this._body = body;
			IHMMGadgetContext ihmmgadgetContext = (IHMMGadgetContext)gadgetContext;
			this._ownerTransform = ((Identifiable)ihmmgadgetContext.GetIdentifiable(ihmmgadgetContext.OwnerId)).transform;
		}

		public override Vector3 GetPosition(float elapsedTime)
		{
			if (this._isGoingBack && this.Finished)
			{
				return this._ownerTransform.position;
			}
			if (!this._isGoingBack && elapsedTime >= this._time)
			{
				this._isGoingBack = true;
				if (this._velocityType != GadgetBodyStraightMovement.MovementType.Constant)
				{
					this._speed = -(this._speed + this._accel * this._time);
					this._accel = Math.Abs(this._accel);
				}
				this._origin = this._body.Position;
			}
			if (!this._isGoingBack)
			{
				return base.GetPosition(elapsedTime);
			}
			Vector3 position = this._ownerTransform.position;
			this._direction = (position - this._body.Position).normalized;
			this._speed += this._accel * Time.deltaTime;
			Vector3 a = this._direction * this._speed;
			Vector3 vector = this._body.Position + a * Time.deltaTime;
			float num = Vector3.SqrMagnitude(position - this._origin);
			this._end = position;
			if (num <= 10f)
			{
				this.Finished = true;
				vector = position;
			}
			vector.y = this._body.Position.y;
			this._origin = vector;
			return vector;
		}

		private bool _isGoingBack;

		private IGadgetBody _body;

		private Transform _ownerTransform;

		private const float DISTANCE_SQR_TO_FINISH = 10f;
	}
}
