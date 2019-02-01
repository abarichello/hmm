using System;
using Hoplon.GadgetScript;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	public interface IParameterComparison
	{
		bool Compare(IParameterContext context);
	}
}
