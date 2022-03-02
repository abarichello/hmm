using System;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Combat.Gadget.GadgetScript;
using HeavyMetalMachines.Frontend;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/HUD/Change Sprite In Gadget")]
	internal class HudChangeSpriteInGadgetBlock : BaseBlock
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
				IGadgetHudElement gadgetHudElement = ihmmcombatGadgetContext.GetGadgetHudElement(this._slot);
				gadgetHudElement.SpriteController.SpriteName = this._spriteName;
			}
			return this._nextBlock;
		}

		[Header("Read")]
		[SerializeField]
		private string _spriteName;

		[SerializeField]
		private GadgetSlot _slot;
	}
}
