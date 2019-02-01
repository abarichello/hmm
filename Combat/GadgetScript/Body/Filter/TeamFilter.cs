using System;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Match;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Body.Filter
{
	public class TeamFilter : ScriptableObject, ICombatFilter
	{
		public bool Match(ICombatObject first, ICombatObject other, Collider col)
		{
			bool result = false;
			TeamFilter.Team team = this._team;
			if (team != TeamFilter.Team.Ally)
			{
				if (team != TeamFilter.Team.Enemy)
				{
					if (team == TeamFilter.Team.Neutral)
					{
						result = ((first != null && first.Team == TeamKind.Neutral) || first.Team == TeamKind.Zero);
					}
				}
				else
				{
					result = (first != null && first.Team != other.Team && ((CombatObject)first).IsPlayer);
				}
			}
			else
			{
				result = (first != null && first.Team == other.Team);
			}
			return result;
		}

		[SerializeField]
		private TeamFilter.Team _team;

		private enum Team
		{
			Ally,
			Enemy,
			Neutral
		}
	}
}
