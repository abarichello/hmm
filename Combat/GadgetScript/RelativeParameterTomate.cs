using System;
using System.Diagnostics;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	public class RelativeParameterTomate<TValue> : IParameterTomate<TValue>, IBaseParameterTomate
	{
		public RelativeParameterTomate(IBaseParameterTomate key, TValue defaultValue)
		{
			this._keyParameter = key;
			this._valueParameter = new ParameterTomate<TValue>(defaultValue);
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event IParamaterValueChange OnParameterValueChange;

		public object GetBoxedValue(object context)
		{
			object boxedValue = this._keyParameter.GetBoxedValue(context);
			if (boxedValue == null)
			{
				return default(TValue);
			}
			return this._valueParameter.GetBoxedValue(boxedValue);
		}

		public TValue GetValue(object context)
		{
			object boxedValue = this._keyParameter.GetBoxedValue(context);
			if (boxedValue == null)
			{
				return default(TValue);
			}
			return this._valueParameter.GetValue(boxedValue);
		}

		public void SetValue(object context, TValue value)
		{
			object boxedValue = this._keyParameter.GetBoxedValue(context);
			if (boxedValue != null)
			{
				this._valueParameter.SetValue(boxedValue, value);
			}
			this.NotifyChange(context);
		}

		public void SetRoute(object context, Func<object, TValue> getter, Action<object, TValue> setter)
		{
			object boxedValue = this._keyParameter.GetBoxedValue(context);
			if (boxedValue != null)
			{
				this._valueParameter.SetRoute(boxedValue, getter, setter);
			}
			this.NotifyChange(context);
		}

		private void NotifyChange(object context)
		{
			if (this.OnParameterValueChange != null)
			{
				this.OnParameterValueChange(context);
			}
		}

		private readonly IBaseParameterTomate _keyParameter;

		private readonly IParameterTomate<TValue> _valueParameter;
	}
}
