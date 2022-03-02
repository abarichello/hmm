using System;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Camera/CameraTeleportTrigger")]
	public class CameraTeleportTriggerBlock : BaseBlock
	{
		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMGadgetContext ihmmgadgetContext = (IHMMGadgetContext)gadgetContext;
			if (ihmmgadgetContext.IsClient && ihmmgadgetContext.Owner.IsLocalPlayer)
			{
				ihmmgadgetContext.GameCamera.TriggerTeleport();
			}
			return this._nextBlock;
		}
	}
}
