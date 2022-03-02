using System;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Matches.DataTransferObjects;
using HeavyMetalMachines.MatchMaking;
using HeavyMetalMachines.Swordfish;
using Hoplon.Logging;
using UniRx;

namespace HeavyMetalMachines.Matchmaking.Queue
{
	public class LegacyGetThenObserveMatchmakingQueueState : IGetThenObserveMatchmakingQueueState
	{
		public LegacyGetThenObserveMatchmakingQueueState(SwordfishMatchmaking swordfishMatchmaking, MatchData matchData, ILogger<LegacyGetThenObserveMatchmakingQueueState> logger)
		{
			this._swordfishMatchmaking = swordfishMatchmaking;
			this._matchData = matchData;
			this._logger = logger;
		}

		public MatchmakingQueueState Get()
		{
			return this.GetState();
		}

		public IObservable<MatchmakingQueueState> GetThenObserve()
		{
			return Observable.Concat<MatchmakingQueueState>(Observable.Defer<MatchmakingQueueState>(() => Observable.Return<MatchmakingQueueState>(this.GetState())), new IObservable<MatchmakingQueueState>[]
			{
				Observable.Select<Unit, MatchmakingQueueState>(Observable.Merge<Unit>(new IObservable<Unit>[]
				{
					this.ObserveMatchKindChangedEvent(),
					this.ObserveClientConnectedEvent(),
					this.ObserveClientmatchMadeEvent(),
					this.ObserveClientDisconnectedEvent()
				}), (Unit _) => this.GetState())
			});
		}

		private IObservable<Unit> ObserveMatchKindChangedEvent()
		{
			return Observable.AsUnitObservable<MatchKind>(Observable.FromEvent<MatchKind>(delegate(Action<MatchKind> handler)
			{
				this._matchData.OnKindChange += handler;
			}, delegate(Action<MatchKind> handler)
			{
				this._matchData.OnKindChange -= handler;
			}));
		}

		private IObservable<Unit> ObserveClientConnectedEvent()
		{
			return Observable.FromEvent(delegate(Action handler)
			{
				this._swordfishMatchmaking.OnClientConnectedEvent += handler;
			}, delegate(Action handler)
			{
				this._swordfishMatchmaking.OnClientConnectedEvent -= handler;
			});
		}

		private IObservable<Unit> ObserveClientDisconnectedEvent()
		{
			return Observable.FromEvent(delegate(Action handler)
			{
				this._swordfishMatchmaking.OnClientDisconnectedEvent += handler;
			}, delegate(Action handler)
			{
				this._swordfishMatchmaking.OnClientDisconnectedEvent -= handler;
			});
		}

		private IObservable<Unit> ObserveClientmatchMadeEvent()
		{
			return Observable.FromEvent(delegate(Action handler)
			{
				this._swordfishMatchmaking.OnClientMatchMadeEvent += handler;
			}, delegate(Action handler)
			{
				this._swordfishMatchmaking.OnClientMatchMadeEvent -= handler;
			});
		}

		private MatchmakingQueueState GetState()
		{
			MatchmakingQueueStep step = 0;
			if (this.IsFindingMatch())
			{
				step = 2;
			}
			if (this._swordfishMatchmaking.State == SwordfishMatchmaking.MatchmakingState.Made)
			{
				step = 3;
			}
			MatchmakingQueueState result = default(MatchmakingQueueState);
			result.QueueName = this._swordfishMatchmaking.MatchMadeQueue;
			result.SearchingMatchKind = this._matchData.Kind;
			result.Step = step;
			return result;
		}

		private bool IsFindingMatch()
		{
			this._logger.DebugFormat("IsFindingMatch {0} {1} {2} {3}", new object[]
			{
				this._swordfishMatchmaking.Connected,
				this._swordfishMatchmaking.WaitingForMatchResult,
				this._swordfishMatchmaking.Undefined,
				this._swordfishMatchmaking.State
			});
			return this._swordfishMatchmaking.Connected || this._swordfishMatchmaking.WaitingForMatchResult || this._swordfishMatchmaking.Undefined || this._swordfishMatchmaking.State == SwordfishMatchmaking.MatchmakingState.Started;
		}

		public IObservable<Exception> ObserveErrors()
		{
			throw new NotImplementedException("LegacyGetThenObserveMatchmakingQueueState.ObserveErrors");
		}

		private readonly SwordfishMatchmaking _swordfishMatchmaking;

		private readonly MatchData _matchData;

		private readonly ILogger<LegacyGetThenObserveMatchmakingQueueState> _logger;
	}
}
