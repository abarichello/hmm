using System;
using HeavyMetalMachines.Combat.GadgetScript.Body;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Body/GetBodyRemainingTimeBlock")]
	[Obsolete("Get the Remaining time as a Parameter with the body as a source")]
	public class GetBodyRemainingTimeBlock : BaseBlock
	{
		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMGadgetContext ihmmgadgetContext = (IHMMGadgetContext)gadgetContext;
			IHMMEventContext ihmmeventContext = (IHMMEventContext)eventContext;
			if (ihmmgadgetContext.IsServer)
			{
				GadgetBody value = this._body.GetValue<GadgetBody>(gadgetContext);
				IParameterTomate<float> parameterTomate = this._remainingTime.ParameterTomate as IParameterTomate<float>;
				parameterTomate.SetValue(gadgetContext, value.GetRemainingTime());
				ihmmeventContext.SaveParameter(this._remainingTime);
				return this._nextBlock;
			}
			ihmmeventContext.LoadParameter(this._remainingTime);
			return this._nextBlock;
		}

		[Header("Read")]
		[Restrict(true, new Type[]
		{
			typeof(GadgetBody)
		})]
		[SerializeField]
		private BaseParameter _body;

		[Header("Write")]
		[SerializeField]
		private BaseParameter _remainingTime;
	}
}
