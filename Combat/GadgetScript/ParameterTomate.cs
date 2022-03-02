using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	public class ParameterTomate<TValue> : IParameterTomate<TValue>, IBaseParameterTomate
	{
		public ParameterTomate(TValue defaultValue)
		{
			this._defaultValue = defaultValue;
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
			if (!this._getters.TryGetValue(context, out func))
			{
				func = ((object c) => this._defaultValue);
				this._getters[context] = func;
				this.NotifyChange(context);
			}
			return func(context);
		}

		public void SetValue(object context, TValue value)
		{
			Action<object, TValue> action;
			if (!this._setters.TryGetValue(context, out action))
			{
				this._getters[context] = ((object c) => this._constantValues[c]);
				action = delegate(object c, TValue newValue)
				{
					this._constantValues[c] = newValue;
				};
				this._setters[context] = action;
			}
			action(context, value);
			this.NotifyChange(context);
		}

		public void SetRoute(object context, Func<object, TValue> getter, Action<object, TValue> setter)
		{
			this._getters[context] = getter;
			this._setters[context] = setter;
			this.NotifyChange(context);
		}

		private void NotifyChange(object context)
		{
			if (this.OnParameterValueChange != null)
			{
				this.OnParameterValueChange(context);
			}
		}

		private readonly IDictionary<object, Func<object, TValue>> _getters = new Dictionary<object, Func<object, TValue>>();

		private readonly IDictionary<object, Action<object, TValue>> _setters = new Dictionary<object, Action<object, TValue>>();

		private readonly Dictionary<object, TValue> _constantValues = new Dictionary<object, TValue>();

		private readonly TValue _defaultValue;
	}
}
