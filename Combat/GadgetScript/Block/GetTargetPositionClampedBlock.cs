using System;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Parameter/GetTargetPositionClamped")]
	public class GetTargetPositionClampedBlock : BaseBlock
	{
		protected override bool CheckSanity(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			if (this._initialPosition == null)
			{
				base.LogSanitycheckError("'Initial Position' parameter cannot be null.");
				return false;
			}
			if (this._targetPosition == null)
			{
				base.LogSanitycheckError("'Target Position' parameter cannot be null.");
				return false;
			}
			if (this._finalPosition == null)
			{
				base.LogSanitycheckError("'Final Position' parameter cannot be null.");
				return false;
			}
			if (this._maxDistance != null && this._maxDistance.GetValue(gadgetContext) < 0f)
			{
				base.LogSanitycheckError("'Max Distance' parameter cannot be negative.");
				return false;
			}
			return true;
		}

		protected override IBlock InnerExecute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMGadgetContext ihmmgadgetContext = (IHMMGadgetContext)gadgetContext;
			IHMMEventContext ihmmeventContext = (IHMMEventContext)eventContext;
			if (ihmmgadgetContext.IsClient)
			{
				ihmmeventContext.LoadParameter(this._finalPosition);
			}
			else
			{
				this.SetPositionParameter(gadgetContext);
				ihmmeventContext.SaveParameter(this._finalPosition);
			}
			return this._nextBlock;
		}

		public override bool UsesParameterWithId(int parameterId)
		{
			return base.CheckIsParameterWithId(this._initialPosition, parameterId) || base.CheckIsParameterWithId(this._targetPosition, parameterId) || base.CheckIsParameterWithId(this._maxDistance, parameterId) || base.CheckIsParameterWithId(this._finalPosition, parameterId);
		}

		private void SetPositionParameter(IGadgetContext gadgetContext)
		{
			Vector3 vector = this._targetPosition.GetValue(gadgetContext);
			float num = (!(this._maxDistance == null)) ? this._maxDistance.GetValue(gadgetContext) : 0f;
			if (num <= 0f)
			{
				this._finalPosition.SetValue(gadgetContext, vector);
				return;
			}
			Vector3 value = this._initialPosition.GetValue(gadgetContext);
			Vector3 b = vector - value;
			float num2 = num * num;
			if (b.sqrMagnitude <= num2)
			{
				this._finalPosition.SetValue(gadgetContext, vector);
				return;
			}
			b = b.normalized * num;
			vector = value + b;
			this._finalPosition.SetValue(gadgetContext, vector);
		}

		[Header("Read")]
		[SerializeField]
		private Vector3Parameter _initialPosition;

		[SerializeField]
		private Vector3Parameter _targetPosition;

		[SerializeField]
		private FloatParameter _maxDistance;

		[Header("Write")]
		[SerializeField]
		private Vector3Parameter _finalPosition;
	}
}
