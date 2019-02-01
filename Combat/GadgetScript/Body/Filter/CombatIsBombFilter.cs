using System;
using HeavyMetalMachines.Infra.Context;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Body.Filter
{
	public class CombatIsBombFilter : ScriptableObject, ICombatFilter
	{
		public bool Match(ICombatObject first, ICombatObject other, Collider col)
		{
			return ((CombatObject)first).IsBomb;
		}
	}
}
