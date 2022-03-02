using System;
using ClientAPI;
using ClientAPI.Matchmaking;
using HeavyMetalMachines.Social.Groups.Models;
using UniRx;

namespace HeavyMetalMachines.MatchMakingQueue.Infra
{
	public interface ISwordfishMatchmakingWrapper
	{
		event Action<MatchmakingArgs> MatchError;

		event Action<MatchmakingArgs> Disconnection;

		event Action<MatchmakingArgs> MatchMade;

		event Action<MatchmakingArgs> MatchCancel;

		event Action<MatchmakingEventArgs> MatchConfirmed;

		event Action<string> MatchAccepted;

		event Action<MatchmakingStartedArgs> MatchStarted;

		event Action NoServerAvailable;

		event Action<MatchmakingTimePredicted> TimeToPlayPredicted;

		bool IsWaitingInQueue();

		string GetCurrentQueueName();

		void PlayTournament(IObserver<Unit> observer, Group group, long tournamentStepId, string queueName, SwordfishClientApi.NetworkErrorCallback errorCallback);

		void PlayTournamentSolo(IObserver<Unit> observer, string playerUniversalId, long tournamentStepId, string queueName, SwordfishClientApi.NetworkErrorCallback errorCallback);

		void FindMatch(string queueName);

		void Accept(string queueName);

		void Decline(string queueName);

		void CancelSearch(string queueName);

		IObservable<MatchmakingStartedArgs> CreateMatch(string config);
	}
}
