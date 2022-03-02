using System;
using HeavyMetalMachines.Combat;
using Pocketverse;

namespace HeavyMetalMachines.Playback.Snapshot
{
	public class SpawnControllerSnapshotData : ISnapshotStreamContent, ISpawnControllerSerialData, IBaseStreamSerialData<ISpawnControllerSerialData>
	{
		public SpawnControllerSnapshotData()
		{
		}

		public SpawnControllerSnapshotData(SpawnControllerSnapshotData other)
		{
			this.Version = other.Version;
			this.Apply(other);
		}

		public short Version { get; set; }

		public SpawnStateKind State
		{
			get
			{
				return this._state;
			}
		}

		public int SpawnTime
		{
			get
			{
				return this._spawnTime;
			}
		}

		public int UnspawnTime
		{
			get
			{
				return this._unspawnTime;
			}
		}

		public void Apply(ISpawnControllerSerialData other)
		{
			this._state = other.State;
			this._spawnTime = other.SpawnTime;
			this._unspawnTime = other.UnspawnTime;
		}

		public void ApplyStreamData(byte[] data)
		{
			BitStream readStream = StaticBitStream.GetReadStream(data);
			this._state = (SpawnStateKind)readStream.ReadCompressedInt();
			this._spawnTime = readStream.ReadCompressedInt();
			this._unspawnTime = readStream.ReadCompressedInt();
		}

		private SpawnStateKind _state;

		private int _spawnTime;

		private int _unspawnTime;
	}
}
