using System;

namespace HeavyMetalMachines
{
	public static class SysRandom
	{
		public static int Int(int min, int max)
		{
			if (SysRandom.random == null)
			{
				SysRandom.random = new Random(DateTime.Now.Millisecond);
			}
			return SysRandom.random.Next(min, max);
		}

		public static float Float(float min, float max)
		{
			if (SysRandom.random == null)
			{
				SysRandom.random = new Random(DateTime.Now.Millisecond);
			}
			return min + (float)SysRandom.random.NextDouble() * (max - min);
		}

		private static Random random;
	}
}
