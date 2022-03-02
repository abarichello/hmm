using System;

namespace HeavyMetalMachines.Memory
{
	public static class DummyAllocator
	{
		public static void Allocate(long bytes)
		{
			long num = bytes % 1024L;
			long num2 = 1024L - num;
			bytes += num2;
			long num3 = bytes / 1024L;
			object[] array = new object[num3];
			int num4 = 0;
			while ((long)num4 < num3)
			{
				array[num4] = new byte[1024L];
				num4++;
			}
		}

		private const long _arraySize = 1024L;
	}
}
