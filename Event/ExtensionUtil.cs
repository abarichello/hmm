using System;
using System.Collections.Generic;
using System.Linq;

namespace HeavyMetalMachines.Event
{
	public static class ExtensionUtil
	{
		public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int chunksize)
		{
			while (source.Any<T>())
			{
				yield return source.Take(chunksize);
				source = source.Skip(chunksize);
			}
			yield break;
		}
	}
}
