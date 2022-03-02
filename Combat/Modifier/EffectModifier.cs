using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat.GadgetScript;
using HeavyMetalMachines.Combat.GadgetScript.Block;
using HeavyMetalMachines.Infra.Context;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Modifier
{
	[CreateAssetMenu(menuName = "Modifier/EffectModifier")]
	public sealed class EffectModifier : BaseModifier
	{
		public override void Apply(ICombatObject causer, ICombatObject target, IHMMContext hmm)
		{
			if (hmm.IsClient)
			{
				return;
			}
			BaseGadget baseGadget = (BaseGadget)target.GetGadgetContext(25);
			if (this._hasCauserParameter)
			{
				this._causer.SetValue<ICombatObject>(baseGadget, causer);
			}
			this._target.SetValue<ICombatObject>(baseGadget, target);
			baseGadget.TriggerEvent(this._onModierAppliedBlock.Id);
		}

		protected override void OnEnable()
		{
			this._hasCauserParameter = (this._causer != null);
			if (null != this._onModierAppliedBlock)
			{
				EffectModifier.BlocksToInitialize.Enqueue(this._onModierAppliedBlock);
			}
		}

		public static readonly Queue<BaseBlock> BlocksToInitialize = new Queue<BaseBlock>();

		[SerializeField]
		private BaseBlock _onModierAppliedBlock;

		[SerializeField]
		private BaseParameter _causer;

		[SerializeField]
		private BaseParameter _target;

		private bool _hasCauserParameter;
	}
}
