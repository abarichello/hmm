using System;

namespace HeavyMetalMachines.Utils
{
	public static class StringCaches
	{
		static StringCaches()
		{
			int num = Math.Max(4, 2);
			StringCaches.NonPaddedIntegers = new NumberStringCache(num, NumberStringCache.NumberDisplayStyle.None);
			StringCaches.InitializeScoreCache(num);
		}

		private static void InitializeScoreCache(int maxScore)
		{
			for (int i = 0; i < maxScore; i++)
			{
				StringCaches.NonPaddedIntegers.Add(i);
			}
		}

		public static readonly NumberStringCache NonPaddedIntegers;
	}
}
