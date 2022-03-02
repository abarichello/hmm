using System;
using HeavyMetalMachines.Combat.Gadget.GadgetScript;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Stats/Register Gadget Activation")]
	public class RegisterGadgetActivationBlock : BaseBlock
	{
		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMCombatGadgetContext ihmmcombatGadgetContext = (IHMMCombatGadgetContext)gadgetContext;
			if (ihmmcombatGadgetContext.IsServer)
			{
				ICombatObject combatObject = ihmmcombatGadgetContext.GetCombatObject(ihmmcombatGadgetContext.Owner.Identifiable.ObjId);
				combatObject.Stats.RegisterGadgetActivation(ihmmcombatGadgetContext.Slot);
			}
			return this._nextBlock;
		}
	}
}
