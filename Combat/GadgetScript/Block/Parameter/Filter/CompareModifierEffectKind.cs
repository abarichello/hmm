using System;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block.Parameter.Filter
{
	public class CompareModifierEffectKind : ScriptableObject, IParameterComparison
	{
		public bool Compare(object context)
		{
			return this._effectKind == this._parameter.GetValue<ModifierData>(context).Info.Effect;
		}

		[Restrict(true, new Type[]
		{
			typeof(ModifierData)
		})]
		[SerializeField]
		private BaseParameter _parameter;

		[SerializeField]
		private EffectKind _effectKind;
	}
}
