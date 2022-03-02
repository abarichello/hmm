using System;
using System.Diagnostics;
using System.Linq;
using ClientAPI;
using ClientAPI.Exceptions;
using ClientAPI.Matchmaking;
using HeavyMetalMachines.Social.Groups.Models;
using Pocketverse;
using UniRx;

namespace HeavyMetalMachines.MatchMakingQueue.Infra
{
	public class SwordfishMatchmakingWrapper : ISwordfishMatchmakingWrapper
	{
		public SwordfishMatchmakingWrapper(MatchmakingClient client)
		{
			this._matchmakingClient = client;
			this._matchmakingClient.MatchError += new EventHandlerEx<MatchmakingErrorEventArgs>(this.MatchmakingClientOnMatchError);
			this._matchmakingClient.MatchMade += new EventHandler<MatchMadeEventArgs>(this.MatchmakingClientOnMatchMade);
			this._matchmakingClient.Disconnection += this.MatchmakingClientOnMatchDisconnection;
			this._matchmakingClient.MatchCanceled += new EventHandler<MatchCancelledArgs>(this.MatchmakingClientOnMatchCancel);
			this._matchmakingClient.MatchStarted += this.MatchmakingClientOnMatchStarted;
			this._matchmakingClient.MatchAccepted += this.OnMatchAccepted;
			this._matchmakingClient.MatchConfirmed += this.OnMatchConfirmed;
			this._matchmakingClient.NoServerAvailable += new EventHandlerEx<MatchmakingEventArgs>(this.OnNoServerAvailable);
			this._matchmakingClient.TimeToPlayPredicted += this.OnTimeToPlayPredicted;
		}

		~SwordfishMatchmakingWrapper()
		{
			if (this._matchmakingClient != null)
			{
				this._matchmakingClient.MatchError -= new EventHandlerEx<MatchmakingErrorEventArgs>(this.MatchmakingClientOnMatchError);
				this._matchmakingClient.MatchMade -= new EventHandler<MatchMadeEventArgs>(this.MatchmakingClientOnMatchMade);
				this._matchmakingClient.Disconnection -= this.MatchmakingClientOnMatchDisconnection;
				this._matchmakingClient.MatchCanceled -= new EventHandler<MatchCancelledArgs>(this.MatchmakingClientOnMatchCancel);
				this._matchmakingClient.MatchStarted -= this.MatchmakingClientOnMatchStarted;
				this._matchmakingClient.MatchAccepted -= this.OnMatchAccepted;
				this._matchmakingClient.MatchConfirmed -= this.OnMatchConfirmed;
				this._matchmakingClient.NoServerAvailable -= new EventHandlerEx<MatchmakingEventArgs>(this.OnNoServerAvailable);
				this._matchmakingClient.TimeToPlayPredicted -= this.OnTimeToPlayPredicted;
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<string> MatchAccepted;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<MatchmakingEventArgs> MatchConfirmed;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action NoServerAvailable;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<MatchmakingTimePredicted> TimeToPlayPredicted;

		public void PlayTournament(IObserver<Unit> observer, Group group, long tournamentStepId, string queueName, SwordfishClientApi.NetworkErrorCallback errorCallback)
		{
			string[] array = new string[group.Members.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = group.Members[i].UniversalId;
			}
			this._matchmakingClient.PlayTournament(observer, group.Guid, array, queueName, tournamentStepId, errorCallback, 0);
		}

		public void PlayTournamentSolo(IObserver<Unit> observer, string playerUniversalId, long tournamentStepId, string queueName, SwordfishClientApi.NetworkErrorCallback errorCallback)
		{
			this._matchmakingClient.PlayTournamentSolo(observer, playerUniversalId, queueName, tournamentStepId, errorCallback, 0);
		}

		public void FindMatch(string queueName)
		{
			this._matchmakingClient.PlaySolo(null, queueName, null);
		}

		public IObservable<MatchmakingStartedArgs> CreateMatch(string config)
		{
			string[] array = new string[]
			{
				GameHubBehaviour.Hub.User.UserSF.UniversalID
			};
			Subject<MatchmakingStartedArgs> subject = new Subject<MatchmakingStartedArgs>();
			this._matchmakingClient.PlayNow(subject, array, config, new SwordfishClientApi.NetworkErrorCallback(this.CreateMatchErrorCallback));
			IObservable<MatchmakingStartedArgs> observable = Observable.FromEvent<MatchmakingStartedArgs>(delegate(Action<MatchmakingStartedArgs> handler)
			{
				this.MatchStarted += handler;
			}, delegate(Action<MatchmakingStartedArgs> handler)
			{
				this.MatchStarted -= handler;
			});
			return Observable.Merge<MatchmakingStartedArgs>(new IObservable<MatchmakingStartedArgs>[]
			{
				subject,
				observable
			});
		}

		private void CreateMatchErrorCallback(object state, ConnectionException exception)
		{
			Subject<MatchmakingStartedArgs> subject = (Subject<MatchmakingStartedArgs>)state;
			subject.OnError(exception);
			subject.OnCompleted();
		}

		public event Action<MatchmakingArgs> MatchError
		{
			add
			{
				this._matchError = (Action<MatchmakingArgs>)Delegate.Combine(this._matchError, value);
			}
			remove
			{
				this._matchError = (Action<MatchmakingArgs>)Delegate.Remove(this._matchError, value);
			}
		}

		private void MatchmakingClientOnMatchError(object sender, MatchmakingEventArgs eventArgs)
		{
			SwordfishMatchmakingWrapper.Log.InfoFormat("MatchmakingClientOnMatchError. sender={0} eventArgs={1}", new object[]
			{
				sender,
				eventArgs
			});
			if (this._matchError != null)
			{
				this._matchError(SwordfishMatchmakingWrapper.ConvertMatchmakingEventArgs(eventArgs));
			}
		}

		public event Action<MatchmakingArgs> Disconnection
		{
			add
			{
				this._matchDisconnection = (Action<MatchmakingArgs>)Delegate.Combine(this._matchDisconnection, value);
			}
			remove
			{
				this._matchDisconnection = (Action<MatchmakingArgs>)Delegate.Remove(this._matchDisconnection, value);
			}
		}

		private void MatchmakingClientOnMatchDisconnection(object sender, MatchmakingEventArgs eventArgs)
		{
			SwordfishMatchmakingWrapper.Log.InfoFormat("MatchmakingClientOnMatchDisconnection. sender={0} eventArgs={1}", new object[]
			{
				sender,
				eventArgs
			});
			if (this._matchDisconnection != null)
			{
				this._matchDisconnection(SwordfishMatchmakingWrapper.ConvertMatchmakingEventArgs(eventArgs));
			}
		}

		public event Action<MatchmakingArgs> MatchMade
		{
			add
			{
				this._matchMade = (Action<MatchmakingArgs>)Delegate.Combine(this._matchMade, value);
			}
			remove
			{
				this._matchMade = (Action<MatchmakingArgs>)Delegate.Remove(this._matchMade, value);
			}
		}

		private void MatchmakingClientOnMatchMade(object sender, MatchmakingEventArgs eventArgs)
		{
			SwordfishMatchmakingWrapper.Log.InfoFormat("MatchmakingClientOnMatchMade. sender={0} eventArgs={1}", new object[]
			{
				sender,
				eventArgs
			});
			if (this._matchMade != null)
			{
				this._matchMade(SwordfishMatchmakingWrapper.ConvertMatchmakingEventArgs(eventArgs));
			}
		}

		public event Action<MatchmakingArgs> MatchCancel
		{
			add
			{
				this._matchCancel = (Action<MatchmakingArgs>)Delegate.Combine(this._matchCancel, value);
			}
			remove
			{
				this._matchCancel = (Action<MatchmakingArgs>)Delegate.Remove(this._matchCancel, value);
			}
		}

		private void MatchmakingClientOnMatchCancel(object sender, MatchmakingEventArgs eventArgs)
		{
			SwordfishMatchmakingWrapper.Log.InfoFormatStackTrace("MatchmakingClientOnMatchCancel. sender={0} eventArgs={1}", new object[]
			{
				sender,
				eventArgs
			});
			if (this._matchCancel != null)
			{
				this._matchCancel(SwordfishMatchmakingWrapper.ConvertMatchmakingEventArgs(eventArgs));
			}
		}

		public event Action<MatchmakingStartedArgs> MatchStarted
		{
			add
			{
				this._matchStarted = (Action<MatchmakingStartedArgs>)Delegate.Combine(this._matchStarted, value);
			}
			remove
			{
				this._matchStarted = (Action<MatchmakingStartedArgs>)Delegate.Remove(this._matchStarted, value);
			}
		}

		private void MatchmakingClientOnMatchStarted(object sender, MatchStartedEventArgs eventArgs)
		{
			SwordfishMatchmakingWrapper.Log.InfoFormat("MatchmakingClientOnMatchStarted. sender={0} eventArgs={1}", new object[]
			{
				sender,
				eventArgs
			});
			if (this._matchStarted != null)
			{
				this._matchStarted(SwordfishMatchmakingWrapper.ConvertMatchStartedEventArgs(eventArgs));
			}
		}

		private void OnMatchAccepted(object sender, MatchAcceptedArgs args)
		{
			if (this.MatchAccepted == null)
			{
				return;
			}
			this.MatchAccepted(args.Clients.Last<string>());
		}

		private void OnMatchConfirmed(object sender, MatchmakingEventArgs args)
		{
			if (this.MatchConfirmed == null)
			{
				return;
			}
			this.MatchConfirmed(args);
		}

		private void OnNoServerAvailable(object sender, MatchmakingEventArgs args)
		{
			if (this.NoServerAvailable == null)
			{
				return;
			}
			this.NoServerAvailable();
		}

		private void OnTimeToPlayPredicted(object sender, MatchmakingTimePredicted args)
		{
			if (this.TimeToPlayPredicted == null)
			{
				return;
			}
			this.TimeToPlayPredicted(args);
		}

		public bool IsWaitingInQueue()
		{
			return this._matchmakingClient.IsWaitingInQueue();
		}

		public string GetCurrentQueueName()
		{
			return this._matchmakingClient.GetCurrentQueueName();
		}

		public void Accept(string queueName)
		{
			this._matchmakingClient.Accept(queueName);
		}

		public void Decline(string queueName)
		{
			this._matchmakingClient.Decline(queueName);
		}

		public void CancelSearch(string queueName)
		{
			this._matchmakingClient.CancelSearch(queueName);
		}

		private static MatchmakingArgs ConvertMatchmakingEventArgs(MatchmakingEventArgs eventArgs)
		{
			MatchmakingArgs matchmakingArgs = new MatchmakingArgs();
			matchmakingArgs.QueueName = eventArgs.QueueName;
			if (eventArgs is MatchCancelledArgs)
			{
				MatchCancelledArgs matchCancelledArgs = eventArgs as MatchCancelledArgs;
				if (matchCancelledArgs.Clients.Length > 0)
				{
					matchmakingArgs.CauserUniversalId = matchCancelledArgs.Clients[0];
				}
			}
			return matchmakingArgs;
		}

		private static MatchmakingStartedArgs ConvertMatchStartedEventArgs(MatchStartedEventArgs eventArgs)
		{
			return new MatchmakingStartedArgs
			{
				Host = eventArgs.Host,
				MatchId = eventArgs.MatchId,
				Port = eventArgs.Port
			};
		}

		private static readonly BitLogger Log = new BitLogger(typeof(SwordfishMatchmakingWrapper));

		private readonly MatchmakingClient _matchmakingClient;

		private Action<MatchmakingArgs> _matchError;

		private Action<MatchmakingArgs> _matchDisconnection;

		private Action<MatchmakingArgs> _matchMade;

		private Action<MatchmakingArgs> _matchCancel;

		private Action<MatchmakingStartedArgs> _matchStarted;
	}
}
