using System;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.MatchMaking;
using HeavyMetalMachines.MatchMakingQueue.Infra;
using HeavyMetalMachines.MatchMakingQueue.Infra.Exceptions;
using HeavyMetalMachines.Players.Business;
using HeavyMetalMachines.Regions.Infra;
using HeavyMetalMachines.Social.Groups.Business;
using HeavyMetalMachines.Social.Groups.Models;
using HeavyMetalMachines.ToggleableFeatures;
using HeavyMetalMachines.Tournaments;
using HeavyMetalMachines.Tournaments.API;
using Hoplon.ToggleableFeatures;
using UniRx;
using Zenject;

namespace HeavyMetalMachines.Matchmaking.Queue
{
	public class MatchmakingTournamentTournamentQueueJoin : IMatchmakingTournamentQueueJoin
	{
		public IObservable<Unit> JoinTournament()
		{
			return Observable.SelectMany<MatchmakingStartedArgs, Unit>(Observable.SelectMany<Unit, MatchmakingStartedArgs>(this.FindTournamentMatch(), (Unit _) => this._matchmaking.WaitForMatchStart()), (MatchmakingStartedArgs _) => Observable.Do<Unit>(this._matchmaking.WaitForClientDisconnect(), delegate(Unit __)
			{
				throw new ClientDisconnectBeforeMatchException();
			}));
		}

		private IObservable<Unit> FindTournamentMatch()
		{
			TournamentCurrentStatus currentTournamentStatus = this._getTournamentStatus.GetCurrentTournamentStatus();
			if (currentTournamentStatus.ClosestStep == null)
			{
				throw new InvalidOperationException("JoinTournament: TournamentCurrentStatus is invalid. ClosestStep is null");
			}
			this._matchData.Kind = 5;
			Tournament tournament = this._getCurrentTournament.Get();
			this._changeUserRegion.ChangeRegion(tournament.Configuration.RegionName);
			string queuName = this._getTournamentTier.Get(tournament.Configuration.TierId).QueuName;
			if (this._isFeatureToggled.Check(Features.TournamentJoinSolo))
			{
				IPlayer player = this._getLocalPlayer.Get();
				return this._matchmaking.FindTournamentMatchAsSolo(player.UniversalId, currentTournamentStatus.ClosestStep.Id, queuName);
			}
			Group group = this._groupStorage.Group;
			return this._matchmaking.FindTournamentMatch(group, currentTournamentStatus.ClosestStep.Id, queuName);
		}

		[Inject]
		private IMatchmakingService _matchmaking;

		[Inject]
		private IGetCurrentTournament _getCurrentTournament;

		[Inject]
		private IRegionService _changeUserRegion;

		[Inject]
		private IGroupStorage _groupStorage;

		[Inject]
		private IGetTournamentStatus _getTournamentStatus;

		[Inject]
		private IGetTournamentTier _getTournamentTier;

		[Inject]
		private IIsFeatureToggled _isFeatureToggled;

		[Inject]
		private IGetLocalPlayer _getLocalPlayer;

		[Inject]
		private MatchData _matchData;
	}
}
