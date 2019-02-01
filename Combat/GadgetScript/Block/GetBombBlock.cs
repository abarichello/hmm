using System;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Bomb/GetBomb")]
	public class GetBombBlock : BaseBlock
	{
		protected override bool CheckSanity(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			if (this._bombCombatObject == null)
			{
				base.LogSanitycheckError("'Bomb Combat Object' parameter cannot be null.");
				return false;
			}
			return true;
		}

		protected override IBlock InnerExecute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMGadgetContext ihmmgadgetContext = (IHMMGadgetContext)gadgetContext;
			this._bombCombatObject.SetValue(gadgetContext, ihmmgadgetContext.Bomb);
			return this._nextBlock;
		}

		public override bool UsesParameterWithId(int parameterId)
		{
			return base.CheckIsParameterWithId(this._bombCombatObject, parameterId);
		}

		[Header("Write")]
		[SerializeField]
		private CombatObjectParameter _bombCombatObject;
	}
}
