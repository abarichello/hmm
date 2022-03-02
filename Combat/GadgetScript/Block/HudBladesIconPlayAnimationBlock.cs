using System;
using HeavyMetalMachines.Combat.Gadget.GadgetScript;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/HUD/Icon/Blades/Play Animation")]
	internal class HudBladesIconPlayAnimationBlock : BaseBlock
	{
		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			ICombatObject value = this._target.GetValue<ICombatObject>(gadgetContext);
			IHMMCombatGadgetContext ihmmcombatGadgetContext = (IHMMCombatGadgetContext)gadgetContext;
			if (ihmmcombatGadgetContext.IsServer)
			{
				((IHMMEventContext)eventContext).SendToClient();
			}
			else if (value != null && (ihmmcombatGadgetContext.Owner.IsLocalPlayer || value.IsLocalPlayer))
			{
				IHudIconBar hudIconBar = ihmmcombatGadgetContext.GetHudIconBar(value);
				hudIconBar.BladesIcon.PlayAnimation(this._animationName);
			}
			return this._nextBlock;
		}

		[Header("Read")]
		[SerializeField]
		private string _animationName;

		[SerializeField]
		private BaseParameter _target;
	}
}
