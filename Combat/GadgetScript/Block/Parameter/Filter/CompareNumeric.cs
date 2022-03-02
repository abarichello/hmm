using System;
using HeavyMetalMachines.Common;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block.Parameter.Filter
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Parameter/CompareNumericParameter")]
	public class CompareNumeric : ScriptableObject, IParameterComparison
	{
		public bool Compare(object context)
		{
			IParameterTomate<float> parameterTomate = this._parameter.ParameterTomate as IParameterTomate<float>;
			float value = parameterTomate.GetValue(context);
			float value2 = this._valueToCompare;
			if (this._otherParameter != null)
			{
				IParameterTomate<float> parameterTomate2 = this._otherParameter.ParameterTomate as IParameterTomate<float>;
				value2 = parameterTomate2.GetValue(context);
			}
			int num = value.CompareTo(value2);
			switch (this._comparisonType)
			{
			case ComparisonType.Equal:
				return num == 0;
			case ComparisonType.Greater:
				return num > 0;
			case ComparisonType.Lesser:
				return num < 0;
			case ComparisonType.EqualOrGreater:
				return num >= 0;
			case ComparisonType.EqualOrLesser:
				return num <= 0;
			default:
				return false;
			}
		}

		[SerializeField]
		private ComparisonType _comparisonType;

		[Restrict(true, new Type[]
		{
			typeof(float)
		})]
		[SerializeField]
		private BaseParameter _parameter;

		[Restrict(false, new Type[]
		{
			typeof(float)
		})]
		[SerializeField]
		private BaseParameter _otherParameter;

		[Tooltip("Used if Other Parameter is not set")]
		[SerializeField]
		private float _valueToCompare;
	}
}
