using System;
using System.Collections.Generic;
using HeavyMetalMachines.Playback.Snapshot;
using Pocketverse;

namespace HeavyMetalMachines.Playback
{
	internal class HORTAFastRewind
	{
		public HORTAFastRewind(IMatchBufferReader bufferReader, IFrameProcessorFactory processorFactory, IMatchBuffer matchBuffer)
		{
			this._bufferReader = bufferReader;
			this._processorFactory = processorFactory;
			this._matchBuffer = matchBuffer;
		}

		public void Execute(int targetTime, int originalTime, List<IFeatureSnapshot> snapshots)
		{
			HORTAFastRewind.Log.InfoFormat("Rewind Tag Start from={0} to={1}", new object[]
			{
				originalTime,
				targetTime
			});
			DefaultProcessContext context = new DefaultProcessContext(() => targetTime);
			this.RewindSnapshots(targetTime, originalTime, snapshots, context);
			this.RewindFrames(targetTime, context);
			this.RunExecutionQueue(context);
			HORTAFastRewind.Log.Info("Rewind Tag End");
		}

		private void RewindSnapshots(int targetTime, int originalTime, List<IFeatureSnapshot> snapshots, IFrameProcessContext context)
		{
			for (int i = 0; i < snapshots.Count; i++)
			{
				HORTAFastRewind.Log.DebugFormat("Snapshot={0}", new object[]
				{
					snapshots[i].Kind
				});
				snapshots[i].RewindToTime(originalTime, targetTime, context);
			}
		}

		private void RewindFrames(int targetTime, IFrameProcessContext context)
		{
			IFrame frame2 = this._bufferReader.Current;
			IFrameProcessorProvider provider = this._processorFactory.GetProvider(OperationKind.Rewind);
			FrameCheck check = (IFrame frame) => frame.Time >= targetTime;
			do
			{
				HORTAFastRewind.ProcessKeyFrame(frame2, context, provider);
				this._bufferReader.ReadPrevious(check, out frame2);
			}
			while (frame2 != null);
			this._bufferReader.ReadPrevious((IFrame x) => true, out frame2);
		}

		private void RunExecutionQueue(DefaultProcessContext context)
		{
			context.ExecutionQueue.Sort(new Comparison<int>(this.KeyFrameTimeComparison));
			IFrameProcessorProvider provider = this._processorFactory.GetProvider(OperationKind.RewindExecutionQueue);
			int i = 0;
			for (int j = 0; j < context.ExecutionQueue.Count; j++)
			{
				IFrame frame = this._matchBuffer.GetFrame(context.ExecutionQueue[j]);
				while (i < context.ActionQueue.Count && context.ActionQueue.Keys[i] <= frame.Time)
				{
					context.ActionQueue.Values[i]();
					i++;
				}
				HORTAFastRewind.ProcessKeyFrame(frame, context, provider);
			}
			while (i < context.ActionQueue.Count)
			{
				context.ActionQueue.Values[i++]();
			}
		}

		private int KeyFrameTimeComparison(int frameId1, int frameId2)
		{
			IFrame frame = this._matchBuffer.GetFrame(frameId1);
			IFrame frame2 = this._matchBuffer.GetFrame(frameId2);
			return frame.Time.CompareTo(frame2.Time);
		}

		private static void ProcessKeyFrame(IFrame frame, IFrameProcessContext context, IFrameProcessorProvider processorProvider)
		{
			ProcessFrame processor = processorProvider.GetProcessor((FrameKind)frame.Type);
			if (processor != null)
			{
				processor(frame, context);
				return;
			}
			HORTAFastRewind.Log.WarnFormat("Processor not found for type={0}. Skipping this keyframe.", new object[]
			{
				frame.Type
			});
		}

		private static readonly BitLogger Log = new BitLogger(typeof(HORTAFastRewind));

		private readonly IMatchBufferReader _bufferReader;

		private readonly IFrameProcessorFactory _processorFactory;

		private readonly IMatchBuffer _matchBuffer;
	}
}
