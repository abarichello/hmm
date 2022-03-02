using System;
using HeavyMetalMachines.Combat;

namespace HeavyMetalMachines.Playback.Snapshot
{
	public class GridGamePlayerSnapshotData : BombGridController.IGridGamePlayerSerialData, ISnapshotStreamContent, IBaseStreamSerialData<BombGridController.IGridGamePlayerSerialData>
	{
		public GridGamePlayerSnapshotData()
		{
		}

		public GridGamePlayerSnapshotData(GridGamePlayerSnapshotData other)
		{
			this.Apply(other);
		}

		public float Value { get; set; }

		public short Version { get; set; }

		public void Apply(BombGridController.IGridGamePlayerSerialData other)
		{
			this.Value = other.Value;
		}

		public void ApplyStreamData(byte[] data)
		{
			this.Value = (float)data[0];
		}
	}
}
