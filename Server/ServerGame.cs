using System;
using System.Collections.Generic;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.Match;
using Pocketverse;

namespace HeavyMetalMachines.Server
{
	[Serializable]
	public class ServerGame : GameState, MatchController.GameOverMessage.IGameOverListener
	{
		protected override void OnStateEnabled()
		{
			List<PlayerData> playersAndBots = GameHubBehaviour.Hub.Players.PlayersAndBots;
			for (int i = 0; i < playersAndBots.Count; i++)
			{
				PlayerData data = playersAndBots[i];
				BombMatchBI.CreatePlayer(data);
			}
			GameHubBehaviour.Hub.Server.LoadLevel(new Action(this.OnArenaLoaded));
		}

		private void OnArenaLoaded()
		{
			GameHubBehaviour.Hub.GameTime.SetTimeZero();
			GameHubBehaviour.Hub.UpdateManager.SetRunning(true);
			GameHubBehaviour.Hub.MatchHistory.WriteMatchStartInfo();
		}

		protected override void OnStateDisabled()
		{
			GameHubBehaviour.Hub.UpdateManager.SetRunning(false);
		}

		public void OnGameOver(MatchController.GameOverMessage msg)
		{
			this.GameOver.State = msg.State;
			this.GameOver.LoadingToken.InheritData(base.LoadingToken);
			GameHubBehaviour.Hub.State.GotoState(this.GameOver, false);
			BombMatchBI.MatchOver();
			MatchLogWriter.WriteAfk();
		}

		public ServerFinish GameOver;
	}
}
