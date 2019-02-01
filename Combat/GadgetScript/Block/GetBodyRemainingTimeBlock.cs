using System;
using HeavyMetalMachines.Combat.GadgetScript.Body;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Body/GetBodyRemainingTimeBlock")]
	public class GetBodyRemainingTimeBlock : BaseBlock
	{
		protected override bool CheckSanity(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			if (this._body == null)
			{
				base.LogSanitycheckError("'Bdoy' parameter cannot be null.");
				return false;
			}
			if (this._remainingTime == null)
			{
				base.LogSanitycheckError("'Remaining Time' parameter cannot be null.");
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
				GadgetBody value = this._body.GetValue(gadgetContext);
				this._remainingTime.SetValue(gadgetContext, value.GetRemainingTime());
				ihmmeventContext.SaveParameter(this._remainingTime);
				return this._nextBlock;
			}
			ihmmeventContext.LoadParameter(this._remainingTime);
			return this._nextBlock;
		}

		public override bool UsesParameterWithId(int parameterId)
		{
			return base.CheckIsParameterWithId(this._body, parameterId) || base.CheckIsParameterWithId(this._remainingTime, parameterId);
		}

		[Header("Read")]
		[SerializeField]
		private GadgetBodyParameter _body;

		[Header("Write")]
		[SerializeField]
		private FloatParameter _remainingTime;
	}
}
