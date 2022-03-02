using System;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Parameter/SetParameter")]
	public class SetParameterBlock : BaseBlock
	{
		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
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

		[Header("Write")]
		[Restrict(true, new Type[]
		{

		})]
		[SerializeField]
		private BaseParameter _parameterToSet;

		[Header("Read")]
		[Restrict(true, new Type[]
		{

		})]
		[SerializeField]
		private BaseParameter _parameterValue;

		[SerializeField]
		private bool _sendToClient;
	}
}
