using System;
using System.Collections.Generic;
using System.Reflection;
using Hoplon.GadgetScript;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	public abstract class Parameter<T> : BaseParameter, IParameter<T>
	{
		public T GetValue(IParameterContext context)
		{
			if (!this._getters.ContainsKey(context))
			{
				this._getters[context] = (() => this.GetInternalValue(context));
			}
			return this._getters[context]();
		}

		public void SetValue(IParameterContext context, T value)
		{
			if (!this._setters.ContainsKey(context))
			{
				this._setters[context] = delegate(T newValue)
				{
					this.SetInternalValue(context, newValue);
				};
			}
			if (this._setters[context] == null)
			{
				Parameter<T>.Log.FatalFormat("{0} Trying to set value to a read-only re-routed parameter", new object[]
				{
					base.name
				});
				return;
			}
			this._setters[context](value);
			base.CallOnParameterValueUpdated(context);
		}

		public void SetRoute(IParameterContext context, Func<T> getter, Action<T> setter)
		{
			this._getters[context] = getter;
			this._setters[context] = setter;
			base.CallOnParameterValueUpdated(context);
		}

		public override bool IsRoutedToObject
		{
			get
			{
				return this._routeSourceObjectGetter != null;
			}
		}

		public override void CreateRouteToObject(IParameterContext context, MethodInfo getter, MethodInfo setter, object boundToObject)
		{
			Func<T> getter2 = (Func<T>)Delegate.CreateDelegate(typeof(Func<T>), boundToObject, getter);
			Action<T> setter2 = null;
			if (setter != null)
			{
				setter2 = (Action<T>)Delegate.CreateDelegate(typeof(Action<T>), boundToObject, setter);
			}
			this.SetRoute(context, getter2, setter2);
		}

		public override void RouteContext(IParameterContext context, IParameterContext otherContext)
		{
			this.SetRoute(context, () => this.GetValue(otherContext), delegate(T value)
			{
				this.SetValue(otherContext, value);
			});
		}

		public override void SetTo(IParameterContext context, BaseParameter other)
		{
			this.SetValue(context, ((IParameter<T>)other).GetValue(context));
		}

		public override int CompareTo(IParameterContext context, BaseParameter other)
		{
			IComparable comparable = this.GetValue(context) as IComparable;
			if (comparable != null)
			{
				IParameter<T> parameter = (IParameter<T>)other;
				return comparable.CompareTo(parameter.GetValue(context));
			}
			Parameter<T>.Log.ErrorFormat("Trying to compare parameters that are not comparable: {0} with {1}", new object[]
			{
				base.name,
				other.name
			});
			return -1;
		}

		public override IParameterContext[] GetAllSetContexts()
		{
			return new List<IParameterContext>(this._getters.Keys).ToArray();
		}

		private T GetInternalValue(IParameterContext context)
		{
			T result = this._defaultValue;
			if (this._values.ContainsKey(context))
			{
				result = this._values[context];
			}
			return result;
		}

		private void SetInternalValue(IParameterContext context, T value)
		{
			this._values[context] = value;
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			if (this._sourceParameter != null && !string.IsNullOrEmpty(this._propertyName))
			{
				this._routeSourceObjectGetter = this._sourceParameter.GetType().GetMethod("GetValue");
				this._sourceParameter.OnParameterValueUpdated += this.OnSourceUpdated;
				IParameterContext[] allSetContexts = this._sourceParameter.GetAllSetContexts();
				for (int i = 0; i < allSetContexts.Length; i++)
				{
					this.OnSourceUpdated(this._sourceParameter, allSetContexts[i]);
				}
			}
		}

		protected override void Reset()
		{
			this._values.Clear();
			this._setters.Clear();
			this._getters.Clear();
		}

		private void OnSourceUpdated(BaseParameter parameter, IParameterContext context)
		{
			object obj = this._routeSourceObjectGetter.Invoke(parameter, new object[]
			{
				context
			});
			if (obj != null)
			{
				PropertyInfo property = obj.GetType().GetProperty(this._propertyName);
				this.CreateRouteToObject(context, property.GetGetMethod(), property.GetSetMethod(), obj);
			}
		}

		[SerializeField]
		private T _defaultValue;

		[SerializeField]
		private BaseParameter _sourceParameter;

		[HideInInspector]
		[SerializeField]
		private string _propertyName;

		private readonly Dictionary<IParameterContext, T> _values = new Dictionary<IParameterContext, T>();

		private readonly Dictionary<IParameterContext, Func<T>> _getters = new Dictionary<IParameterContext, Func<T>>();

		private readonly Dictionary<IParameterContext, Action<T>> _setters = new Dictionary<IParameterContext, Action<T>>();

		private MethodInfo _routeSourceObjectGetter;

		private new static readonly BitLogger Log = new BitLogger(typeof(Parameter<T>));
	}
}
