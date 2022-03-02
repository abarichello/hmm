using System;
using HeavyMetalMachines.Combat.Gadget.GadgetScript;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/HUD/Gauge/Play Animation")]
	public class HudGaugePlayAnimationBlock : BaseBlock
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
				ihmmcombatGadgetContext.GadgetHudElement.Gauge.PlayAnimation(this._animationName);
			}
			return this._nextBlock;
		}

		[SerializeField]
		private string _animationName;
	}
}
