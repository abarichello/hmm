using System;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block.Input
{
	public class PressGadgetBlock : BaseBlock
	{
		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMGadgetContext ihmmgadgetContext = (IHMMGadgetContext)gadgetContext;
			if (ihmmgadgetContext.IsClient)
			{
				return this._nextBlock;
			}
			IGadgetInput gadgetInput = ihmmgadgetContext.Owner.GadgetCombatObject.GetGadgetInput(this._gadgetSlot);
			if (this._inputChange == InputChange.Press)
			{
				gadgetInput.ForcePressed();
			}
			else
			{
				gadgetInput.ForceReleased();
			}
			return this._nextBlock;
		}

		[SerializeField]
		private InputChange _inputChange;

		[SerializeField]
		private GadgetSlot _gadgetSlot;
	}
}
