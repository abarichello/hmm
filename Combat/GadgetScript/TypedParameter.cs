using System;
using Hoplon.GadgetScript;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	public abstract class TypedParameter<TValue> : BaseParameter, IParameter<TValue>
	{
		protected abstract TValue InternalGetValue(object context);

		protected abstract void InternalSetValue(object context, TValue value);

		protected abstract void InternalSetRoute(object context, Func<object, TValue> getter, Action<object, TValue> setter);

		public TValue GetValue(object context)
		{
			base.TouchContext(context);
			return this.InternalGetValue(context);
		}

		public void SetValue(object context, TValue value)
		{
			base.TouchContext(context);
			this.InternalSetValue(context, value);
			base.CallOnParameterValueUpdated(context);
		}

		public void SetRoute(object context, Func<object, TValue> getter, Action<object, TValue> setter)
		{
			base.TouchContext(context);
			this.InternalSetRoute(context, getter, setter);
			base.CallOnParameterValueUpdated(context);
		}

		public sealed override void SetTo(object context, BaseParameter other)
		{
			this.SetValue(context, ((IParameter<TValue>)other).GetValue(context));
		}

		public override int CompareTo(object context, BaseParameter other)
		{
			IComparable comparable = this.GetValue(context) as IComparable;
			if (comparable != null)
			{
				IParameter<TValue> parameter = (IParameter<TValue>)other;
				return comparable.CompareTo(parameter.GetValue(context));
			}
			BaseParameter.Log.ErrorFormat("Trying to compare parameters that are not comparable: {0} with {1}", new object[]
			{
				base.name,
				other.name
			});
			return -1;
		}

		public sealed override void RouteContext(object context, object otherContext)
		{
			this.SetRoute(context, (object c) => this.GetValue(otherContext), delegate(object c, TValue value)
			{
				this.SetValue(otherContext, value);
			});
		}

		protected void NullSet(object o, TValue value)
		{
		}
	}
}
