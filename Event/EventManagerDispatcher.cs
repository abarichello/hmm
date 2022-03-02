using System;
using HeavyMetalMachines.Playback;
using Pocketverse;

namespace HeavyMetalMachines.Event
{
	public class EventManagerDispatcher : GameHubObject, IEventManagerDispatcher
	{
		public EventManagerDispatcher(IFrameProcessorFactory factory)
		{
			factory.GetProvider(OperationKind.Playback).Bind(FrameKind.ManagerEvent, new ProcessFrame(this.Process));
			factory.GetProvider(OperationKind.ReplayPlayback).Bind(FrameKind.ManagerEvent, new ProcessFrame(this.Process));
			factory.GetProvider(OperationKind.Rewind).Bind(FrameKind.ManagerEvent, new ProcessFrame(this.RewindProcess));
			factory.GetProvider(OperationKind.ReplayRewind).Bind(FrameKind.ManagerEvent, new ProcessFrame(this.RewindProcess));
			factory.GetProvider(OperationKind.RewindExecutionQueue).Bind(FrameKind.ManagerEvent, new ProcessFrame(this.Process));
			factory.GetProvider(OperationKind.ReplayExecutionQueue).Bind(FrameKind.ManagerEvent, new ProcessFrame(this.Process));
			factory.GetProvider(OperationKind.FastForward).Bind(FrameKind.ManagerEvent, new ProcessFrame(this.FastForwardProcess));
		}

		private void Process(IFrame frame, IFrameProcessContext ctx)
		{
			GameHubObject.Hub.Events.Process(frame.GetReadData());
		}

		private void RewindProcess(IFrame frame, IFrameProcessContext ctx)
		{
			bool flag = GameHubObject.Hub.Events.RewindProcess(frame);
			if (flag && frame.PreviousFrameId != -1)
			{
				ctx.AddToExecutionQueue(frame.PreviousFrameId);
			}
			ctx.RemoveFromExecutionQueue(frame.FrameId);
		}

		private void FastForwardProcess(IFrame frame, IFrameProcessContext ctx)
		{
			GameHubObject.Hub.Events.FastForward(frame, ctx);
		}

		public void Send(EventData e)
		{
			GameHubObject.Hub.Events.Send(e);
		}

		public void SendFullFrame(byte to)
		{
			GameHubObject.Hub.Events.SendFullFrame(to);
		}

		private const FrameKind Kind = FrameKind.ManagerEvent;
	}
}
