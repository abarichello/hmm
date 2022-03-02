using System;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Parameter/MultiplyNumericParameter")]
	public class MultiplyNumericParameterBlock : BaseBlock
	{
		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMGadgetContext ihmmgadgetContext = (IHMMGadgetContext)gadgetContext;
			IHMMEventContext ihmmeventContext = (IHMMEventContext)eventContext;
			if (ihmmgadgetContext.IsClient)
			{
				ihmmeventContext.LoadParameter(this._parameterToMultiply);
				return this._nextBlock;
			}
			IParameterTomate<float> parameterTomate = this._parameterToMultiply.ParameterTomate as IParameterTomate<float>;
			parameterTomate.SetValue(gadgetContext, parameterTomate.GetValue(gadgetContext) * this._value);
			ihmmeventContext.SaveParameter(this._parameterToMultiply);
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
		private BaseParameter _parameterToMultiply;

		[SerializeField]
		private float _value;

		[SerializeField]
		private bool _sendToClient;
	}
}
