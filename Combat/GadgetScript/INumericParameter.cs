using System;
using Hoplon.GadgetScript;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	public interface INumericParameter
	{
		float GetFloatValue(IParameterContext context);

		void SetFloatValue(IParameterContext context, float value);
	}
}
