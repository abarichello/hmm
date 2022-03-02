using System;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[Obsolete("Obsolete! Use Filter Block")]
	[CreateAssetMenu(menuName = "GadgetScript/Block/Modifier/CheckStatusBlock")]
	public class CheckStatusBlock : BaseBlock
	{
		protected override void OnEnable()
		{
			base.OnEnable();
			if (CheckStatusBlock._resultParameter == null)
			{
				CheckStatusBlock._resultParameter = ScriptableObject.CreateInstance<BoolParameter>();
			}
		}

		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMEventContext ihmmeventContext = (IHMMEventContext)eventContext;
			if (((IHMMGadgetContext)gadgetContext).IsServer)
			{
				CheckStatusBlock._resultParameter.SetValue(gadgetContext, ParameterComparer.CompareParameter(gadgetContext, this._comparisons, this._booleanOperation));
				ihmmeventContext.SaveParameter(CheckStatusBlock._resultParameter);
			}
			else
			{
				ihmmeventContext.LoadParameter(CheckStatusBlock._resultParameter);
			}
			if (CheckStatusBlock._resultParameter.GetValue(gadgetContext))
			{
				return this._nextBlock;
			}
			return this._failureBlock;
		}

		[SerializeField]
		private BaseBlock _failureBlock;

		[Header("Read")]
		[SerializeField]
		private ParameterComparer.BooleanOperation _booleanOperation;

		[SerializeField]
		private CheckStatusBlock.HasFlagComparison[] _comparisons;

		private static BoolParameter _resultParameter;

		[Serializable]
		private class HasFlagComparison : IParameterComparison
		{
			public bool Compare(object context)
			{
				ICombatObject value = this._targetCombat.GetValue(context);
				return value.Attributes.CurrentStatus.HasFlag(this._status);
			}

			[SerializeField]
			private StatusKind _status;

			[SerializeField]
			private CombatObjectParameter _targetCombat;
		}
	}
}
