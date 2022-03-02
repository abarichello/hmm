using System;
using System.Collections.Generic;

namespace HeavyMetalMachines.Utils
{
	public static class CollectionsExtensions
	{
		public static IEnumerable<T> Enumerate<T>(this T value)
		{
			yield return value;
			yield break;
		}
	}
}
