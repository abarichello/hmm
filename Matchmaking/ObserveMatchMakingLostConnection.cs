using System;
using ClientAPI;
using ClientAPI.Publisher3rdp.Contracts;
using Hoplon.Logging;
using UniRx;

namespace HeavyMetalMachines.MatchMaking
{
	public class ObserveMatchMakingLostConnection : IObserveMatchMakingLostConnection
	{
		public ObserveMatchMakingLostConnection(ILogger<ObserveMatchMakingLostConnection> logger, IPublisherNetwork publisherNetwork, IIsMatchMakingConnected isMatchMakingConnected)
		{
			this._logger = logger;
			this._publisherNetwork = publisherNetwork;
			this._isMatchMakingConnected = isMatchMakingConnected;
		}

		public IObservable<Unit> Observe()
		{
			return Observable.Merge<Unit>(this.ObserveIsMatchMakingConnected(), new IObservable<Unit>[]
			{
				this.ObserveLostConnectionEvent()
			});
		}

		private IObservable<Unit> ObserveIsMatchMakingConnected()
		{
			return Observable.AsUnitObservable<bool>(Observable.Do<bool>(Observable.Where<bool>(Observable.Select<long, bool>(Observable.Interval(TimeSpan.FromSeconds((double)this._matchMakingDisconnectionCheckDuration)), (long _) => this._isMatchMakingConnected.IsConnected()), (bool value) => !value), delegate(bool _)
			{
				this._logger.Info("Detected connection lost to matchmaking.");
			}));
		}

		private IObservable<Unit> ObserveLostConnectionEvent()
		{
			return Observable.SelectMany<Unit, Unit>(Observable.FromEvent<Action>((Action h) => new Action(h.Invoke), delegate(Action h)
			{
				this._publisherNetwork.OnLostConnection += h;
			}, delegate(Action h)
			{
				this._publisherNetwork.OnLostConnection -= h;
			}), delegate(Unit _)
			{
				this._logger.Debug("Lost internet Connection");
				return Observable.ReturnUnit();
			});
		}

		private readonly ILogger<ObserveMatchMakingLostConnection> _logger;

		private readonly IPublisherNetwork _publisherNetwork;

		private readonly float _matchMakingDisconnectionCheckDuration = 0.5f;

		private readonly IIsMatchMakingConnected _isMatchMakingConnected;
	}
}
