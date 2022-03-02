using System;
using HeavyMetalMachines.Combat.Gadget.GadgetScript;
using HeavyMetalMachines.Utils;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/HUD/PlayAnimation")]
	internal class HudPlayAnimationBlock : BaseBlock
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
				for (int i = 0; i < ihmmcombatGadgetContext.GadgetHudElement.AnimationControllers.Length; i++)
				{
					if (ihmmcombatGadgetContext.GadgetHudElement.AnimationControllers[i].GetClip(this._animationName))
					{
						GUIUtils.PlayAnimation(ihmmcombatGadgetContext.GadgetHudElement.AnimationControllers[i], this._reverse, (float)this._animationSpeed, this._animationName);
						break;
					}
				}
			}
			return this._nextBlock;
		}

		[Header("Read")]
		[SerializeField]
		private string _animationName;

		[SerializeField]
		private int _animationSpeed = 1;

		[SerializeField]
		private bool _reverse;
	}
}
