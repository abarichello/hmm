using System;
using HeavyMetalMachines.Combat.Gadget.GadgetScript;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/HUD/SetRadialTimer")]
	internal class HudSetRadialTimerBlock : BaseBlock
	{
		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMCombatGadgetContext ihmmcombatGadgetContext = (IHMMCombatGadgetContext)gadgetContext;
			if (ihmmcombatGadgetContext.IsServer)
			{
				((IHMMEventContext)eventContext).SendToClient();
			}
			else if (ihmmcombatGadgetContext.Owner.IsLocalPlayer)
			{
				ihmmcombatGadgetContext.GadgetHudElement.Radial.SetTimer(this._totalTimeParameter.GetValue<float>(gadgetContext), ihmmcombatGadgetContext);
			}
			return this._nextBlock;
		}

		[Header("Read")]
		[SerializeField]
		[Restrict(true, new Type[]
		{
			typeof(float)
		})]
		private BaseParameter _totalTimeParameter;
	}
}
