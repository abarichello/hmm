using System;
using Hoplon;
using UnityEngine;

namespace HeavyMetalMachines
{
	public class UnityRandom : IRandom
	{
		public int Range(int startInclusive, int endExclusive)
		{
			return Random.Range(startInclusive, endExclusive);
		}

		public double NextDouble()
		{
			return (double)Random.value;
		}
	}
}
