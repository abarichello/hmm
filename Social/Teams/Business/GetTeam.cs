using System;
using System.Runtime.CompilerServices;
using ClientAPI;
using ClientAPI.Service.Interfaces;
using HeavyMetalMachines.Publishing.Presenting;
using HeavyMetalMachines.Social.DataTransferObjects;
using HeavyMetalMachines.Swordfish;
using HeavyMetalMachines.Teams.DataTransferObjects;
using Hoplon.Serialization;
using UniRx;

namespace HeavyMetalMachines.Social.Teams.Business
{
	public class GetTeam : IGetTeam
	{
		public GetTeam(ICustomWS customWs, IGetDisplayablePublisherUserName getDisplayablePublisherUserName)
		{
			this._customWs = customWs;
			this._getDisplayablePublisherUserName = getDisplayablePublisherUserName;
		}

		public IObservable<TeamInfo> Get(long playerId)
		{
			IObservable<SerializablePlayerProfileInfo> observable = GetTeam.ExecuteAsObservable<SerializablePlayerProfileInfo>(this._customWs, "GetPlayerProfileInfo", playerId.ToString());
			if (GetTeam.<>f__mg$cache0 == null)
			{
				GetTeam.<>f__mg$cache0 = new Func<SerializablePlayerProfileInfo, TeamInfo>(GetTeam.ConvertSerializablePlayerProfileInfoIntoTeamInfo);
			}
			return Observable.ContinueWith<TeamInfo, TeamInfo>(Observable.Select<SerializablePlayerProfileInfo, TeamInfo>(observable, GetTeam.<>f__mg$cache0), new Func<TeamInfo, IObservable<TeamInfo>>(this.FetchUserGeneratedContentCurrentOwnerPublisherUserName));
		}

		private static IObservable<TResultType> ExecuteAsObservable<TResultType>(ICustomWS customWs, string methodName, string argument) where TResultType : JsonSerializeable<TResultType>, new()
		{
			return SwordfishObservable.FromStringSwordfishCall<TResultType>(delegate(SwordfishClientApi.ParameterizedCallback<string> success, SwordfishClientApi.ErrorCallback error)
			{
				customWs.ExecuteCustomWSWithReturn(null, methodName, argument, success, error);
			});
		}

		private static TeamInfo ConvertSerializablePlayerProfileInfoIntoTeamInfo(SerializablePlayerProfileInfo serializablePlayerProfileInfo)
		{
			if (serializablePlayerProfileInfo.TeamInfo == null)
			{
				return null;
			}
			return new TeamInfo
			{
				Id = serializablePlayerProfileInfo.TeamInfo.Id,
				Tag = serializablePlayerProfileInfo.TeamInfo.TeamTag,
				Name = serializablePlayerProfileInfo.TeamInfo.TeamName,
				IconName = serializablePlayerProfileInfo.TeamInfo.TeamIconImage,
				UserGeneratedContentCurrentOwnerUniversalId = serializablePlayerProfileInfo.TeamInfo.CurrentUgmUserUniversalId
			};
		}

		private IObservable<TeamInfo> FetchUserGeneratedContentCurrentOwnerPublisherUserName(TeamInfo teamInfo)
		{
			if (teamInfo == null)
			{
				return Observable.Return<TeamInfo>(null);
			}
			return Observable.ContinueWith<string, TeamInfo>(Observable.Do<string>(this._getDisplayablePublisherUserName.GetAsTeamUgcOwner(teamInfo.UserGeneratedContentCurrentOwnerUniversalId), delegate(string displayablePublisherUserName)
			{
				teamInfo.UserGeneratedContentCurrentOwnerPublisherUserName = displayablePublisherUserName;
			}), Observable.Return<TeamInfo>(teamInfo));
		}

		private readonly ICustomWS _customWs;

		private readonly IGetDisplayablePublisherUserName _getDisplayablePublisherUserName;

		[CompilerGenerated]
		private static Func<SerializablePlayerProfileInfo, TeamInfo> <>f__mg$cache0;
	}
}
