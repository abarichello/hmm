using System;

namespace HeavyMetalMachines.UpdateStream
{
	internal static class UpdateDataStreamBuffer
	{
		public static void AddRef(int bufferSize)
		{
			UpdateDataStreamBuffer._refCount++;
			if (UpdateDataStreamBuffer.Buffer == null)
			{
				UpdateDataStreamBuffer.Buffer = new byte[bufferSize];
			}
		}

		public static void Release()
		{
			if (UpdateDataStreamBuffer._refCount <= 0)
			{
				return;
			}
			if (--UpdateDataStreamBuffer._refCount == 0)
			{
				UpdateDataStreamBuffer.Buffer = null;
			}
		}

		public static byte[] Buffer;

		private static int _refCount;
	}
}
