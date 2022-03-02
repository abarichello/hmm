using System;
using HeavyMetalMachines.Combat.GadgetScript;

namespace HeavyMetalMachines.Infra.Context
{
	public static class HmmContextExtensions
	{
		public static void LoadParameterIfExisting(this IHMMEventContext eventContext, BaseParameter parameter)
		{
			if (parameter != null)
			{
				eventContext.LoadParameter(parameter);
			}
		}

		public static void SaveParameterIfExisting(this IHMMEventContext eventContext, BaseParameter parameter)
		{
			if (parameter != null)
			{
				eventContext.SaveParameter(parameter);
			}
		}
	}
}
