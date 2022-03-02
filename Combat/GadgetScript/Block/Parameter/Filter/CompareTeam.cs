using System;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Match;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block.Parameter.Filter
{
	public class CompareTeam : ScriptableObject, IParameterComparison
	{
		public bool Compare(object context)
		{
			bool flag = false;
			ICombatObject value = this._combat.GetValue<ICombatObject>(context);
			if (value != null)
			{
				if (this._otherCombat != null)
				{
					ICombatObject value2 = this._otherCombat.GetValue<ICombatObject>(context);
					flag = (value2 != null && value.Team == this._otherCombat.GetValue<ICombatObject>(context).Team);
				}
				else
				{
					flag = (value.Team == this._team);
				}
			}
			if (this._teamComparison == CompareTeam.TeamComparisonType.DifferentTeam)
			{
				flag = !flag;
			}
			return flag;
		}

		[Restrict(true, new Type[]
		{
			typeof(ICombatObject)
		})]
		[SerializeField]
		private BaseParameter _combat;

		[SerializeField]
		private CompareTeam.TeamComparisonType _teamComparison;

		[Restrict(false, new Type[]
		{
			typeof(ICombatObject)
		})]
		[SerializeField]
		private BaseParameter _otherCombat;

		[Tooltip("This is only used if the Other Combat is not set.")]
		[SerializeField]
		private TeamKind _team;

		private enum TeamComparisonType
		{
			SameTeam,
			DifferentTeam
		}
	}
}
