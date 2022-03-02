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
			this._gadget = gadgetContext;
			this._body = body;
			IHMMGadgetContext ihmmgadgetContext = (IHMMGadgetContext)gadgetContext;
			this._referenceTransform = ((!this._useDummyBombHook) ? ((Identifiable)ihmmgadgetContext.Owner.Identifiable).transform : ihmmgadgetContext.Owner.Dummy.BombHook);
			this._returning = false;
			this._returningParameter.SetValue(this._gadget, false);
			this._returningParameter.OnParameterValueUpdated -= this.OnReturningUpdated;
			this._returningParameter.OnParameterValueUpdated += this.OnReturningUpdated;
		}

		public override Vector3 GetPosition(float elapsedTime)
		{
			if (this._returning && this.Finished)
			{
				return this._referenceTransform.position;
			}
			if (!this._returning && elapsedTime >= this._time)
			{
				this._returningParameter.SetValue(this._gadget, true);
			}
			if (!this._returning)
			{
				return base.GetPosition(elapsedTime);
			}
			Vector3 position = this._referenceTransform.position;
			this._direction = (position - this._body.Position).normalized;
			this._speed += this._accel * Time.deltaTime;
			Vector3 vector = this._direction * this._speed;
			Vector3 vector2 = this._body.Position + vector * Time.deltaTime;
			float num = Vector3.SqrMagnitude(position - this._origin);
			this._end = position;
			if (num <= 10f)
			{
				this.Finished = true;
				vector2 = position;
			}
			vector2.y = this._body.Position.y;
			this._origin = vector2;
			return vector2;
		}

		private void OnReturningUpdated(object context)
		{
			if (context != this._gadget)
			{
				return;
			}
			bool value = this._returningParameter.GetValue(context);
			if (value == this._returning)
			{
				return;
			}
			this._returning = value;
			if (this._returning)
			{
				this._origin = this._body.Position;
				if (this._velocityType != GadgetBodyStraightMovement.MovementType.Constant)
				{
					this._speed = 0f;
					this._accel = Math.Abs(this._accel);
				}
			}
		}

		public bool isReturning
		{
			get
			{
				return this._returning;
			}
		}

		[SerializeField]
		private BoolParameter _returningParameter;

		[SerializeField]
		private bool _useDummyBombHook;

		private bool _returning;

		private IGadgetBody _body;

		private IGadgetContext _gadget;

		private Transform _referenceTransform;

		private const float DISTANCE_SQR_TO_FINISH = 10f;
	}
}
