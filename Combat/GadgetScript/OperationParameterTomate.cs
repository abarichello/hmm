using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	public class OperationParameterTomate<TValue, TFirst, TSecond> : IParameterTomate<TValue>, IBaseParameterTomate
	{
		public OperationParameterTomate(IParameterTomate<TFirst> firstParameter, IParameterTomate<TSecond> secondParameter, Func<TFirst, TSecond, TValue> operation)
		{
			this._firstParameter = firstParameter;
			this._secondParameter = secondParameter;
			this._operationFunction = operation;
			firstParameter.OnParameterValueChange += this.NotifyChange;
			secondParameter.OnParameterValueChange += this.NotifyChange;
		}

		public OperationParameterTomate(IParameterTomate<TFirst> firstParameter, Func<TFirst, TValue> operation)
		{
			this._firstParameter = firstParameter;
			this._unaryOperationFunction = operation;
			firstParameter.OnParameterValueChange += this.NotifyChange;
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event IParamaterValueChange OnParameterValueChange;

		public object GetBoxedValue(object context)
		{
			return this.GetValue(context);
		}

		public TValue GetValue(object context)
		{
			Func<object, TValue> func;
			if (this._getters.TryGetValue(context, out func))
			{
				return func(context);
			}
			TFirst value = this._firstParameter.GetValue(context);
			if (this._operationFunction != null)
			{
				TSecond value2 = this._secondParameter.GetValue(context);
				return this._operationFunction(value, value2);
			}
			return this._unaryOperationFunction(value);
		}

		public void SetValue(object context, TValue value)
		{
		}

		public void SetRoute(object context, Func<object, TValue> getter, Action<object, TValue> setter)
		{
			this._getters[context] = getter;
		}

		private void NotifyChange(object context)
		{
			if (this.OnParameterValueChange != null)
			{
				this.OnParameterValueChange(context);
			}
		}

		private Func<TFirst, TValue> _unaryOperationFunction;

		private Func<TFirst, TSecond, TValue> _operationFunction;

		public readonly IParameterTomate<TFirst> _firstParameter;

		public readonly IParameterTomate<TSecond> _secondParameter;

		private readonly IDictionary<object, Func<object, TValue>> _getters = new Dictionary<object, Func<object, TValue>>();
	}
}
