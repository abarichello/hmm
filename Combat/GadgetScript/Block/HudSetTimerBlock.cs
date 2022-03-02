using System;
using HeavyMetalMachines.Combat.Gadget.GadgetScript;
using Hoplon.GadgetScript;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/HUD/SetTimer")]
	internal class HudSetTimerBlock : BaseBlock
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
				ihmmcombatGadgetContext.GadgetHudElement.Timer.SetTimer((long)this._timeParameter.GetValue<float>(gadgetContext) * 1000L + (long)GameHubBehaviour.Hub.Clock.GetPlaybackTime(), ihmmcombatGadgetContext);
			}
			return this._nextBlock;
		}

		[Header("Read")]
		[SerializeField]
		[Restrict(true, new Type[]
		{
			typeof(float)
		})]
		private BaseParameter _timeParameter;
	}
}
