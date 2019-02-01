using System;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Bomb/BreakBombLink")]
	public class BreakBombLinkBlock : BaseBlock
	{
		protected override bool CheckSanity(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			if (this._combatObject == null)
			{
				base.LogSanitycheckError("'Combat Object' parameter cannot be null.");
				return false;
			}
			return true;
		}

		protected override IBlock InnerExecute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			ICombatObject value = this._combatObject.GetValue(gadgetContext);
			value.BreakBombLink();
			return this._nextBlock;
		}

		public override bool UsesParameterWithId(int parameterId)
		{
			return base.CheckIsParameterWithId(this._combatObject, parameterId);
		}

		[Header("Read")]
		[SerializeField]
		private CombatObjectParameter _combatObject;
	}
}
