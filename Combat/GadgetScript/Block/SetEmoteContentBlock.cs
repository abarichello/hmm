using System;
using HeavyMetalMachines.Combat.Gadget.GadgetScript;
using HeavyMetalMachines.Frontend;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/HUD/Emote/Set Content")]
	public class SetEmoteContentBlock : BaseBlock
	{
		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMCombatGadgetContext ihmmcombatGadgetContext = (IHMMCombatGadgetContext)gadgetContext;
			if (ihmmcombatGadgetContext.IsServer)
			{
				((IHMMEventContext)eventContext).SendToClient();
			}
			else
			{
				IHudEmotePresenter hudEmoteView = ihmmcombatGadgetContext.HudEmoteView;
				hudEmoteView.PlayEmote(ihmmcombatGadgetContext.Slot, ihmmcombatGadgetContext.Owner.Identifiable.ObjId);
			}
			return this._nextBlock;
		}
	}
}
