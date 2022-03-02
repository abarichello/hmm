using System;
using ClientAPI.Exceptions;
using ClientAPI.Service.Interfaces;
using HeavyMetalMachines.DataTransferObjects.Result;
using HeavyMetalMachines.Swordfish;
using HeavyMetalMachines.Tournaments.DataTransferObjects;
using UniRx;

namespace HeavyMetalMachines.Tournaments.Infra
{
	public class SwordfishTeamTournamentSubscriptionService : ITeamTournamentSubscriptionService
	{
		public SwordfishTeamTournamentSubscriptionService(ICustomWS customWs)
		{
			this._customWs = customWs;
		}

		public IObservable<Unit> Subscribe(Guid teamId, long tournamentId)
		{
			return this.ExecuteCustomWsCall("SubscribeTeamTournament", teamId, tournamentId);
		}

		public IObservable<Unit> Unsubscribe(Guid teamId, long tournamentId)
		{
			return this.ExecuteCustomWsCall("UnsubscribeTeamTournament", teamId, tournamentId);
		}

		private IObservable<Unit> ExecuteCustomWsCall(string methodName, Guid teamId, long tournamentId)
		{
			return Observable.Select<NetResult, Unit>(this._customWs.ExecuteAsObservable(methodName, this.GetSerializedTeamTournamentSubscription(teamId, tournamentId)), delegate(NetResult netResult)
			{
				if (!netResult.Success)
				{
					throw new ConfigApiException(netResult.Msg);
				}
				return Unit.Default;
			});
		}

		private string GetSerializedTeamTournamentSubscription(Guid teamId, long tournamentId)
		{
			SerializableTeamTournamentSubscription serializableTeamTournamentSubscription = new SerializableTeamTournamentSubscription
			{
				TeamId = teamId,
				TournamentId = tournamentId
			};
			return serializableTeamTournamentSubscription.Serialize();
		}

		private readonly ICustomWS _customWs;
	}
}
