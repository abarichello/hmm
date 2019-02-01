using System;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Modifier/RemovePassiveModifier")]
	public class RemovePassiveModifiersBlock : BaseBlock
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
			return true;
		}

		protected override IBlock InnerExecute(IGadgetContext context, IEventContext eventContext)
		{
			IHMMGadgetContext ihmmgadgetContext = (IHMMGadgetContext)context;
			if (ihmmgadgetContext.IsServer && this._target.GetValue(context) != null)
			{
				ICombatController modifierController = this._target.GetValue(context).ModifierController;
				modifierController.RemovePassiveModifiers(this._modifiers.GetValue(context), ihmmgadgetContext.GetCombatObject(ihmmgadgetContext.OwnerId), -1);
			}
			return this._nextBlock;
		}

		public override bool UsesParameterWithId(int parameterId)
		{
			return base.CheckIsParameterWithId(this._target, parameterId);
		}

		[SerializeField]
		private CombatObjectParameter _target;

		[SerializeField]
		private ModifierDataArrayParameter _modifiers;
	}
}
