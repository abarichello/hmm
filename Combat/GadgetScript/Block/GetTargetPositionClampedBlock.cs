using System;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Parameter/GetTargetPositionClamped")]
	public class GetTargetPositionClampedBlock : BaseBlock
	{
		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
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

		private void SetPositionParameter(IGadgetContext gadgetContext)
		{
			Vector3 vector = this._targetPosition.GetValue<Vector3>(gadgetContext);
			IParameterTomate<float> parameterTomate = this._maxDistance.ParameterTomate as IParameterTomate<float>;
			float num = (!(this._maxDistance == null)) ? parameterTomate.GetValue(gadgetContext) : 0f;
			if (num <= 0f)
			{
				this._finalPosition.SetValue<Vector3>(gadgetContext, vector);
				return;
			}
			Vector3 value = this._initialPosition.GetValue<Vector3>(gadgetContext);
			Vector3 vector2 = vector - value;
			float num2 = num * num;
			if (vector2.sqrMagnitude <= num2)
			{
				this._finalPosition.SetValue<Vector3>(gadgetContext, vector);
				return;
			}
			vector2 = vector2.normalized * num;
			vector = value + vector2;
			this._finalPosition.SetValue<Vector3>(gadgetContext, vector);
		}

		[Header("Read")]
		[SerializeField]
		private BaseParameter _initialPosition;

		[SerializeField]
		private BaseParameter _targetPosition;

		[SerializeField]
		private BaseParameter _maxDistance;

		[Header("Write")]
		[SerializeField]
		private BaseParameter _finalPosition;
	}
}
