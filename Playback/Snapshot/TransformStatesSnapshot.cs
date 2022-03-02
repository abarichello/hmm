using System;
using HeavyMetalMachines.UpdateStream;
using Pocketverse;
using Zenject;

namespace HeavyMetalMachines.Playback.Snapshot
{
	public class TransformStatesSnapshot : IFeatureSnapshot
	{
		public FrameKind Kind
		{
			get
			{
				return FrameKind.TransformStates;
			}
		}

		public void RewindToTime(int currentTime, int targetTime, IFrameProcessContext ctx)
		{
			foreach (MovementStream movementStream in this._manager.AllStreams)
			{
				movementStream.Clear();
			}
		}

		public void AddFrame(IFrame frame)
		{
		}

		public void Clear()
		{
		}

		[Inject]
		private IUpdateManager _manager;
	}
}
