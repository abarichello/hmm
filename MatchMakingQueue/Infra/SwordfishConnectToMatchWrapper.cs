using System;
using HeavyMetalMachines.Matches;
using HeavyMetalMachines.MatchMakingQueue.Infra.Exceptions;
using Hoplon.Logging;
using Pocketverse;
using UniRx;

namespace HeavyMetalMachines.MatchMakingQueue.Infra
{
	public class SwordfishConnectToMatchWrapper : GameHubObject, IConnectToMatch
	{
		public SwordfishConnectToMatchWrapper(ILogger<SwordfishConnectToMatchWrapper> logger)
		{
			this._logger = logger;
		}

		public IObservable<Unit> Connect(Match matchState)
		{
			GameHubObject.Hub.Server.ServerIp = matchState.Connection.Host;
			GameHubObject.Hub.Server.ServerPort = matchState.Connection.Port;
			GameHubObject.Hub.Swordfish.Msg.ClientMatchId = new Guid(matchState.MatchId);
			Subject<Unit> observation = new Subject<Unit>();
			this._logger.InfoFormat("Will Connect To Match. Ip={0} Port={1} MatchId={2}", new object[]
			{
				matchState.Connection.Host,
				matchState.Connection.Port,
				matchState.MatchId
			});
			GameHubObject.Hub.User.ConnectToServer(false, delegate
			{
				this.OnFailCallback(observation);
			}, delegate
			{
				this.OnSuccessCallback(observation);
			});
			return observation;
		}

		private void OnSuccessCallback(Subject<Unit> observation)
		{
			this._logger.Info("OnSuccessCallback");
			observation.OnNext(Unit.Default);
			observation.OnCompleted();
		}

		private void OnFailCallback(Subject<Unit> observation)
		{
			this._logger.Error("OnFailCallback");
			observation.OnError(new ClientDisconnectBeforeMatchException());
		}

		private readonly ILogger<SwordfishConnectToMatchWrapper> _logger;
	}
}
