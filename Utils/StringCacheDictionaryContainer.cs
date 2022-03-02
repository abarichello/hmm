using System;
using System.Collections.Generic;

namespace HeavyMetalMachines.Utils
{
	public class StringCacheDictionaryContainer<TKey> : IStringCacheContainer<TKey>
	{
		public StringCacheDictionaryContainer(int capacity)
		{
			this._container = new Dictionary<TKey, string>();
		}

		public void Add(TKey key, string value)
		{
			this._container.Add(key, value);
		}

		public bool TryGetValue(TKey key, out string value)
		{
			return this._container.TryGetValue(key, out value);
		}

		public string Get(TKey key)
		{
			return this._container[key];
		}

		private readonly Dictionary<TKey, string> _container;
	}
}
