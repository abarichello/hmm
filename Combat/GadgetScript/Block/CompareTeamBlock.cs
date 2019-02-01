using System;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Match;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Parameter/CompareTeam")]
	public class CompareTeamBlock : BaseBlock
	{
		protected override void OnEnable()
		{
			base.OnEnable();
			if (CompareTeamBlock._resultParameter == null)
			{
				CompareTeamBlock._resultParameter = ScriptableObject.CreateInstance<BoolParameter>();
			}
		}

		protected override bool CheckSanity(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			return !((IHMMGadgetContext)gadgetContext).IsClient || true;
		}

		protected override IBlock InnerExecute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMEventContext ihmmeventContext = (IHMMEventContext)eventContext;
			if (((IHMMGadgetContext)gadgetContext).IsServer)
			{
				CompareTeamBlock._resultParameter.SetValue(gadgetContext, ParameterComparer.CompareParameter(gadgetContext, this._comparisons, this._booleanOperation));
				ihmmeventContext.SaveParameter(CompareTeamBlock._resultParameter);
			}
			else
			{
				ihmmeventContext.LoadParameter(CompareTeamBlock._resultParameter);
			}
			if (CompareTeamBlock._resultParameter.GetValue(gadgetContext))
			{
				return this._nextBlock;
			}
			return this._teamNotEqualBlock;
		}

		public override bool UsesParameterWithId(int parameterId)
		{
			return false;
		}

		[SerializeField]
		private BaseBlock _teamNotEqualBlock;

		[Header("Read")]
		[SerializeField]
		private ParameterComparer.BooleanOperation _booleanOperation;

		[SerializeField]
		private CompareTeamBlock.Comparison[] _comparisons;

		private static BoolParameter _resultParameter;

		[Serializable]
		private class Comparison : IParameterComparison, IUsedParametersArray
		{
			public bool Compare(IParameterContext context)
			{
				bool result = false;
				ICombatObject value = this._combat.GetValue(context);
				if (value != null)
				{
					if (this._otherCombat != null)
					{
						ICombatObject value2 = this._otherCombat.GetValue(context);
						result = (value2 != null && value.Team == this._otherCombat.GetValue(context).Team);
					}
					else
					{
						result = (value.Team == this._team);
					}
				}
				return result;
			}

			public BaseParameter[] GetParameterArray()
			{
				return new BaseParameter[]
				{
					this._combat,
					this._otherCombat
				};
			}

			[SerializeField]
			private CombatObjectParameter _combat;

			[SerializeField]
			private CombatObjectParameter _otherCombat;

			[SerializeField]
			[Tooltip("This is only used if the Other Combat is not set.")]
			private TeamKind _team;
		}
	}
}
