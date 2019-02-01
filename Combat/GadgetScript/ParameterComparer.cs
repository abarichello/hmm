using System;
using Hoplon.GadgetScript;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	public static class ParameterComparer
	{
		public static bool CompareParameter(IParameterContext context, IParameterComparison[] comparisons, ParameterComparer.BooleanOperation booleanOperation)
		{
			bool flag = comparisons[0].Compare(context);
			for (int i = 1; i < comparisons.Length; i++)
			{
				if (booleanOperation == ParameterComparer.BooleanOperation.AND)
				{
					flag &= comparisons[i].Compare(context);
				}
				else
				{
					flag |= comparisons[i].Compare(context);
				}
			}
			return flag;
		}

		public enum BooleanOperation
		{
			AND,
			OR
		}
	}
}
