using System;
using Pocketverse;
using Zenject;

namespace HeavyMetalMachines.Playback.Snapshot
{
	public class GadgetLevelSnapshot : TimeIntervalFeatureSnapshot<GadgetLevelSnapshotData>
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

		protected override void RestoreSnapshot(GadgetLevelSnapshotData data, int targetTime, IFrameProcessContext ctx)
		{
			data.Restore(targetTime, ctx, this._identifiableCollection);
		}

		[Inject]
		private IIdentifiableCollection _identifiableCollection;

		private const int IntervalMillis = 5000;
	}
}
