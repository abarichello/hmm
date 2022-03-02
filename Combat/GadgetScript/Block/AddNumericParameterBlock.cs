using System;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Parameter/AddNumericParameter")]
	public class AddNumericParameterBlock : BaseBlock
	{
		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMGadgetContext ihmmgadgetContext = (IHMMGadgetContext)gadgetContext;
			IHMMEventContext ihmmeventContext = (IHMMEventContext)eventContext;
			if (this._parameterToAdd == null)
			{
				Debug.LogFormat(this, "Missing parameters {0}", new object[]
				{
					base.name
				});
			}
			IParameterTomate<float> parameterTomate = this._parameterToAdd.ParameterTomate as IParameterTomate<float>;
			float value = this._value;
			if (ihmmgadgetContext.IsClient)
			{
				parameterTomate.SetValue(gadgetContext, parameterTomate.GetValue(gadgetContext) + value);
				ihmmeventContext.LoadParameter(this._parameterToAdd);
				return this._nextBlock;
			}
			if (this._valueParameter != null)
			{
				IParameterTomate<float> parameterTomate2 = this._valueParameter.ParameterTomate as IParameterTomate<float>;
				value = parameterTomate2.GetValue(gadgetContext);
			}
			parameterTomate.SetValue(gadgetContext, parameterTomate.GetValue(gadgetContext) + value);
			ihmmeventContext.SaveParameter(this._parameterToAdd);
			if (this._sendToClient)
			{
				IHMMEventContext ihmmeventContext2 = (IHMMEventContext)eventContext;
				ihmmeventContext2.SendToClient();
			}
			return this._nextBlock;
		}

		[Header("Read")]
		[Restrict(true, new Type[]
		{
			typeof(float)
		})]
		[SerializeField]
		private BaseParameter _parameterToAdd;

		[Restrict(false, new Type[]
		{
			typeof(float)
		})]
		[SerializeField]
		private BaseParameter _valueParameter;

		[SerializeField]
		[Tooltip("Only used if no Value Parameter is set.")]
		private float _value;

		[SerializeField]
		private bool _sendToClient;
	}
}
