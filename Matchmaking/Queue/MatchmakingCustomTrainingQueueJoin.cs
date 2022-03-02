using System;
using HeavyMetalMachines.MatchMaking;
using HeavyMetalMachines.MatchMakingQueue.Infra;
using Hoplon.Logging;
using UniRx;
using Zenject;

namespace HeavyMetalMachines.Matchmaking.Queue
{
	public class MatchmakingCustomTrainingQueueJoin : IMatchmakingTrainingQueueJoin
	{
		public MatchmakingCustomTrainingQueueJoin(ILogger<MatchmakingCustomTrainingQueueJoin> logger)
		{
			this._logger = logger;
		}

		public IObservable<Unit> JoinTraining(string config)
		{
			Subject<Unit> observation = new Subject<Unit>();
			ObservableExtensions.Subscribe<Exception>(this._matchmakingStateStorage.OnMatchmakingError, delegate(Exception ex)
			{
				MatchmakingCustomTrainingQueueJoin.NotifyError(observation, ex);
			});
			ObservableExtensions.Subscribe<MatchmakingQueueState>(this._matchmakingStateStorage.OnStateChanged, delegate(MatchmakingQueueState state)
			{
				this.OnMatchmakingQueueStateChanged(observation, state);
			}, delegate(Exception exception)
			{
				MatchmakingCustomTrainingQueueJoin.NotifyError(observation, exception);
			});
			this._matchmaking.CreateMatch(config);
			return observation;
		}

		private void OnMatchmakingQueueStateChanged(Subject<Unit> observation, MatchmakingQueueState state)
		{
			this._logger.DebugFormat("OnMatchmakingQueueStateChanged. Step={0}", new object[]
			{
				state.Step
			});
			if (state.Step == 6)
			{
				this._logger.InfoFormat("OnMatchmakingQueueStateChanged. MatchStarted", new object[0]);
				ObservableExtensions.Subscribe<Unit>(this._connectToMatch.Connect(state.Match), delegate(Unit _)
				{
					MatchmakingCustomTrainingQueueJoin.NotifyCompletition(observation);
				}, delegate(Exception ex)
				{
					MatchmakingCustomTrainingQueueJoin.NotifyError(observation, ex);
				});
			}
		}

		private static void NotifyCompletition(Subject<Unit> observation)
		{
			observation.OnNext(Unit.Default);
			observation.OnCompleted();
		}

		private static void NotifyError(Subject<Unit> observation, Exception ex)
		{
			observation.OnError(ex);
		}

		private readonly ILogger<MatchmakingCustomTrainingQueueJoin> _logger;

		[Inject]
		private IConnectToMatch _connectToMatch;

		[Inject]
		private IMatchmakingMatchCreate _matchmaking;

		[Inject]
		private IMatchmakingStateStorage _matchmakingStateStorage;
	}
}
