using System;
using HeavyMetalMachines.Match;
using Pocketverse;

namespace HeavyMetalMachines.Server
{
	public class ServerFinish : GameState
	{
		protected override void OnStateEnabled()
		{
			this.State = GameHubBehaviour.Hub.Match.State;
			GameHubBehaviour.Hub.GameTime.MatchTimer.Stop();
			ServerFinish.Log.DebugFormat("Entered server finish state", new object[0]);
		}

		private static readonly BitLogger Log = new BitLogger(typeof(ServerFinish));

		public MatchData.MatchState State;
	}
}
