using System;

namespace HeavyMetalMachines.Utils
{
	public class StringCache<TKey> where TKey : IEquatable<TKey>
	{
		public StringCache(Func<TKey, string> generateFunction, int capacity)
		{
			this._cache = new StringCacheDictionaryContainer<TKey>(capacity);
			this._generateFunction = generateFunction;
		}

		public StringCache(Func<TKey, string> generateFunction, int capacity, IStringCacheContainer<TKey> container)
		{
			this._cache = container;
			this._generateFunction = generateFunction;
		}

		public string Get(TKey key)
		{
			return this._cache.Get(key);
		}

		public bool TryGetValue(TKey key, out string value)
		{
			return this._cache.TryGetValue(key, out value);
		}

		public string GenerateValue(TKey key)
		{
			string text = this._generateFunction(key);
			this._cache.Add(key, text);
			return text;
		}

		private readonly IStringCacheContainer<TKey> _cache;

		private readonly Func<TKey, string> _generateFunction;
	}
}
