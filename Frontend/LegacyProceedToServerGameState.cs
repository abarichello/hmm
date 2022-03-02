using System;
using System.Linq;
using HeavyMetalMachines.Match;
using Hoplon.Assertions;
using Pocketverse;

namespace HeavyMetalMachines.Frontend
{
	public class LegacyProceedToServerGameState : IProceedToServerGameState
	{
		public LegacyProceedToServerGameState(MatchData matchData, IMatchPlayers matchPlayers, ServerInfo serverInfo, IMatchTeamsDispatcher matchTeamsDispatcher, StateMachine stateMachine, LoadingState loadingState)
		{
			this._matchData = matchData;
			this._matchPlayers = matchPlayers;
			this._serverInfo = serverInfo;
			this._matchTeamsDispatcher = matchTeamsDispatcher;
			this._stateMachine = stateMachine;
			this._loadingState = loadingState;
		}

		public void Proceed()
		{
			this.AssertCharactersAreSet();
			this._matchData.State = MatchData.MatchState.PreMatch;
			this._serverInfo.SpreadInfo();
			this._matchPlayers.UpdatePlayers();
			this._matchTeamsDispatcher.UpdateTeams();
			this._stateMachine.GotoState(this._loadingState, false);
		}

		private void AssertCharactersAreSet()
		{
			Assert.IsTrue(this._matchPlayers.PlayersAndBots.All((PlayerData player) => player.CharacterItemType != null), "Cannot proceed to game state when there are players without a character.");
		}

		private readonly MatchData _matchData;

		private readonly IMatchPlayers _matchPlayers;

		private readonly ServerInfo _serverInfo;

		private readonly IMatchTeamsDispatcher _matchTeamsDispatcher;

		private readonly StateMachine _stateMachine;

		private readonly LoadingState _loadingState;
	}
}
