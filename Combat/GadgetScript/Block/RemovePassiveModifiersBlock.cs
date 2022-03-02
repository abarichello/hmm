using System;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Modifier/RemovePassiveModifier")]
	public class RemovePassiveModifiersBlock : BaseBlock
	{
		public override IBlock Execute(IGadgetContext context, IEventContext eventContext)
		{
			IHMMGadgetContext ihmmgadgetContext = (IHMMGadgetContext)context;
			if (ihmmgadgetContext.IsServer && this._target.GetValue<ICombatObject>(context) != null)
			{
				ICombatController modifierController = this._target.GetValue<ICombatObject>(context).ModifierController;
				modifierController.RemovePassiveModifiers(this._modifiers.GetValue<ModifierData[]>(context), ihmmgadgetContext.Owner as ICombatObject, -1);
			}
			return this._nextBlock;
		}

		[Restrict(true, new Type[]
		{
			typeof(ICombatObject)
		})]
		[SerializeField]
		private BaseParameter _target;

		[Restrict(true, new Type[]
		{
			typeof(ModifierData[])
		})]
		[SerializeField]
		private BaseParameter _modifiers;
	}
}
