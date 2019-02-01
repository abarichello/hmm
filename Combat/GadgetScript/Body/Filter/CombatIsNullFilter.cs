using System;
using HeavyMetalMachines.Infra.Context;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Body.Filter
{
	public class CombatIsNullFilter : ScriptableObject, ICombatFilter
	{
		public bool Match(ICombatObject first, ICombatObject other, Collider col)
		{
			return first == null;
		}
	}
}
