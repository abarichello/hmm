using System;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block.Parameter.Filter
{
	public class CompareIdentity : ScriptableObject, IParameterComparison
	{
		public bool Compare(object context)
		{
			return this._firstParameter.ParameterTomate.GetBoxedValue(context) == this._secondParameter.ParameterTomate.GetBoxedValue(context);
		}

		[Restrict(true, new Type[]
		{

		})]
		[SerializeField]
		private BaseParameter _firstParameter;

		[Restrict(true, new Type[]
		{

		})]
		[SerializeField]
		private BaseParameter _secondParameter;
	}
}
