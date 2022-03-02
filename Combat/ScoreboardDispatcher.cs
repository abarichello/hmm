using System;
using HeavyMetalMachines.Playback;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public class ScoreboardDispatcher : ScoreboardParser
	{
		public ScoreboardDispatcher(IFrameProcessorFactory factory)
		{
			factory.GetProvider(OperationKind.Playback).Bind(FrameKind.Scoreboard, new ProcessFrame(this.Process));
			factory.GetProvider(OperationKind.ArrivalDuringReplay).Bind(FrameKind.Scoreboard, new ProcessFrame(this.Process));
			factory.GetProvider(OperationKind.FastForward).Bind(FrameKind.Scoreboard, new ProcessFrame(this.Process));
			factory.GetProvider(OperationKind.RewindExecutionQueue).Bind(FrameKind.Scoreboard, new ProcessFrame(this.ProcessSnapshot));
		}

		private void Process(IFrame frame, IFrameProcessContext ctx)
		{
			this.Update(frame.GetReadData());
		}

		private void ProcessSnapshot(IFrame frame, IFrameProcessContext ctx)
		{
			BitStream readData = frame.GetReadData();
			readData.ReadBool();
			this._scoreBoard.ReadFromBitStream(readData);
			this._gameTime.MatchTimer.ReadFromBitStream(readData);
			ScoreboardParser.Log.DebugFormat("Scoreboard snapshot state={0}", new object[]
			{
				this._scoreBoard.CurrentState
			});
			this._bombManager.PhaseChanged();
			this._bombManager.MatchUpdated();
			if (this._scoreBoard.IsInOvertime)
			{
				this._bombManager.OvertimeStarted();
			}
		}

		private const FrameKind Kind = FrameKind.Scoreboard;
	}
}
