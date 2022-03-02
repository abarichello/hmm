using System;
using ClientAPI.Exceptions;
using ClientAPI.Service.Interfaces;
using HeavyMetalMachines.DataTransferObjects.Result;
using HeavyMetalMachines.Swordfish;
using HeavyMetalMachines.Tournaments.DataTransferObjects;
using UniRx;

namespace HeavymetalMachines.Tournaments.Infra
{
	public class IsTeamSubscribedInTournamentService : IIsTeamSubscribedInTournamentService
	{
		public IsTeamSubscribedInTournamentService(ICustomWS customWs)
		{
			this._customWs = customWs;
		}

		public IObservable<bool> IsTeamSubscribedInActiveTournament(Guid teamId, long tournamentId)
		{
			return this.IsTeamSubscribedCustomWsCall("IsTeamSubscribedInActiveTournament", teamId, tournamentId);
		}

		private IObservable<bool> IsTeamSubscribedCustomWsCall(string methodName, Guid teamId, long tournamentId)
		{
			return Observable.Select<NetResult, bool>(this._customWs.ExecuteAsObservable(methodName, this.GetSerializedTeamTournamentSubscription(teamId, tournamentId)), delegate(NetResult netResult)
			{
				if (!netResult.Success)
				{
					throw new ConfigApiException(netResult.Msg);
				}
				return netResult.Msg == true.ToString();
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
