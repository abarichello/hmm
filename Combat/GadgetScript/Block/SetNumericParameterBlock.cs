using System;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Parameter/SetNumericParameter")]
	public class SetNumericParameterBlock : BaseBlock
	{
		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMGadgetContext ihmmgadgetContext = (IHMMGadgetContext)gadgetContext;
			IHMMEventContext ihmmeventContext = (IHMMEventContext)eventContext;
			if (ihmmgadgetContext.IsServer)
			{
				IParameterTomate<float> parameterTomate = this._parameterToSet.ParameterTomate as IParameterTomate<float>;
				parameterTomate.SetValue(gadgetContext, this._value);
				ihmmeventContext.SaveParameter(this._parameterToSet);
				if (this._sendToClient)
				{
					IHMMEventContext ihmmeventContext2 = (IHMMEventContext)eventContext;
					ihmmeventContext2.SendToClient();
				}
			}
			else
			{
				ihmmeventContext.LoadParameter(this._parameterToSet);
			}
			return this._nextBlock;
		}

		[Restrict(true, new Type[]
		{
			typeof(float)
		})]
		[SerializeField]
		private BaseParameter _parameterToSet;

		[SerializeField]
		private float _value;

		[SerializeField]
		private bool _sendToClient;
	}
}
