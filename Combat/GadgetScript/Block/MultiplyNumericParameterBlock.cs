using System;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Parameter/MultiplyNumericParameter")]
	public class MultiplyNumericParameterBlock : BaseBlock
	{
		protected override bool CheckSanity(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			if (this._parameterToMultiply == null)
			{
				base.LogSanitycheckError("'Parameter To Multiply' parameter cannot be null.");
				return false;
			}
			if (!(this._parameterToMultiply is INumericParameter))
			{
				base.LogSanitycheckError("'Parameter To Multiply' must be a numeric parameter.");
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
				ihmmeventContext.LoadParameter(this._parameterToMultiply);
				return this._nextBlock;
			}
			INumericParameter numericParameter = (INumericParameter)this._parameterToMultiply;
			numericParameter.SetFloatValue(gadgetContext, numericParameter.GetFloatValue(gadgetContext) * this._value);
			ihmmeventContext.SaveParameter(this._parameterToMultiply);
			if (this._sendToClient)
			{
				IHMMEventContext ihmmeventContext2 = (IHMMEventContext)eventContext;
				ihmmeventContext2.SendToClient();
			}
			return this._nextBlock;
		}

		public override bool UsesParameterWithId(int parameterId)
		{
			return base.CheckIsParameterWithId(this._parameterToMultiply, parameterId);
		}

		[Header("Read")]
		[SerializeField]
		private BaseParameter _parameterToMultiply;

		[SerializeField]
		private float _value;

		[SerializeField]
		private bool _sendToClient;
	}
}
