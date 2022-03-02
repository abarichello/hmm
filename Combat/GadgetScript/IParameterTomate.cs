using System;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	public interface IParameterTomate<TValue> : IBaseParameterTomate
	{
		TValue GetValue(object context);

		void SetValue(object context, TValue value);

		void SetRoute(object context, Func<object, TValue> getter, Action<object, TValue> setter);

		event IParamaterValueChange OnParameterValueChange;
	}
}
