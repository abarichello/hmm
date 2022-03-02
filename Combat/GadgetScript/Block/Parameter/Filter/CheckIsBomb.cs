using System;
using HeavyMetalMachines.Infra.Context;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block.Parameter.Filter
{
	public class CheckIsBomb : ScriptableObject, IParameterComparison
	{
		public bool Compare(object context)
		{
			CombatObject combatObject = this._parameter.GetValue<ICombatObject>(context) as CombatObject;
			return combatObject != null && combatObject.IsBomb;
		}

		[Restrict(true, new Type[]
		{
			typeof(ICombatObject)
		})]
		[SerializeField]
		private BaseParameter _parameter;
	}
}
