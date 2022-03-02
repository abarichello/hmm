using System;
using System.Collections.Generic;
using System.Linq;
using ClientAPI;
using ClientAPI.Objects;
using ClientAPI.Objects.Custom;
using ClientAPI.Service.Interfaces;
using HeavyMetalMachines.Players.Business;
using HeavyMetalMachines.Swordfish;
using Hoplon.Logging;
using UniRx;

namespace HeavyMetalMachines.ParentalControl
{
	public class OrbisParentalControlInfoService : IParentalControlInfoService
	{
		public OrbisParentalControlInfoService(IParentalControl parentalControlService, IPlayerToAllPlayersRestriction playerToAllPlayersRestriction, ILogger<OrbisParentalControlInfoService> logger)
		{
			this._parentalControlService = parentalControlService;
			this._playerToAllPlayersRestriction = playerToAllPlayersRestriction;
			this._logger = logger;
		}

		public IObservable<ParentalControlInfo> GetGlobalParentalControlInfo()
		{
			return Observable.Defer<ParentalControlInfo>(delegate()
			{
				ParentalControlInfo parentalControlInfoSync = this._parentalControlService.GetParentalControlInfoSync();
				return Observable.Return<ParentalControlInfo>(OrbisParentalControlInfoService.ConvertToParentalControlInfo(parentalControlInfoSync));
			});
		}

		public IObservable<Dictionary<PlayerIdentification, ParentalControlInfo>> GetParentalControlInfoByUniversalIds(PlayerIdentification[] players)
		{
			return Observable.DoOnError<Dictionary<PlayerIdentification, ParentalControlInfo>>(Observable.Select<Dictionary<PlayerIdentification, ParentalControlInfo>, Dictionary<PlayerIdentification, ParentalControlInfo>>(Observable.Select<PlayerToAllPlayersRestriction[], Dictionary<PlayerIdentification, ParentalControlInfo>>(SwordfishObservable.FromSwordfishCall<PlayerToAllPlayersRestriction[]>(delegate(SwordfishClientApi.ParameterizedCallback<PlayerToAllPlayersRestriction[]> success, SwordfishClientApi.ErrorCallback error)
			{
				string[] array = (from p in players
				select p.UniversalId).ToArray<string>();
				this._playerToAllPlayersRestriction.GetPlayersByUniversalId(null, array, success, error);
			}), (PlayerToAllPlayersRestriction[] results) => this.ConvertToParentalControlInfoByPlayers(players, results)), new Func<Dictionary<PlayerIdentification, ParentalControlInfo>, Dictionary<PlayerIdentification, ParentalControlInfo>>(this.UpdateAllRestrictions)), delegate(Exception ex)
			{
				this._logger.Error("error while getting parental control info from SF. " + ex.Message);
			});
		}

		private Dictionary<PlayerIdentification, ParentalControlInfo> UpdateAllRestrictions(Dictionary<PlayerIdentification, ParentalControlInfo> allRestrictions)
		{
			ParentalControlInfo parentalControlInfoSync = this._parentalControlService.GetParentalControlInfoSync();
			foreach (KeyValuePair<PlayerIdentification, ParentalControlInfo> keyValuePair in allRestrictions)
			{
				keyValuePair.Value.RestrictAllChat = parentalControlInfoSync.AllChatRestriction;
				keyValuePair.Value.RestrictVoiceChat = parentalControlInfoSync.AllChatRestriction;
				keyValuePair.Value.RestrictTextChat = parentalControlInfoSync.AllChatRestriction;
				keyValuePair.Value.RestrictCrossPlay = parentalControlInfoSync.CrossPlayRestriction;
				if (parentalControlInfoSync.UgcRestriction)
				{
					keyValuePair.Value.RestrictUserGeneratedContent = parentalControlInfoSync.UgcRestriction;
				}
				this._logger.DebugFormat("updating player {0}, AllChatRestriction= {1}, RestrictVoiceChat= {2}, RestrictTextChat= {3}, RestrictCrossPlay= {4}, UgcRestriction={5}", new object[]
				{
					keyValuePair.Key,
					keyValuePair.Value.RestrictAllChat,
					keyValuePair.Value.RestrictVoiceChat,
					keyValuePair.Value.RestrictTextChat,
					keyValuePair.Value.RestrictCrossPlay,
					keyValuePair.Value.RestrictUserGeneratedContent
				});
			}
			return allRestrictions;
		}

		private Dictionary<PlayerIdentification, ParentalControlInfo> ConvertToParentalControlInfoByPlayers(IEnumerable<PlayerIdentification> players, IEnumerable<PlayerToAllPlayersRestriction> restrictionResult)
		{
			Dictionary<string, ParentalControlInfo> dictionary = players.ToDictionary((PlayerIdentification p) => p.UniversalId, (PlayerIdentification p) => new ParentalControlInfo());
			using (Dictionary<string, ParentalControlInfo>.Enumerator enumerator = dictionary.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					KeyValuePair<string, ParentalControlInfo> player = enumerator.Current;
					PlayerToAllPlayersRestriction playerToAllPlayersRestriction = restrictionResult.FirstOrDefault((PlayerToAllPlayersRestriction r) => r.UniversalId.Equals(player.Key));
					this._logger.DebugFormat("checking player {0}, find restriction? = {1}, Is restricted? = {2}", new object[]
					{
						player.Key,
						playerToAllPlayersRestriction != null,
						playerToAllPlayersRestriction != null && playerToAllPlayersRestriction.IsRestriction
					});
					player.Value.RestrictUserGeneratedContent = (playerToAllPlayersRestriction != null && playerToAllPlayersRestriction.IsRestriction);
				}
			}
			return dictionary.ToDictionary((KeyValuePair<string, ParentalControlInfo> pair) => players.First((PlayerIdentification p) => p.UniversalId.Equals(pair.Key)), (KeyValuePair<string, ParentalControlInfo> pair) => pair.Value);
		}

		private static ParentalControlInfo ConvertToParentalControlInfo(ParentalControlInfo p)
		{
			return new ParentalControlInfo
			{
				RestrictAllChat = p.AllChatRestriction,
				RestrictVoiceChat = p.AllChatRestriction,
				RestrictTextChat = p.AllChatRestriction,
				RestrictUserGeneratedContent = p.UgcRestriction,
				RestrictCrossPlay = p.CrossPlayRestriction
			};
		}

		private readonly IParentalControl _parentalControlService;

		private readonly IPlayerToAllPlayersRestriction _playerToAllPlayersRestriction;

		private readonly ILogger<OrbisParentalControlInfoService> _logger;
	}
}
