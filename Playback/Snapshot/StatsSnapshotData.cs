using System;
using System.Collections.Generic;
using HeavyMetalMachines.Bank;
using Pocketverse;

namespace HeavyMetalMachines.Playback.Snapshot
{
	public class StatsSnapshotData : ISnapshotData<StatsSnapshotData>
	{
		public StatsSnapshotData()
		{
			this._frames = new List<IFrame>();
			this._playerSnapshots = new Dictionary<int, PlayerStatsSnapshotData>();
			this._updateStream = new SnapshotUpdateStream<PlayerStatsSnapshotData>();
		}

		public IFrame BaseFrame { get; private set; }

		public IList<IFrame> Frames
		{
			get
			{
				return this._frames;
			}
		}

		public void Init(IFrame baseFrame, StatsSnapshotData lastSnapshot)
		{
			this.BaseFrame = baseFrame;
			if (lastSnapshot == null)
			{
				this.ApplyFrame(this.BaseFrame);
				return;
			}
			this._blueTeamDeaths = lastSnapshot._blueTeamDeaths;
			this._redTeamDeaths = lastSnapshot._redTeamDeaths;
			foreach (KeyValuePair<int, PlayerStatsSnapshotData> keyValuePair in lastSnapshot._playerSnapshots)
			{
				this._playerSnapshots[keyValuePair.Key] = new PlayerStatsSnapshotData(keyValuePair.Value);
			}
			foreach (IFrame frame in lastSnapshot._frames)
			{
				this.ApplyFrame(frame);
			}
			this.ApplyFrame(this.BaseFrame);
		}

		public void Restore(int targetTime, IFrameProcessContext ctx, IPlayerStatsFeature statsFeature)
		{
			foreach (KeyValuePair<int, PlayerStatsSnapshotData> keyValuePair in this._playerSnapshots)
			{
				IPlayerStatsSerialData stats = statsFeature.GetStats(keyValuePair.Key);
				stats.Apply(keyValuePair.Value);
			}
			statsFeature.BlueTeamDeaths = this._blueTeamDeaths;
			statsFeature.RedTeamDeaths = this._redTeamDeaths;
			for (int i = 0; i < this._frames.Count; i++)
			{
				if (this._frames[i].Time > targetTime)
				{
					break;
				}
				ctx.AddToExecutionQueue(this._frames[i].FrameId);
			}
		}

		private void ApplyFrame(IFrame frame)
		{
			BitStream readData = frame.GetReadData();
			this._updateStream.ReadStream(readData, this._playerSnapshots);
			this._blueTeamDeaths = readData.ReadCompressedInt();
			this._redTeamDeaths = readData.ReadCompressedInt();
		}

		private int _blueTeamDeaths;

		private int _redTeamDeaths;

		private readonly Dictionary<int, PlayerStatsSnapshotData> _playerSnapshots;

		private readonly List<IFrame> _frames;

		private readonly SnapshotUpdateStream<PlayerStatsSnapshotData> _updateStream;
	}
}
