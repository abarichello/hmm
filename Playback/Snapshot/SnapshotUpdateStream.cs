using System;
using System.Collections.Generic;
using Pocketverse;

namespace HeavyMetalMachines.Playback.Snapshot
{
	public class SnapshotUpdateStream<T> where T : ISnapshotStreamContent, new()
	{
		public void ReadStream(BitStream stream, IDictionary<int, T> snapshots)
		{
			while (stream.ReadBool())
			{
				int key = stream.ReadCompressedInt();
				short version = stream.ReadShort();
				byte[] data = stream.ReadByteArray();
				T t;
				if (!snapshots.TryGetValue(key, out t))
				{
					T t2 = Activator.CreateInstance<T>();
					snapshots[key] = t2;
					t = t2;
				}
				t.ApplyStreamData(data);
				t.Version = version;
			}
		}

		private byte[] _byteArrayCache = new byte[1024];
	}
}
