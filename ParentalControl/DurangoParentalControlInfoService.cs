using System;
using System.Collections.Generic;
using System.Linq;
using ClientAPI;
using ClientAPI.Objects;
using ClientAPI.Objects.Custom;
using ClientAPI.Service.Interfaces;
using ClientAPI.Utils;
using HeavyMetalMachines.Players.Business;
using HeavyMetalMachines.Social.Friends.Business;
using HeavyMetalMachines.Swordfish;
using Hoplon.Logging;
using Pocketverse;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.ParentalControl
{
	public class DurangoParentalControlInfoService : IParentalControlInfoService
	{
		public DurangoParentalControlInfoService(IParentalControl parentalControlService, IPlayerToAllPlayersRestriction playerToAllPlayersRestriction, IIsPlayerLocalPlayerFriend isFriend, ILogger<DurangoParentalControlInfoService> logger)
		{
			this._parentalControlService = parentalControlService;
			this._playerToAllPlayersRestriction = playerToAllPlayersRestriction;
			this._isFriend = isFriend;
			this._logger = logger;
		}

		public IObservable<ParentalControlInfo> GetGlobalParentalControlInfo()
		{
			return Observable.Defer<ParentalControlInfo>(delegate()
			{
				ParentalControlInfo parentalControlInfoSync = this._parentalControlService.GetParentalControlInfoSync();
				return Observable.Return<ParentalControlInfo>(DurangoParentalControlInfoService.ConvertToParentalControlInfo(parentalControlInfoSync));
			});
		}

		public IObservable<Dictionary<PlayerIdentification, ParentalControlInfo>> GetParentalControlInfoByUniversalIds(PlayerIdentification[] targetPlayers)
		{
			List<PlayerIdentification> xboxPlayers = DurangoParentalControlInfoService.GetXboxPlayers(targetPlayers);
			return Observable.DoOnError<Dictionary<PlayerIdentification, ParentalControlInfo>>(Observable.ContinueWith<Dictionary<PlayerIdentification, ParentalControlInfo>, Dictionary<PlayerIdentification, ParentalControlInfo>>(Observable.Select<CheckUserToUsersRestrictionsResults, Dictionary<PlayerIdentification, ParentalControlInfo>>(SwordfishObservable.FromSwordfishCall<CheckUserToUsersRestrictionsResults>(delegate(SwordfishClientApi.ParameterizedCallback<CheckUserToUsersRestrictionsResults> success, SwordfishClientApi.ErrorCallback error)
			{
				List<string> list = (from p in xboxPlayers
				select p.UniversalId).ToList<string>();
				list.Add("crossNetworkUser");
				list.Add("crossNetworkFriend");
				this._parentalControlService.CheckUserToUsersRestrictions(null, list.ToArray(), success, error);
			}), (CheckUserToUsersRestrictionsResults results) => this.ConvertToParentalControlInfoByPlayers(targetPlayers, results.Results)), (Dictionary<PlayerIdentification, ParentalControlInfo> results) => this.UpdateNonXboxPlayersRestrictions(targetPlayers, results)), delegate(Exception ex)
			{
				Debug.LogError("error while getting parental control info from SF. " + ex.Message);
			});
		}

		private IObservable<Dictionary<PlayerIdentification, ParentalControlInfo>> UpdateNonXboxPlayersRestrictions(PlayerIdentification[] players, Dictionary<PlayerIdentification, ParentalControlInfo> allRestrictions)
		{
			return Observable.DoOnError<Dictionary<PlayerIdentification, ParentalControlInfo>>(Observable.Select<PlayerToAllPlayersRestriction[], Dictionary<PlayerIdentification, ParentalControlInfo>>(SwordfishObservable.FromSwordfishCall<PlayerToAllPlayersRestriction[]>(delegate(SwordfishClientApi.ParameterizedCallback<PlayerToAllPlayersRestriction[]> success, SwordfishClientApi.ErrorCallback error)
			{
				IEnumerable<PlayerIdentification> nonXboxLivePlayers = DurangoParentalControlInfoService.GetNonXboxLivePlayers(players);
				string[] array = (from p in nonXboxLivePlayers
				select p.UniversalId).ToArray<string>();
				this._playerToAllPlayersRestriction.GetPlayersByUniversalId(null, array, success, error);
			}), (PlayerToAllPlayersRestriction[] results) => this.MergeParentalControlInfos(players, results, allRestrictions)), delegate(Exception ex)
			{
				this._logger.Error("error while UpdateNonXboxPlayersRestrictions parental control info from SF. " + ex.Message);
			});
		}

		private Dictionary<PlayerIdentification, ParentalControlInfo> MergeParentalControlInfos(IEnumerable<PlayerIdentification> nonXboxPlayers, IEnumerable<PlayerToAllPlayersRestriction> UGCRestrictionResult, Dictionary<PlayerIdentification, ParentalControlInfo> allRestrictions)
		{
			using (IEnumerator<PlayerIdentification> enumerator = nonXboxPlayers.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					PlayerIdentification player = enumerator.Current;
					PlayerToAllPlayersRestriction playerToAllPlayersRestriction = UGCRestrictionResult.FirstOrDefault((PlayerToAllPlayersRestriction r) => r.UniversalId.Equals(player.UniversalId));
					this._logger.DebugFormat("checking player {0}, find restriction? = {1}, Is restricted? = {2}", new object[]
					{
						player.UniversalId,
						playerToAllPlayersRestriction != null,
						playerToAllPlayersRestriction != null && playerToAllPlayersRestriction.IsRestriction
					});
					KeyValuePair<PlayerIdentification, ParentalControlInfo> keyValuePair = allRestrictions.FirstOrDefault((KeyValuePair<PlayerIdentification, ParentalControlInfo> identification) => identification.Key.UniversalId.Equals(player.UniversalId));
					if (!keyValuePair.Value.RestrictUserGeneratedContent)
					{
						this._logger.Debug("Updating UGC info");
						keyValuePair.Value.RestrictUserGeneratedContent = (playerToAllPlayersRestriction != null && playerToAllPlayersRestriction.IsRestriction);
					}
				}
			}
			return allRestrictions;
		}

		private static List<PlayerIdentification> GetXboxPlayers(IEnumerable<PlayerIdentification> targetPlayers)
		{
			return (from player in targetPlayers
			where UniversalIdUtil.IsXboxLiveId(player.UniversalId)
			select player).ToList<PlayerIdentification>();
		}

		private Dictionary<PlayerIdentification, ParentalControlInfo> ConvertToParentalControlInfoByPlayers(IEnumerable<PlayerIdentification> targetPlayers, IEnumerable<CheckUserToUserRestrictionResult> checkResults)
		{
			foreach (PlayerIdentification playerIdentification in targetPlayers)
			{
				DurangoParentalControlInfoService.Log.DebugFormat("[TOMATE] player {0}", new object[]
				{
					playerIdentification.UniversalId
				});
			}
			Dictionary<string, ParentalControlInfo> dictionary = targetPlayers.ToDictionary((PlayerIdentification p) => p.UniversalId, (PlayerIdentification p) => new ParentalControlInfo());
			IEnumerable<PlayerIdentification> nonXboxLivePlayers = DurangoParentalControlInfoService.GetNonXboxLivePlayers(targetPlayers);
			CheckUserToUserRestrictionResult checkUserToUserRestrictionResult = null;
			CheckUserToUserRestrictionResult checkUserToUserRestrictionResult2 = null;
			using (IEnumerator<CheckUserToUserRestrictionResult> enumerator2 = checkResults.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					CheckUserToUserRestrictionResult checkResult = enumerator2.Current;
					DurangoParentalControlInfoService.Log.DebugFormat("[TOMATE] xbox player result: name {0}, text: {1} voice: {2} ugc: {3}", new object[]
					{
						checkResult.UniversalId,
						checkResult.IsTextRestricted,
						checkResult.IsVoiceRestricted,
						checkResult.IsUGCRestricted
					});
					PlayerIdentification playerIdentification2 = targetPlayers.FirstOrDefault((PlayerIdentification p) => p.UniversalId.Equals(checkResult.UniversalId));
					if (playerIdentification2 != null)
					{
						dictionary[playerIdentification2.UniversalId].RestrictUserGeneratedContent = checkResult.IsUGCRestricted;
						dictionary[playerIdentification2.UniversalId].RestrictTextChat = checkResult.IsTextRestricted;
						dictionary[playerIdentification2.UniversalId].RestrictVoiceChat = checkResult.IsVoiceRestricted;
					}
					else if (checkResult.UniversalId.Equals("crossNetworkUser"))
					{
						checkUserToUserRestrictionResult = checkResult;
					}
					else if (checkResult.UniversalId.Equals("crossNetworkFriend"))
					{
						checkUserToUserRestrictionResult2 = checkResult;
					}
				}
			}
			foreach (string text in dictionary.Keys)
			{
				DurangoParentalControlInfoService.Log.DebugFormat("[TOMATE] player universal ids {0}", new object[]
				{
					text
				});
			}
			foreach (PlayerIdentification playerIdentification3 in nonXboxLivePlayers)
			{
				if (this._isFriend.IsFriend(playerIdentification3.UniversalId))
				{
					DurangoParentalControlInfoService.Log.DebugFormat("[TOMATE] non xbox players friend result: name {0}, text: {1} voice: {2} ugc: {3} hash: {4}", new object[]
					{
						playerIdentification3.UniversalId,
						checkUserToUserRestrictionResult2.IsTextRestricted,
						checkUserToUserRestrictionResult2.IsVoiceRestricted,
						checkUserToUserRestrictionResult2.IsUGCRestricted,
						playerIdentification3.GetHashCode()
					});
					dictionary[playerIdentification3.UniversalId].RestrictUserGeneratedContent = checkUserToUserRestrictionResult2.IsUGCRestricted;
					dictionary[playerIdentification3.UniversalId].RestrictTextChat = checkUserToUserRestrictionResult2.IsTextRestricted;
					dictionary[playerIdentification3.UniversalId].RestrictVoiceChat = checkUserToUserRestrictionResult2.IsVoiceRestricted;
				}
				else
				{
					DurangoParentalControlInfoService.Log.DebugFormat("[TOMATE] non xbox players user result: name {0}, text: {1} voice: {2} ugc: {3} hash: {4}", new object[]
					{
						playerIdentification3.UniversalId,
						checkUserToUserRestrictionResult.IsTextRestricted,
						checkUserToUserRestrictionResult.IsVoiceRestricted,
						checkUserToUserRestrictionResult.IsUGCRestricted,
						playerIdentification3.GetHashCode()
					});
					dictionary[playerIdentification3.UniversalId].RestrictUserGeneratedContent = checkUserToUserRestrictionResult.IsUGCRestricted;
					dictionary[playerIdentification3.UniversalId].RestrictTextChat = checkUserToUserRestrictionResult.IsTextRestricted;
					dictionary[playerIdentification3.UniversalId].RestrictVoiceChat = checkUserToUserRestrictionResult.IsVoiceRestricted;
				}
			}
			DurangoParentalControlInfoService.Log.Debug("[TOMATE] saiu convert");
			return dictionary.ToDictionary((KeyValuePair<string, ParentalControlInfo> pair) => targetPlayers.First((PlayerIdentification p) => p.UniversalId.Equals(pair.Key)), (KeyValuePair<string, ParentalControlInfo> pair) => pair.Value);
		}

		private static IEnumerable<PlayerIdentification> GetNonXboxLivePlayers(IEnumerable<PlayerIdentification> targetPlayers)
		{
			return from p in targetPlayers
			where !UniversalIdUtil.IsXboxLiveId(p.UniversalId)
			select p;
		}

		private static ParentalControlInfo ConvertToParentalControlInfo(ParentalControlInfo p)
		{
			return new ParentalControlInfo
			{
				RestrictAllChat = p.AllChatRestriction,
				RestrictVoiceChat = p.VoiceChatRestriction,
				RestrictTextChat = p.TextChatRestriction,
				RestrictUserGeneratedContent = p.UgcRestriction,
				RestrictCrossPlay = p.CrossPlayRestriction
			};
		}

		private static readonly BitLogger Log = new BitLogger(typeof(DurangoParentalControlInfoService));

		private readonly IParentalControl _parentalControlService;

		private readonly IPlayerToAllPlayersRestriction _playerToAllPlayersRestriction;

		private readonly IIsPlayerLocalPlayerFriend _isFriend;

		private readonly ILogger<DurangoParentalControlInfoService> _logger;

		private const string CrossNetworkUser = "crossNetworkUser";

		private const string CrossNetworkFriend = "crossNetworkFriend";
	}
}
