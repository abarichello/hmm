﻿using System;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Parameter/SetBoolParameter")]
	public class SetBoolParameterBlock : BaseBlock
	{
		protected override bool CheckSanity(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			if (this._parameterToSet == null)
			{
				base.LogSanitycheckError("'Parameter To Set' parameter cannot be null.");
				return false;
			}
			return true;
		}

		protected override IBlock InnerExecute(IGadgetContext context, IEventContext eventContext)
		{
			IHMMGadgetContext ihmmgadgetContext = (IHMMGadgetContext)context;
			IHMMEventContext ihmmeventContext = (IHMMEventContext)eventContext;
			if (ihmmgadgetContext.IsServer)
			{
				this._parameterToSet.SetValue(context, this._value);
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
		private BoolParameter _parameterToSet;

		[SerializeField]
		private bool _value;

		[SerializeField]
		private bool _sendToClient;
	}
}