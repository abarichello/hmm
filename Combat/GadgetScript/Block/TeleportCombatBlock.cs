using System;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/CombatObject/TeleportCombat")]
	public class TeleportCombatBlock : BaseBlock
	{
		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			if (((IHMMGadgetContext)gadgetContext).IsClient)
			{
				return this._nextBlock;
			}
			ICombatObject value = this._target.GetValue(gadgetContext);
			CombatMovement combatMovement = (CombatMovement)value.PhysicalObject;
			combatMovement.BreakAllLinks();
			combatMovement.ForcePosition(this._targetLocation.GetValue<Vector3>(gadgetContext), true);
			return this._nextBlock;
		}

		[Header("Read")]
		[SerializeField]
		private CombatObjectParameter _target;

		[SerializeField]
		private BaseParameter _targetLocation;
	}
}
