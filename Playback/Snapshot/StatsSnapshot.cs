using System;
using Zenject;

namespace HeavyMetalMachines.Playback.Snapshot
{
	public class StatsSnapshot : TimeIntervalFeatureSnapshot<StatsSnapshotData>
	{
		public override FrameKind Kind
		{
			get
			{
				return FrameKind.PlayerStats;
			}
		}

		protected override int SnapshotIntervalMillis
		{
			get
			{
				return 5000;
			}
		}

		protected override void RestoreSnapshot(StatsSnapshotData data, int targetTime, IFrameProcessContext ctx)
		{
			data.Restore(targetTime, ctx, this._playerStatsFeature);
		}

		[Inject]
		private IPlayerStatsFeature _playerStatsFeature;

		private const int IntervalMillis = 5000;
	}
}
