using System;
using ClientAPI;
using HeavyMetalMachines.DataTransferObjects.Result;
using HeavyMetalMachines.DataTransferObjects.Tournament;
using HeavyMetalMachines.Tournaments.DataTransferObjects;
using Pocketverse;
using UniRx;

namespace HeavyMetalMachines.Swordfish
{
	public class TournamentCustomWS : GameHubObject
	{
		public static IObservable<TournamentRanking> GetTournamentRankByTournamentIdDetailed(TournamentRankingParameters tournamentRankingParameters)
		{
			return SwordfishObservable.FromStringSwordfishCall<TournamentRanking>(delegate(SwordfishClientApi.ParameterizedCallback<string> success, SwordfishClientApi.ErrorCallback error)
			{
				GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "GetTournamentRankByTournamentIdDetailed", (string)tournamentRankingParameters, success, error);
			});
		}

		public static IObservable<TournamentRanking> GetStepRankByStepIdDetailed(TournamentStepRankingParameters tournamentStepRankingParameters)
		{
			return SwordfishObservable.FromStringSwordfishCall<TournamentRanking>(delegate(SwordfishClientApi.ParameterizedCallback<string> success, SwordfishClientApi.ErrorCallback error)
			{
				GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "GetStepRankByStepIdDetailed", (string)tournamentStepRankingParameters, success, error);
			});
		}

		public static IObservable<TournamentRank> GetMyTeamTournamentRankByTournamentIdDetailed(long tournamentId)
		{
			return SwordfishObservable.FromStringSwordfishCall<TournamentRank>(delegate(SwordfishClientApi.ParameterizedCallback<string> success, SwordfishClientApi.ErrorCallback error)
			{
				GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "GetMyTeamTournamentRankByTournamentIdDetailed", tournamentId.ToString(), success, error);
			});
		}

		public static IObservable<TournamentRank> GetMyTeamStepRankByStepIdDetailed(long stepId)
		{
			return SwordfishObservable.FromStringSwordfishCall<TournamentRank>(delegate(SwordfishClientApi.ParameterizedCallback<string> success, SwordfishClientApi.ErrorCallback error)
			{
				GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "GetMyTeamStepRankByStepIdDetailed", stepId.ToString(), success, error);
			});
		}

		public static IObservable<NetResult> MarkTournamentFirstSeen()
		{
			return SwordfishObservable.FromStringSwordfishCall<NetResult>(delegate(SwordfishClientApi.ParameterizedCallback<string> success, SwordfishClientApi.ErrorCallback error)
			{
				GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "MarkTournamentFirstSeen", string.Empty, success, error);
			});
		}
	}
}
