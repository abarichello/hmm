using System;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	public static class ParameterComparer
	{
		public static bool CompareParameter(object context, IParameterComparison[] comparisons, ParameterComparer.BooleanOperation booleanOperation)
		{
			bool flag = true;
			if (booleanOperation == ParameterComparer.BooleanOperation.AND)
			{
				flag = false;
			}
			for (int i = 0; i < comparisons.Length; i++)
			{
				if (comparisons[i].Compare(context) == flag)
				{
					return flag;
				}
			}
			return !flag;
		}

		public enum BooleanOperation
		{
			AND,
			OR
		}
	}
}
