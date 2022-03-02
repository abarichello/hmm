using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	public abstract class Parameter<T> : TypedParameter<T>
	{
		protected override T InternalGetValue(object context)
		{
			return this._parameter.GetValue(context);
		}

		protected override void InternalSetValue(object context, T value)
		{
			this._parameter.SetValue(context, value);
		}

		protected override void InternalSetRoute(object context, Func<object, T> getter, Action<object, T> setter)
		{
			this._parameter.SetRoute(context, getter, setter);
		}

		public override IBaseParameterTomate ParameterTomate
		{
			get
			{
				return this._parameter;
			}
		}

		protected override void Initialize()
		{
			base.Initialize();
			if (base.ContentId > 0 && BaseParameter._parameters[base.ContentId].GetInstanceID() != base.GetInstanceID() && Application.isPlaying)
			{
				Parameter<T> parameter = (Parameter<T>)BaseParameter._parameters[base.ContentId];
				this._parameter = parameter._parameter;
				base.TouchedContexts = parameter.TouchedContexts;
				parameter.OnParameterValueUpdated += base.CallOnParameterValueUpdated;
				foreach (KeyValuePair<int, object> keyValuePair in base.TouchedContexts)
				{
					base.CallOnParameterValueUpdated(keyValuePair.Value);
				}
			}
			else if (this._sourceParameter != null && !string.IsNullOrEmpty(this._propertyName))
			{
				this._sourceParameter.OnParameterValueUpdated += this.OnSourceUpdated;
				this._parameter = new ParameterTomate<T>(this._defaultValue);
				foreach (KeyValuePair<int, object> keyValuePair2 in this._sourceParameter.TouchedContexts)
				{
					this.OnSourceUpdated(keyValuePair2.Value);
				}
			}
			else
			{
				this._parameter = new ParameterTomate<T>(this._defaultValue);
			}
			this._parameter.OnParameterValueChange += base.CallOnParameterValueUpdated;
		}

		protected override void Reset()
		{
			this._parameter = null;
			this._gettersCache.Clear();
			this._settersCache.Clear();
		}

		private void OnSourceUpdated(object context)
		{
			object boxedValue = this._sourceParameter.ParameterTomate.GetBoxedValue(context);
			object obj;
			if (boxedValue == null || (this._currentBountToObject.TryGetValue(context, out obj) && obj == boxedValue))
			{
				return;
			}
			this._currentBountToObject[context] = boxedValue;
			Func<T> getterDelegate = null;
			Action<T> setterDelegate = null;
			if (!this._gettersCache.TryGetValue(boxedValue, out getterDelegate))
			{
				PropertyInfo property = boxedValue.GetType().GetProperty(this._propertyName);
				getterDelegate = (Func<T>)Delegate.CreateDelegate(typeof(Func<T>), boxedValue, property.GetGetMethod());
				setterDelegate = delegate(T t)
				{
					this.NullSet(null, t);
				};
				if (property.GetSetMethod() != null)
				{
					setterDelegate = (Action<T>)Delegate.CreateDelegate(typeof(Action<T>), boxedValue, property.GetSetMethod());
				}
				this._gettersCache[boxedValue] = getterDelegate;
				this._settersCache[boxedValue] = setterDelegate;
			}
			else
			{
				setterDelegate = this._settersCache[boxedValue];
			}
			base.SetRoute(context, (object c) => getterDelegate(), delegate(object c, T t)
			{
				setterDelegate(t);
			});
		}

		protected ParameterTomate<T> _parameter;

		[SerializeField]
		private T _defaultValue;

		[SerializeField]
		private BaseParameter _sourceParameter;

		[HideInInspector]
		[SerializeField]
		private string _propertyName;

		private readonly Dictionary<object, Func<T>> _gettersCache = new Dictionary<object, Func<T>>();

		private readonly Dictionary<object, Action<T>> _settersCache = new Dictionary<object, Action<T>>();

		private readonly Dictionary<object, object> _currentBountToObject = new Dictionary<object, object>();
	}
}
