using System;
using HeavyMetalMachines.Combat.Gadget;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Cancel Listen")]
	public class CancelListenToBlock : BaseBlock
	{
		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMGadgetContext ihmmgadgetContext = gadgetContext as IHMMGadgetContext;
			if (ihmmgadgetContext.IsServer || ihmmgadgetContext.IsTest)
			{
				IParameterTomate<float> parameterTomate = (IParameterTomate<float>)this._listenId.ParameterTomate;
				IHMMGadgetContext gadgetContext2 = ihmmgadgetContext.Owner.GetGadgetContext((int)this._listenedGadget);
				gadgetContext2.CancelListenToBlock((int)parameterTomate.GetValue(ihmmgadgetContext));
				if (this._listenIds != null)
				{
					for (int i = 0; i < this._listenIds.Length; i++)
					{
						parameterTomate = (IParameterTomate<float>)this._listenIds[i].ParameterTomate;
						gadgetContext2.CancelListenToBlock((int)parameterTomate.GetValue(ihmmgadgetContext));
					}
				}
			}
			return this._nextBlock;
		}

		[Header("Read")]
		[SerializeField]
		private GadgetSlot _listenedGadget;

		[SerializeField]
		private BaseParameter _listenId;

		[SerializeField]
		private BaseParameter[] _listenIds;
	}
}
