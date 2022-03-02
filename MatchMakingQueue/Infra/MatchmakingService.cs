using System;
using ClientAPI.Exceptions;
using ClientAPI.Matchmaking;
using ClientAPI.MessageHub;
using HeavyMetalMachines.Matches;
using HeavyMetalMachines.Matches.DataTransferObjects;
using HeavyMetalMachines.MatchMakingQueue.Infra.Exceptions;
using HeavyMetalMachines.Net.Infra;
using HeavyMetalMachines.Social.Groups.Models;
using UniRx;

namespace HeavyMetalMachines.MatchMakingQueue.Infra
{
	public class MatchmakingService : IMatchmakingService
	{
		public MatchmakingService(ISwordfishMatchmakingWrapper matchmakingClient, INetworkClient networkClient, ISwordfishHubClientWrapper swordfishHubClient)
		{
			this._matchmakingClient = matchmakingClient;
			this._networkClient = networkClient;
			this._swordfishHubClient = swordfishHubClient;
		}

		public IObservable<Unit> FindTournamentMatch(Group group, long tournamentStepId, string queueName)
		{
			return Observable.Create<Unit>(delegate(IObserver<Unit> observer)
			{
				this._matchmakingClient.PlayTournament(observer, group, tournamentStepId, queueName, delegate(object state, ConnectionException exception)
				{
					observer.OnError(new MatchJoinQueueException(exception));
				});
				return this.WaitForTournamentQueueResponse(observer);
			});
		}

		public IObservable<Unit> FindTournamentMatchAsSolo(string playerUniversalId, long tournamentStepId, string queueName)
		{
			return Observable.Create<Unit>(delegate(IObserver<Unit> observer)
			{
				this._matchmakingClient.PlayTournamentSolo(observer, playerUniversalId, tournamentStepId, queueName, delegate(object state, ConnectionException exception)
				{
					observer.OnError(new MatchJoinQueueException(exception));
				});
				return this.WaitForTournamentQueueResponse(observer);
			});
		}

		private IDisposable WaitForTournamentQueueResponse(IObserver<Unit> observer)
		{
			IObservable<Unit> observable = Observable.Merge<Unit>(Observable.Merge<Unit>(this.WaitForMatchMade(), new IObservable<Unit>[]
			{
				Observable.Do<Unit>(this.WaitForMatchError(), delegate(Unit _)
				{
					observer.OnError(new MatchErrorException());
				})
			}), new IObservable<Unit>[]
			{
				Observable.Do<Unit>(this.WaitForMatchDisconnection(), delegate(Unit _)
				{
					observer.OnError(new MatchMakingDisconnectException());
				})
			});
			IObservable<Unit>[] array = new IObservable<Unit>[1];
			array[0] = Observable.Select<MatchmakingArgs, Unit>(Observable.Do<MatchmakingArgs>(this.WaitForMatchCancel(), delegate(MatchmakingArgs mmArgs)
			{
				observer.OnError(new MatchMakingCanceledException(mmArgs.CauserUniversalId));
			}), (MatchmakingArgs _) => Unit.Default);
			return ObservableExtensions.Subscribe<Unit>(Observable.First<Unit>(Observable.Merge<Unit>(observable, array)), delegate(Unit _)
			{
				observer.OnNext(Unit.Default);
				observer.OnCompleted();
			});
		}

		private IObservable<Unit> WaitForMatchError()
		{
			return Observable.AsUnitObservable<MatchmakingArgs>(Observable.FromEvent<MatchmakingArgs>(delegate(Action<MatchmakingArgs> handler)
			{
				this._matchmakingClient.MatchError += handler;
			}, delegate(Action<MatchmakingArgs> handler)
			{
				this._matchmakingClient.MatchError -= handler;
			}));
		}

		private IObservable<Unit> WaitForMatchDisconnection()
		{
			return Observable.AsUnitObservable<MatchmakingArgs>(Observable.FromEvent<MatchmakingArgs>(delegate(Action<MatchmakingArgs> handler)
			{
				this._matchmakingClient.Disconnection += handler;
			}, delegate(Action<MatchmakingArgs> handler)
			{
				this._matchmakingClient.Disconnection -= handler;
			}));
		}

		private IObservable<MatchmakingArgs> WaitForMatchCancel()
		{
			return Observable.FromEvent<MatchmakingArgs>(delegate(Action<MatchmakingArgs> handler)
			{
				this._matchmakingClient.MatchCancel += handler;
			}, delegate(Action<MatchmakingArgs> handler)
			{
				this._matchmakingClient.MatchCancel -= handler;
			});
		}

		public IObservable<Unit> WaitForClientDisconnect()
		{
			return Observable.AsUnitObservable<Unit>(Observable.FromEvent(delegate(Action handler)
			{
				this._networkClient.OnClientDisconnect += handler;
			}, delegate(Action handler)
			{
				this._networkClient.OnClientDisconnect -= handler;
			}));
		}

		public IObservable<Match> CreateMatch(string config)
		{
			return Observable.Select<MatchmakingStartedArgs, Match>(this._matchmakingClient.CreateMatch(config), (MatchmakingStartedArgs mmArgs) => this.ConvertMatchData(mmArgs));
		}

		public IObservable<Unit> OnMatchFound
		{
			get
			{
				return this.WaitForMatchMade();
			}
		}

		public IObservable<string> OnPlayerAcceptedMatch
		{
			get
			{
				return Observable.FromEvent<string>(delegate(Action<string> handler)
				{
					this._matchmakingClient.MatchAccepted += handler;
				}, delegate(Action<string> handler)
				{
					this._matchmakingClient.MatchAccepted -= handler;
				});
			}
		}

		public IObservable<string> OnPlayerDeclinedMatch { get; private set; }

		public IObservable<Match> OnMatchReady
		{
			get
			{
				return Observable.Select<MatchmakingStartedArgs, Match>(Observable.FromEvent<MatchmakingStartedArgs>(delegate(Action<MatchmakingStartedArgs> handler)
				{
					this._matchmakingClient.MatchStarted += handler;
				}, delegate(Action<MatchmakingStartedArgs> handler)
				{
					this._matchmakingClient.MatchStarted -= handler;
				}), new Func<MatchmakingStartedArgs, Match>(this.ConvertMatchData));
			}
		}

		private Match ConvertMatchData(MatchmakingStartedArgs mmArgs)
		{
			Match result = default(Match);
			MatchConnection matchConnection = default(MatchConnection);
			matchConnection.Host = mmArgs.Host;
			matchConnection.Port = mmArgs.Port;
			MatchConnection connection = matchConnection;
			result.MatchId = mmArgs.MatchId.ToString();
			result.Connection = connection;
			return result;
		}

		public IObservable<Unit> OnNoServerAvailable
		{
			get
			{
				return Observable.FromEvent(delegate(Action handler)
				{
					this._matchmakingClient.NoServerAvailable += handler;
				}, delegate(Action handler)
				{
					this._matchmakingClient.NoServerAvailable -= handler;
				});
			}
		}

		public IObservable<Unit> OnMatchDisconnection
		{
			get
			{
				return this.WaitForMatchDisconnection();
			}
		}

		public IObservable<Unit> OnMatchCanceled
		{
			get
			{
				return Observable.AsUnitObservable<MatchmakingArgs>(this.WaitForMatchCancel());
			}
		}

		public IObservable<Unit> OnMatchError
		{
			get
			{
				return this.WaitForMatchError();
			}
		}

		public IObservable<Unit> OnClientDisconnected
		{
			get
			{
				return this.WaitForClientDisconnect();
			}
		}

		private IObservable<ConnectionInstabilityMessage> AbstractHubClientOnConnectionInstability
		{
			get
			{
				return Observable.FromEvent<ConnectionInstabilityMessage>(delegate(Action<ConnectionInstabilityMessage> handler)
				{
					this._swordfishHubClient.OnConnectionInstability += handler;
				}, delegate(Action<ConnectionInstabilityMessage> handler)
				{
					this._swordfishHubClient.OnConnectionInstability -= handler;
				});
			}
		}

		public IObservable<Unit> OnConnectionInstability
		{
			get
			{
				return Observable.AsUnitObservable<ConnectionInstabilityMessage>(this.AbstractHubClientOnConnectionInstability);
			}
		}

		public IObservable<MatchmakingStartMatchSearchResult> StartFindingMatch(MatchKind matchKind)
		{
			if (matchKind != null)
			{
				throw new Exception(string.Format("Cannot find match of kind {0}.", matchKind));
			}
			return Observable.Select<long, MatchmakingStartMatchSearchResult>(Observable.ContinueWith<Unit, long>(Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				this._matchmakingClient.FindMatch("Normal");
			}), (Unit _) => this.WaitForTimeToPlayPredicted()), (long estimatedWaitTimeMinutes) => new MatchmakingStartMatchSearchResult
			{
				MatchKind = matchKind,
				EstimatedWaitTimeMinutes = estimatedWaitTimeMinutes
			});
		}

		private IObservable<long> WaitForTimeToPlayPredicted()
		{
			return Observable.Select<MatchmakingTimePredicted, long>(Observable.FromEvent<MatchmakingTimePredicted>(delegate(Action<MatchmakingTimePredicted> handler)
			{
				this._matchmakingClient.TimeToPlayPredicted += handler;
			}, delegate(Action<MatchmakingTimePredicted> handler)
			{
				this._matchmakingClient.TimeToPlayPredicted -= handler;
			}), (MatchmakingTimePredicted time) => (long)time.TimePredicted);
		}

		private IObservable<Unit> WaitForMatchMade()
		{
			return Observable.AsUnitObservable<MatchmakingArgs>(Observable.FromEvent<MatchmakingArgs>(delegate(Action<MatchmakingArgs> handler)
			{
				this._matchmakingClient.MatchMade += handler;
			}, delegate(Action<MatchmakingArgs> handler)
			{
				this._matchmakingClient.MatchMade -= handler;
			}));
		}

		public IObservable<MatchmakingStartedArgs> WaitForMatchStart()
		{
			IObservable<MatchmakingStartedArgs> observable = Observable.FromEvent<MatchmakingStartedArgs>(delegate(Action<MatchmakingStartedArgs> handler)
			{
				this._matchmakingClient.MatchStarted += handler;
			}, delegate(Action<MatchmakingStartedArgs> handler)
			{
				this._matchmakingClient.MatchStarted -= handler;
			});
			IObservable<MatchmakingStartedArgs>[] array = new IObservable<MatchmakingStartedArgs>[1];
			array[0] = Observable.Select<Unit, MatchmakingStartedArgs>(Observable.Do<Unit>(this.WaitForMatchError(), delegate(Unit _)
			{
				throw new MatchErrorException();
			}), (Unit _) => new MatchmakingStartedArgs());
			IObservable<MatchmakingStartedArgs> observable2 = Observable.Merge<MatchmakingStartedArgs>(observable, array);
			IObservable<MatchmakingStartedArgs>[] array2 = new IObservable<MatchmakingStartedArgs>[1];
			array2[0] = Observable.Select<Unit, MatchmakingStartedArgs>(Observable.Do<Unit>(this.WaitForMatchDisconnection(), delegate(Unit _)
			{
				throw new MatchMakingDisconnectException();
			}), (Unit _) => new MatchmakingStartedArgs());
			IObservable<MatchmakingStartedArgs> observable3 = Observable.Merge<MatchmakingStartedArgs>(observable2, array2);
			IObservable<MatchmakingStartedArgs>[] array3 = new IObservable<MatchmakingStartedArgs>[1];
			array3[0] = Observable.Select<MatchmakingArgs, MatchmakingStartedArgs>(Observable.Do<MatchmakingArgs>(this.WaitForMatchCancel(), delegate(MatchmakingArgs mmArgs)
			{
				throw new MatchMakingCanceledException(mmArgs.CauserUniversalId);
			}), (MatchmakingArgs _) => new MatchmakingStartedArgs());
			return Observable.First<MatchmakingStartedArgs>(Observable.Merge<MatchmakingStartedArgs>(observable3, array3));
		}

		public bool IsWaitingInQueue()
		{
			return this._matchmakingClient.IsWaitingInQueue();
		}

		public string GetCurrentQueueName()
		{
			return this._matchmakingClient.GetCurrentQueueName();
		}

		public void AcceptMatch()
		{
			this._matchmakingClient.Accept(this.GetCurrentQueueName());
		}

		public void DeclineMatch()
		{
			this._matchmakingClient.Decline(this.GetCurrentQueueName());
		}

		public void CancelSearch(string queueName)
		{
			this._matchmakingClient.CancelSearch(queueName);
		}

		private readonly ISwordfishMatchmakingWrapper _matchmakingClient;

		private readonly INetworkClient _networkClient;

		private readonly ISwordfishHubClientWrapper _swordfishHubClient;
	}
}
