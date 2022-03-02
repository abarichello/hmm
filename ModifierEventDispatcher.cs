using System;
using HeavyMetalMachines.Playback;
using Pocketverse;

namespace HeavyMetalMachines
{
	public class ModifierEventDispatcher : ModifierEventParser
	{
		public ModifierEventDispatcher(IFrameProcessorFactory factory)
		{
			factory.GetProvider(OperationKind.Playback).Bind(FrameKind.ModifierEvent, new ProcessFrame(this.PlaybackProcess));
			factory.GetProvider(OperationKind.ReplayPlayback).Bind(FrameKind.ModifierEvent, new ProcessFrame(this.PlaybackProcess));
		}

		private void PlaybackProcess(IFrame frame, IFrameProcessContext ctx)
		{
			this.Process(frame.GetReadData());
		}

		private const FrameKind Kind = FrameKind.ModifierEvent;
	}
}
