using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ClientAPI.Service.Interfaces;
using HeavyMetalMachines.Competitive.DataTransferObjects;
using HeavyMetalMachines.PeriodicRefresh.DataTransferObjects;
using HeavyMetalMachines.Players.Presenting;
using HeavyMetalMachines.ReportSystem.DataTransferObjects;
using HeavyMetalMachines.ReportSystem.Infra;
using HeavyMetalMachines.Social.Teams.Models;
using HeavyMetalMachines.Swordfish;
using HeavyMetalMachines.Teams.DataTransferObjects;
using HeavyMetalMachines.Tournaments;
using HeavyMetalMachines.Tournaments.DataTransferObjects;
using Hoplon.Logging;
using UniRx;

namespace HeavyMetalMachines.PeriodicRefresh.Infra
{
	public class SwordfishPeriodicRefreshDataService : IPeriodicRefreshDataService
	{
		public SwordfishPeriodicRefreshDataService(ICustomWS customWs, IGetDisplayableNickName getDisplayableNickName, ILogger<SwordfishPeriodicRefreshDataService> logger)
		{
			this._customWs = customWs;
			this._getDisplayableNickName = getDisplayableNickName;
			this._logger = logger;
		}

		public IObservable<PeriodicRefreshData> Get()
		{
			return Observable.Select<PeriodicRefreshSerializableData, PeriodicRefreshData>(Observable.ContinueWith<PeriodicRefreshSerializableData, PeriodicRefreshSerializableData>(this.GetPeriodicRefreshData(), new Func<PeriodicRefreshSerializableData, IObservable<PeriodicRefreshSerializableData>>(this.UpdateNicknames)), new Func<PeriodicRefreshSerializableData, PeriodicRefreshData>(this.ConvertData));
		}

		private IObservable<PeriodicRefreshSerializableData> UpdateNicknames(PeriodicRefreshSerializableData periodicRefreshData)
		{
			return Observable.Select<DisplayableNicknameResult[], PeriodicRefreshSerializableData>(this._getDisplayableNickName.GetLatestFormattedNickName(this.ConvertTeamToDisplayableNicknameParameters(periodicRefreshData.Team)), (DisplayableNicknameResult[] results) => this.UpdateTeamMemberNames(periodicRefreshData, results));
		}

		private DisplayableNicknameParameters[] ConvertTeamToDisplayableNicknameParameters(SerializableTeam team)
		{
			if (team == null)
			{
				return new DisplayableNicknameParameters[0];
			}
			return (from member in team.Members
			select new DisplayableNicknameParameters
			{
				PlayerId = member.PlayerId,
				PlayerName = member.Nickname,
				UniversalId = member.UniversalId
			}).ToArray<DisplayableNicknameParameters>();
		}

		private PeriodicRefreshSerializableData UpdateTeamMemberNames(PeriodicRefreshSerializableData periodicRefreshData, DisplayableNicknameResult[] results)
		{
			if (periodicRefreshData.Team == null)
			{
				return periodicRefreshData;
			}
			foreach (SerializableTeamMember serializableTeamMember in periodicRefreshData.Team.Members)
			{
				foreach (DisplayableNicknameResult displayableNicknameResult in results)
				{
					if (displayableNicknameResult.PlayerId == serializableTeamMember.PlayerId)
					{
						serializableTeamMember.Nickname = displayableNicknameResult.LatestPlayerName;
					}
				}
			}
			return periodicRefreshData;
		}

		private IObservable<PeriodicRefreshSerializableData> GetPeriodicRefreshData()
		{
			return this._customWs.ExecuteAsObservable("GetPeriodicRefreshData", string.Empty);
		}

		private PeriodicRefreshData ConvertData(PeriodicRefreshSerializableData serializableData)
		{
			PeriodicRefreshData periodicRefreshData = new PeriodicRefreshData
			{
				TournamentTeamStatus = new List<TournamentTeamStatus>()
			};
			foreach (SerializableTournamentTeamStatus serializableTournamentTeamStatus in serializableData.TournamentTeamStatus)
			{
				periodicRefreshData.TournamentTeamStatus.Add(TournamentConversions.ToModel(serializableTournamentTeamStatus));
			}
			periodicRefreshData.Team = this.ConvertTeam(serializableData.Team);
			PeriodicRefreshData periodicRefreshData2 = periodicRefreshData;
			SerializableFeedback[] feedbacks = serializableData.Feedbacks;
			if (SwordfishPeriodicRefreshDataService.<>f__mg$cache0 == null)
			{
				SwordfishPeriodicRefreshDataService.<>f__mg$cache0 = new Converter<SerializableFeedback, IPlayerFeedbackInfo>(SwordfishPeriodicRefreshDataService.ConvertFeedback);
			}
			periodicRefreshData2.Feedbacks = new List<IPlayerFeedbackInfo>(Array.ConvertAll<SerializableFeedback, IPlayerFeedbackInfo>(feedbacks, SwordfishPeriodicRefreshDataService.<>f__mg$cache0));
			PeriodicRefreshData periodicRefreshData3 = periodicRefreshData;
			SerializableRestriction[] restrictions = serializableData.Restrictions;
			if (SwordfishPeriodicRefreshDataService.<>f__mg$cache1 == null)
			{
				SwordfishPeriodicRefreshDataService.<>f__mg$cache1 = new Converter<SerializableRestriction, IRestriction>(SwordfishPeriodicRefreshDataService.ConvertRestriction);
			}
			periodicRefreshData3.Restrictions = new List<IRestriction>(Array.ConvertAll<SerializableRestriction, IRestriction>(restrictions, SwordfishPeriodicRefreshDataService.<>f__mg$cache1));
			return periodicRefreshData;
		}

		private static IRestriction ConvertRestriction(SerializableRestriction input)
		{
			Restriction restriction = new Restriction();
			restriction.Initialize(input);
			return restriction;
		}

		private static IPlayerFeedbackInfo ConvertFeedback(SerializableFeedback input)
		{
			return new PlayerFeedbackInfo(input);
		}

		private Team ConvertTeam(SerializableTeam remoteTeam)
		{
			if (remoteTeam == null)
			{
				return null;
			}
			Team team = new Team
			{
				IconName = remoteTeam.IconName,
				Id = remoteTeam.Id,
				Name = remoteTeam.Name,
				Tag = remoteTeam.Tag,
				UserGeneratedContentCurrentOwnerUniversalId = remoteTeam.UserGeneratedContentCurrentOwnerUniversalId,
				UserGeneratedContentCurrentOwnerPlayerId = remoteTeam.UserGeneratedContentCurrentOwnerPlayerId
			};
			foreach (SerializableTeamMember remoteTeamMember in remoteTeam.Members)
			{
				team.AddMember(this.ConvertTeamMember(remoteTeamMember));
			}
			return team;
		}

		private static TournamentTeamStatus ConvertTournamentTeamStatus(SerializableTournamentTeamStatus remoteTournamentTeamStatus)
		{
			TournamentTeamStatus result = default(TournamentTeamStatus);
			result.TournamentId = remoteTournamentTeamStatus.TournamentId;
			result.BlockedStepIds = remoteTournamentTeamStatus.BlockedStepIds;
			result.PlayerCriterionResults = SwordfishPeriodicRefreshDataService.ConvertPlayerCriterionResultToList(remoteTournamentTeamStatus.PlayerCriterionResults);
			result.IsSubscribedInThisTournament = remoteTournamentTeamStatus.IsSubscribedInThisTournament;
			result.IsSubscribedToAnotherTournamentOnSameRegion = remoteTournamentTeamStatus.IsSubscribedToAnotherTournamentOnSameRegion;
			return result;
		}

		private TeamMember ConvertTeamMember(SerializableTeamMember remoteTeamMember)
		{
			return new TeamMember
			{
				UniversalId = remoteTeamMember.UniversalId,
				IsLeader = remoteTeamMember.IsLeader,
				Nickname = remoteTeamMember.Nickname,
				PlayerId = remoteTeamMember.PlayerId,
				PlayerTag = remoteTeamMember.PlayerTag,
				PortraitAssetName = string.Empty,
				IsBannedFromTournament = remoteTeamMember.IsBannedFromTournament,
				IsCrossPlayEnable = remoteTeamMember.IsCrossPlayEnable,
				Publisher = remoteTeamMember.Publisher
			};
		}

		private static List<PlayerCriterionResult> ConvertPlayerCriterionResultToList(SerializablePlayerCriteriaResult[] criteriaResults)
		{
			if (SwordfishPeriodicRefreshDataService.<>f__mg$cache2 == null)
			{
				SwordfishPeriodicRefreshDataService.<>f__mg$cache2 = new Func<SerializablePlayerCriteriaResult, PlayerCriterionResult>(SwordfishPeriodicRefreshDataService.ConvertPlayerCriterionResult);
			}
			return criteriaResults.Select(SwordfishPeriodicRefreshDataService.<>f__mg$cache2).ToList<PlayerCriterionResult>();
		}

		private static PlayerCriterionResult ConvertPlayerCriterionResult(SerializablePlayerCriteriaResult criteriaResult)
		{
			PlayerCriterionResult result = default(PlayerCriterionResult);
			result.IsCompetitiveCriteriaMet = criteriaResult.IsCompetitiveCriteriaMet;
			result.IsMet = criteriaResult.IsMet;
			result.IsVictoriesCriteriaMet = criteriaResult.IsVictoriesCriteriaMet;
			result.CompetitiveRank = CompetitiveSerializableExtensions.ToModel(criteriaResult.CompetitiveRank);
			result.PlayerId = criteriaResult.PlayerId;
			result.PlayerVictories = criteriaResult.PlayerVictories;
			return result;
		}

		private readonly ICustomWS _customWs;

		private readonly IGetDisplayableNickName _getDisplayableNickName;

		private readonly ILogger<SwordfishPeriodicRefreshDataService> _logger;

		[CompilerGenerated]
		private static Converter<SerializableFeedback, IPlayerFeedbackInfo> <>f__mg$cache0;

		[CompilerGenerated]
		private static Converter<SerializableRestriction, IRestriction> <>f__mg$cache1;

		[CompilerGenerated]
		private static Func<SerializablePlayerCriteriaResult, PlayerCriterionResult> <>f__mg$cache2;
	}
}
