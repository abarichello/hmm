using System;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Parameter/SetNumericParameter")]
	public class SetNumericParameterBlock : BaseBlock
	{
		protected override bool CheckSanity(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			if (this._parameterToSet == null)
			{
				base.LogSanitycheckError("'Parameter To Set' parameter cannot be null.");
				return false;
			}
			if (!(this._parameterToSet is INumericParameter))
			{
				base.LogSanitycheckError("'Parameter To Set' must be a numeric parameter.");
				return false;
			}
			return true;
		}

		protected override IBlock InnerExecute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMGadgetContext ihmmgadgetContext = (IHMMGadgetContext)gadgetContext;
			IHMMEventContext ihmmeventContext = (IHMMEventContext)eventContext;
			if (ihmmgadgetContext.IsServer)
			{
				((INumericParameter)this._parameterToSet).SetFloatValue(gadgetContext, this._value);
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

		public override bool UsesParameterWithId(int parameterId)
		{
			return base.CheckIsParameterWithId(this._parameterToSet, parameterId);
		}

		[SerializeField]
		private BaseParameter _parameterToSet;

		[SerializeField]
		private float _value;

		[SerializeField]
		private bool _sendToClient;
	}
}
