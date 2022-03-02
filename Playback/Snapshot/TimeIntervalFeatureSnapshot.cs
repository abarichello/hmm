using System;
using System.Collections.Generic;
using Pocketverse;

namespace HeavyMetalMachines.Playback.Snapshot
{
	public abstract class TimeIntervalFeatureSnapshot<TSnapshotData> : AbstractFeatureSnapshot where TSnapshotData : class, ISnapshotData<TSnapshotData>, new()
	{
		protected abstract int SnapshotIntervalMillis { get; }

		protected abstract void RestoreSnapshot(TSnapshotData data, int targetTime, IFrameProcessContext ctx);

		protected override bool CheckAndAddKeyFrame(IFrame frame)
		{
			if (this._snapshots.Count == 0)
			{
				this.AddSnapshot(frame, (TSnapshotData)((object)null));
				return true;
			}
			TSnapshotData lastSnapshot = this._snapshotList.Values[this._snapshotList.Count - 1];
			if (frame.Time - lastSnapshot.BaseFrame.Time < this.SnapshotIntervalMillis)
			{
				lastSnapshot.Frames.Add(frame);
				return false;
			}
			this.AddSnapshot(frame, lastSnapshot);
			return true;
		}

		private void AddSnapshot(IFrame frame, TSnapshotData lastSnapshot)
		{
			TSnapshotData value = Activator.CreateInstance<TSnapshotData>();
			value.Init(frame, lastSnapshot);
			this._snapshots[frame] = value;
			this._snapshotList[frame.FrameId] = value;
		}

		protected override void OnClear()
		{
			this._snapshotList.Clear();
			this._snapshots.Clear();
		}

		protected override void ApplySnapshot(IFrame frame, int targetTime, IFrameProcessContext ctx)
		{
			TSnapshotData data;
			if (!this._snapshots.TryGetValue(frame, out data))
			{
				throw new KeyNotFoundException("Trying to apply a snapshot, but keyframe has no match snapshot data");
			}
			this.RestoreSnapshot(data, targetTime, ctx);
		}

		private readonly SortedList<int, TSnapshotData> _snapshotList = new SortedList<int, TSnapshotData>();

		private readonly Dictionary<IFrame, TSnapshotData> _snapshots = new Dictionary<IFrame, TSnapshotData>();
	}
}
