using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat.Gadget;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Listen")]
	public class ListenToBlock : BaseBlock
	{
		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMGadgetContext ihmmgadgetContext = gadgetContext as IHMMGadgetContext;
			if (ihmmgadgetContext.IsServer || ihmmgadgetContext.IsTest)
			{
				IEventContext instance = GadgetEvent.GetInstance(this._onBlockExecuted.Id, ihmmgadgetContext, this._parameterList);
				IHMMGadgetContext gadgetContext2 = ihmmgadgetContext.Owner.GetGadgetContext((int)this._listenedGadget);
				gadgetContext2.ListenToBlock(this._listenTo, instance, ihmmgadgetContext);
				if (null != this._listenId)
				{
					IParameterTomate<float> parameterTomate = this._listenId.ParameterTomate as IParameterTomate<float>;
					parameterTomate.SetValue(ihmmgadgetContext, (float)instance.Id);
				}
			}
			return this._nextBlock;
		}

		[Header("Read")]
		[SerializeField]
		private GadgetSlot _listenedGadget;

		[SerializeField]
		private BaseBlock _listenTo;

		[SerializeField]
		private BaseBlock _onBlockExecuted;

		[SerializeField]
		private List<BaseParameter> _parameterList;

		[Header("Write")]
		[SerializeField]
		private BaseParameter _listenId;
	}
}
