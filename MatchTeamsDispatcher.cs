using System;
using HeavyMetalMachines.Playback;
using Pocketverse;

namespace HeavyMetalMachines
{
	public class MatchTeamsDispatcher : GameHubObject, IMatchTeamsDispatcher
	{
		public MatchTeamsDispatcher(IFrameProcessorFactory factory)
		{
			factory.GetProvider(OperationKind.Playback).Bind(FrameKind.Teams, new ProcessFrame(this.Process));
			factory.GetProvider(OperationKind.ArrivalDuringReplay).Bind(FrameKind.Teams, new ProcessFrame(this.Process));
			factory.GetProvider(OperationKind.FastForward).Bind(FrameKind.Teams, new ProcessFrame(this.Process));
		}

		private void Process(IFrame frame, IFrameProcessContext ctx)
		{
			GameHubObject.Hub.Teams.Update(frame.GetReadData());
		}

		public void SendTeams(byte to)
		{
			GameHubObject.Hub.TeamsDispatcher.SendTeams(to);
		}

		public void UpdateTeams()
		{
			GameHubObject.Hub.TeamsDispatcher.UpdateTeams();
		}

		private const FrameKind Kind = FrameKind.Teams;
	}
}
