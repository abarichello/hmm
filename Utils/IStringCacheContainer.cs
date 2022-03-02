using System;

namespace HeavyMetalMachines.Utils
{
	public interface IStringCacheContainer<TKey>
	{
		void Add(TKey key, string value);

		bool TryGetValue(TKey key, out string value);

		string Get(TKey key);
	}
}
