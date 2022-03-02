using System;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block.Parameter.Filter
{
	public class CompareBool : ScriptableObject, IParameterComparison
	{
		public bool Compare(object context)
		{
			return this._parameter.GetValue<bool>(context) == this._valueToCompare;
		}

		[Restrict(true, new Type[]
		{
			typeof(bool)
		})]
		[SerializeField]
		private BaseParameter _parameter;

		[SerializeField]
		private bool _valueToCompare;
	}
}
