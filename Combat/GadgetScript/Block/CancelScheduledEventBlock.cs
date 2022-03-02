using System;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Time/CancelScheduledEvent")]
	public class CancelScheduledEventBlock : BaseBlock
	{
		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMGadgetContext ihmmgadgetContext = (IHMMGadgetContext)gadgetContext;
			if (ihmmgadgetContext.IsServer)
			{
				IParameterTomate<float> parameterTomate = this._eventId.ParameterTomate as IParameterTomate<float>;
				gadgetContext.CancelScheduledEvent((int)parameterTomate.GetValue(gadgetContext));
			}
			return this._nextBlock;
		}

		[Header("Read")]
		[Tooltip("Id of the event to Cancel")]
		[Restrict(true, new Type[]
		{
			typeof(float)
		})]
		[SerializeField]
		private BaseParameter _eventId;
	}
}
