using System;
using Zenject;

namespace HeavyMetalMachines.Playback.Snapshot
{
	public class CombatStatesSnapshot : TimeIntervalFeatureSnapshot<CombatStatesSnapshotData>
	{
		public override FrameKind Kind
		{
			get
			{
				return FrameKind.CombatStates;
			}
		}

		protected override int SnapshotIntervalMillis
		{
			get
			{
				return 5000;
			}
		}

		protected override void RestoreSnapshot(CombatStatesSnapshotData data, int targetTime, IFrameProcessContext ctx)
		{
			data.Restore(targetTime, ctx, this._combatStatesFeature);
		}

		[Inject]
		private ICombatStatesFeature _combatStatesFeature;

		private const int IntervalMillis = 5000;
	}
}
