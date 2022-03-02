using System;
using System.Collections.Generic;
using Pocketverse;
using Pocketverse.Util;

namespace HeavyMetalMachines.Playback.Snapshot
{
	public abstract class AbstractFeatureSnapshot : IFeatureSnapshot
	{
		protected AbstractFeatureSnapshot()
		{
			this._frames = new SortedList<int, IFrame>();
		}

		public abstract FrameKind Kind { get; }

		protected abstract bool CheckAndAddKeyFrame(IFrame frame);

		protected abstract void OnClear();

		protected abstract void ApplySnapshot(IFrame frame, int targetTime, IFrameProcessContext ctx);

		public void RewindToTime(int currentTime, int targetTime, IFrameProcessContext ctx)
		{
			AbstractFeatureSnapshot.FrameSearchFunction frameSearchFunction = new AbstractFeatureSnapshot.FrameSearchFunction
			{
				TimeToCheck = currentTime
			};
			int num;
			if (!this._frames.Values.BinarySearchValue(new ListUtils.BinarySearchFunction<IFrame>(frameSearchFunction.Compare), out num))
			{
				AbstractFeatureSnapshot.Log.DebugFormat("Could not find frame to apply snapshot for feature={0} frameCount={1}", new object[]
				{
					this.Kind,
					this._frames.Count
				});
				return;
			}
			frameSearchFunction.TimeToCheck = targetTime;
			int num2;
			bool flag = this._frames.Values.BinarySearchValue(new ListUtils.BinarySearchFunction<IFrame>(frameSearchFunction.Compare), out num2);
			if (flag && num2 != num)
			{
				AbstractFeatureSnapshot.Log.DebugFormat("Applying snapshot for feature={2} frame={1} at time={0}", new object[]
				{
					this._frames.Values[num2].Time,
					this._frames.Values[num2].FrameId,
					this.Kind
				});
				this.ApplySnapshot(this._frames.Values[num2], targetTime, ctx);
			}
			else if (flag)
			{
				AbstractFeatureSnapshot.Log.DebugFormat("Found ideal frame but it's the same as current frame={0} @={1}", new object[]
				{
					this._frames.Values[num2].FrameId,
					this._frames.Values[num2].Time
				});
			}
			else
			{
				AbstractFeatureSnapshot.Log.DebugFormat("Could not find ideal frame, current={0} @={1}", new object[]
				{
					this._frames.Values[num].FrameId,
					this._frames.Values[num].Time
				});
			}
		}

		public void AddFrame(IFrame frame)
		{
			if (this.CheckAndAddKeyFrame(frame))
			{
				this._frames.Add(frame.FrameId, frame);
			}
		}

		public void Clear()
		{
			this.OnClear();
			this._frames.Clear();
		}

		private static readonly BitLogger Log = new BitLogger(typeof(AbstractFeatureSnapshot));

		private readonly SortedList<int, IFrame> _frames;

		private struct FrameSearchFunction
		{
			public int Compare(IList<IFrame> list, int index)
			{
				IFrame frame = list[index];
				if (frame.Time > this.TimeToCheck)
				{
					return 1;
				}
				if (list.Count < index + 2)
				{
					return 0;
				}
				IFrame frame2 = list[index + 1];
				if (frame2.Time > this.TimeToCheck)
				{
					return 0;
				}
				return -1;
			}

			public int TimeToCheck;
		}
	}
}
