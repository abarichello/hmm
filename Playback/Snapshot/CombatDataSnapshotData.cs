using System;
using HeavyMetalMachines.Combat;
using Pocketverse;

namespace HeavyMetalMachines.Playback.Snapshot
{
	public class CombatDataSnapshotData : ICombatDataSerialData, ISnapshotStreamContent, IBaseStreamSerialData<ICombatDataSerialData>
	{
		public CombatDataSnapshotData()
		{
		}

		public CombatDataSnapshotData(CombatDataSnapshotData other)
		{
			this.Version = other.Version;
			this.Apply(other);
		}

		public short Version { get; set; }

		public float HP { get; private set; }

		public float HPTemp { get; private set; }

		public float EP { get; private set; }

		public void ApplyStreamData(byte[] data)
		{
			BitStream readStream = StaticBitStream.GetReadStream(data);
			this.HP = readStream.ReadCompressedFloat();
			this.HPTemp = readStream.ReadCompressedFloat();
			this.EP = readStream.ReadCompressedFloat();
		}

		public void Apply(ICombatDataSerialData other)
		{
			this.HP = other.HP;
			this.HPTemp = other.HPTemp;
			this.EP = other.EP;
		}
	}
}
