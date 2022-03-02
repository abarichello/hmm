using System;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Parameter/SetBoolParameter")]
	public class SetBoolParameterBlock : BaseBlock
	{
		public override IBlock Execute(IGadgetContext context, IEventContext eventContext)
		{
			IHMMGadgetContext ihmmgadgetContext = (IHMMGadgetContext)context;
			IHMMEventContext ihmmeventContext = (IHMMEventContext)eventContext;
			if (ihmmgadgetContext.IsServer)
			{
				this._parameterToSet.SetValue<bool>(context, this._value);
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

		[Header("Write")]
		[Restrict(true, new Type[]
		{
			typeof(bool)
		})]
		[SerializeField]
		private BaseParameter _parameterToSet;

		[Header("Read")]
		[SerializeField]
		private bool _value;

		[SerializeField]
		private bool _sendToClient;
	}
}
