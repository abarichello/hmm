using System;
using System.Collections.Generic;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Modifier/ApplyPassiveModifier")]
	public sealed class ApplyPassiveModifiersBlock : BaseApplyModifierBlock
	{
		protected override void InternalInitialize(ref IList<BaseBlock> referencedBlocks, IHMMContext context)
		{
			base.InternalInitialize(ref referencedBlocks, context);
			this._modifiers = new ModifierData[this._modifierInfoContainer.Length];
		}

		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			this._numOfModsApplied = 0;
			IBlock result = base.Execute(gadgetContext, eventContext);
			this._modifiersParameter.SetValue<ModifierData[]>(gadgetContext, this._modifiers);
			return result;
		}

		protected override void ApplyModifier(ModifierData data, IHMMGadgetContext gadgetContext, ICombatController target)
		{
			target.AddPassiveModifier(data, (ICombatObject)gadgetContext.Owner, -1);
			this._modifiers[this._numOfModsApplied] = data;
			this._numOfModsApplied++;
		}

		[Header("Write")]
		[Restrict(true, new Type[]
		{
			typeof(ModifierData[])
		})]
		[SerializeField]
		private BaseParameter _modifiersParameter;

		private ModifierData[] _modifiers;

		private int _numOfModsApplied;
	}
}
