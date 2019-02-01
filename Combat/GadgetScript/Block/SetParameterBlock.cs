using System;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Parameter/SetParameter")]
	public class SetParameterBlock : BaseBlock
	{
		protected override bool CheckSanity(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			if (this._parameterToSet == null)
			{
				base.LogSanitycheckError("'Parameter To Set' parameter cannot be null.");
				return false;
			}
			if (this._parameterValue == null)
			{
				base.LogSanitycheckError("'Parameter Value' parameter cannot be null.");
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
				this._parameterToSet.SetTo(gadgetContext, this._parameterValue);
				ihmmeventContext.SaveParameter(this._parameterToSet);
				if (this._sendToClient)
				{
					ihmmeventContext.SendToClient();
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
			return base.CheckIsParameterWithId(this._parameterToSet, parameterId) || base.CheckIsParameterWithId(this._parameterValue, parameterId);
		}

		[SerializeField]
		private BaseParameter _parameterToSet;

		[SerializeField]
		private BaseParameter _parameterValue;

		[SerializeField]
		private bool _sendToClient;
	}
}
