using System;
using HeavyMetalMachines.Matches;
using HeavyMetalMachines.Matches.DataTransferObjects;
using HeavyMetalMachines.Social.Groups.Models;
using UniRx;

namespace HeavyMetalMachines.MatchMakingQueue.Infra
{
	public interface IMatchmakingService
	{
		IObservable<Unit> FindTournamentMatch(Group group, long tournamentStepId, string queueName);

		IObservable<Unit> FindTournamentMatchAsSolo(string playerUniversalId, long tournamentStepId, string queueName);

		IObservable<Match> CreateMatch(string config);

		IObservable<MatchmakingStartedArgs> WaitForMatchStart();

		bool IsWaitingInQueue();

		string GetCurrentQueueName();

		void AcceptMatch();

		void DeclineMatch();

		void CancelSearch(string queueName);

		IObservable<Unit> WaitForClientDisconnect();

		IObservable<MatchmakingStartMatchSearchResult> StartFindingMatch(MatchKind matchKind);

		IObservable<Unit> OnMatchFound { get; }

		IObservable<string> OnPlayerAcceptedMatch { get; }

		IObservable<string> OnPlayerDeclinedMatch { get; }

		IObservable<Match> OnMatchReady { get; }

		IObservable<Unit> OnNoServerAvailable { get; }

		IObservable<Unit> OnMatchDisconnection { get; }

		IObservable<Unit> OnMatchCanceled { get; }

		IObservable<Unit> OnMatchError { get; }

		IObservable<Unit> OnClientDisconnected { get; }

		IObservable<Unit> OnConnectionInstability { get; }
	}
}
