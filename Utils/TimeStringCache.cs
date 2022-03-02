using System;

namespace HeavyMetalMachines.Utils
{
	public class TimeStringCache
	{
		public TimeStringCache(int capacity, bool precache)
		{
			Func<int, string> generateFunction = delegate(int totalSeconds)
			{
				int num = (int)((long)totalSeconds % 60L);
				int num2 = (int)((long)totalSeconds / 60L % 60L);
				int num3 = (int)((long)totalSeconds / 3600L % 24L);
				return (num3 <= 0) ? string.Format("{0:00}:{1:00}", num2, num) : string.Format("{0:00}:{1:00}:{2:00}", num3, num2, num);
			};
			this._cache = new StringCache<int>(generateFunction, capacity);
			if (!precache)
			{
				return;
			}
			for (int i = 0; i < capacity; i++)
			{
				this._cache.GenerateValue(i);
			}
		}

		public string Get(int key)
		{
			return this._cache.Get(key);
		}

		public bool TryGetValue(int key, out string value)
		{
			return this._cache.TryGetValue(key, out value);
		}

		public string GenerateValue(int key)
		{
			return this._cache.GenerateValue(key);
		}

		private readonly StringCache<int> _cache;

		private int _maxCapacity;
	}
}
