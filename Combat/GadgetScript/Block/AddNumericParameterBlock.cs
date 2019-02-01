using System;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Parameter/AddNumericParameter")]
	public class AddNumericParameterBlock : BaseBlock
	{
		protected override bool CheckSanity(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			if (this._parameterToAdd == null)
			{
				base.LogSanitycheckError("'Parameter To Add' cannot be null.");
				return false;
			}
			if (!(this._parameterToAdd is INumericParameter))
			{
				base.LogSanitycheckError("'Parameter To Add' must be a numeric parameter.");
				return false;
			}
			if (this._valueParameter != null && !(this._valueParameter is INumericParameter))
			{
				base.LogSanitycheckError("When set, 'Value Parameter' must be a numeric parameter.");
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
				ihmmeventContext.LoadParameter(this._parameterToAdd);
				return this._nextBlock;
			}
			INumericParameter numericParameter = (INumericParameter)this._parameterToAdd;
			INumericParameter numericParameter2 = this._valueParameter as INumericParameter;
			float num = this._value;
			if (numericParameter2 != null)
			{
				num = numericParameter2.GetFloatValue(gadgetContext);
			}
			numericParameter.SetFloatValue(gadgetContext, numericParameter.GetFloatValue(gadgetContext) + num);
			ihmmeventContext.SaveParameter(this._parameterToAdd);
			if (this._sendToClient)
			{
				IHMMEventContext ihmmeventContext2 = (IHMMEventContext)eventContext;
				ihmmeventContext2.SendToClient();
			}
			return this._nextBlock;
		}

		public override bool UsesParameterWithId(int parameterId)
		{
			return base.CheckIsParameterWithId(this._valueParameter, parameterId) || base.CheckIsParameterWithId(this._parameterToAdd, parameterId);
		}

		[Header("Read")]
		[SerializeField]
		private BaseParameter _parameterToAdd;

		[SerializeField]
		private BaseParameter _valueParameter;

		[SerializeField]
		[Tooltip("Only used if no Value Parameter is set.")]
		private float _value;

		[SerializeField]
		private bool _sendToClient;
	}
}
