using System;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/CombatObject/TeleportCombat")]
	public class TeleportCombatBlock : BaseBlock
	{
		protected override bool CheckSanity(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			if (((IHMMGadgetContext)gadgetContext).IsClient)
			{
				return true;
			}
			if (this._target == null)
			{
				base.LogSanitycheckError("'Target' parameter cannot be null.");
				return false;
			}
			if (this._targetLocation == null)
			{
				base.LogSanitycheckError("'Target Location' parameter cannot be null.");
				return false;
			}
			return true;
		}

		protected override IBlock InnerExecute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			if (((IHMMGadgetContext)gadgetContext).IsClient)
			{
				return this._nextBlock;
			}
			ICombatObject value = this._target.GetValue(gadgetContext);
			CombatMovement combatMovement = (CombatMovement)value.PhysicalObject;
			combatMovement.BreakAllLinks();
			combatMovement.ForcePosition(this._targetLocation.GetValue(gadgetContext), true);
			return this._nextBlock;
		}

		public override bool UsesParameterWithId(int parameterId)
		{
			return base.CheckIsParameterWithId(this._target, parameterId) || base.CheckIsParameterWithId(this._targetLocation, parameterId);
		}

		[Header("Read")]
		[SerializeField]
		private CombatObjectParameter _target;

		[SerializeField]
		private Vector3Parameter _targetLocation;
	}
}
