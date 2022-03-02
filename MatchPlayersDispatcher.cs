using System;
using HeavyMetalMachines.Playback;
using Pocketverse;

namespace HeavyMetalMachines
{
	public class MatchPlayersDispatcher : GameHubObject, IMatchPlayersDispatcher
	{
		public MatchPlayersDispatcher(IFrameProcessorFactory factory)
		{
			factory.GetProvider(OperationKind.Playback).Bind(FrameKind.Players, new ProcessFrame(this.Process));
			factory.GetProvider(OperationKind.ArrivalDuringReplay).Bind(FrameKind.Players, new ProcessFrame(this.Process));
			factory.GetProvider(OperationKind.FastForward).Bind(FrameKind.Players, new ProcessFrame(this.Process));
		}

		private void Process(IFrame frame, IFrameProcessContext ctx)
		{
			GameHubObject.Hub.Players.Update(frame.GetReadData());
		}

		public void UpdatePlayers()
		{
			GameHubObject.Hub.Players.UpdatePlayers();
		}

		public void UpdatePlayer(int objId)
		{
			GameHubObject.Hub.Players.UpdatePlayer(objId);
		}

		public void SendPlayers(byte to)
		{
			GameHubObject.Hub.Players.SendPlayers(to);
		}

		private const FrameKind Kind = FrameKind.Players;
	}
}
