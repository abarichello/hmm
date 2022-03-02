using System;
using System.Collections.Generic;

namespace HeavyMetalMachines.Presenting
{
	public class ViewProvider : IViewProvider
	{
		public void Bind<T>(T view, string context = null)
		{
			ViewProvider.ViewProviderKey key = this.GetKey(typeof(T), context);
			this._views.Add(key, view);
		}

		public void Unbind<T>(string context = null)
		{
			ViewProvider.ViewProviderKey key = this.GetKey(typeof(T), context);
			this._views.Remove(key);
		}

		public T Provide<T>(string context = null) where T : class
		{
			ViewProvider.ViewProviderKey key = this.GetKey(typeof(T), context);
			return (T)((object)this._views[key]);
		}

		public bool TryProvide<T>(out T view, string context = null) where T : class
		{
			if (context == null)
			{
				context = string.Empty;
			}
			foreach (ViewProvider.ViewProviderKey viewProviderKey in this._views.Keys)
			{
				if (viewProviderKey.T == typeof(T) && string.Equals(viewProviderKey.Context, context))
				{
					view = (T)((object)this._views[viewProviderKey]);
					return true;
				}
			}
			view = (T)((object)null);
			return false;
		}

		private ViewProvider.ViewProviderKey GetKey(Type type, string context)
		{
			if (context == null)
			{
				context = string.Empty;
			}
			return new ViewProvider.ViewProviderKey(type, context);
		}

		private readonly Dictionary<ViewProvider.ViewProviderKey, object> _views = new Dictionary<ViewProvider.ViewProviderKey, object>();

		private class ViewProviderKey
		{
			public ViewProviderKey(Type type, string context)
			{
				this.T = type;
				this.Context = context;
			}

			public override bool Equals(object obj)
			{
				ViewProvider.ViewProviderKey other = (ViewProvider.ViewProviderKey)obj;
				return this.Equals(other);
			}

			protected bool Equals(ViewProvider.ViewProviderKey other)
			{
				return other != null && this.T == other.T && string.Equals(this.Context, other.Context);
			}

			public override int GetHashCode()
			{
				int num = 0;
				if (this.T != null)
				{
					num += this.T.GetHashCode() * 397;
				}
				if (this.Context != null)
				{
					num += this.Context.GetHashCode();
				}
				return num;
			}

			public override string ToString()
			{
				if (string.IsNullOrEmpty(this.Context))
				{
					return this.T.Name;
				}
				return string.Format("{0} ({1}", this.T.Name, this.Context);
			}

			public readonly Type T;

			public readonly string Context;
		}
	}
}
