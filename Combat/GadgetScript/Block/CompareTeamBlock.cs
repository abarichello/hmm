using System;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Match;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[Obsolete("Obsolete! Use FilterBlock")]
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

		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
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

		[SerializeField]
		private BaseBlock _teamNotEqualBlock;

		[Header("Read")]
		[SerializeField]
		private ParameterComparer.BooleanOperation _booleanOperation;

		[SerializeField]
		private CompareTeamBlock.Comparison[] _comparisons;

		private static BoolParameter _resultParameter;

		[Serializable]
		private class Comparison : IParameterComparison
		{
			public bool Compare(object context)
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

			[Tooltip("This is only used if the Other Combat is not set.")]
			[SerializeField]
			private TeamKind _team;
		}
	}
}
