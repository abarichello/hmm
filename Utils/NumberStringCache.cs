using System;

namespace HeavyMetalMachines.Utils
{
	public class NumberStringCache
	{
		public NumberStringCache(int maxCapacity, NumberStringCache.NumberDisplayStyle style)
		{
			Func<int, string> generateFunction = (int i) => (style != NumberStringCache.NumberDisplayStyle.LeftPadded) ? i.ToString() : i.ToString().PadLeft(maxCapacity.ToString().Length);
			this._cache = new StringCache<int>(generateFunction, maxCapacity, new StringCacheArrayContainer(maxCapacity));
		}

		public string Get(int key)
		{
			return this._cache.Get(key);
		}

		public void Add(int index)
		{
			this._cache.GenerateValue(index);
		}

		private readonly StringCache<int> _cache;

		private NumberStringCache.NumberDisplayStyle _style;

		private int _maxCapacity;

		public enum NumberDisplayStyle
		{
			None,
			LeftPadded
		}
	}
}
