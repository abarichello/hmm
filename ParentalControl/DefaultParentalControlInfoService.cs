using System;
using System.Collections.Generic;
using System.Linq;
using ClientAPI;
using ClientAPI.Objects;
using ClientAPI.Service.Interfaces;
using HeavyMetalMachines.Players.Business;
using HeavyMetalMachines.Swordfish;
using Hoplon.Logging;
using Hoplon.Reactive;
using UniRx;

namespace HeavyMetalMachines.ParentalControl
{
	public class DefaultParentalControlInfoService : IParentalControlInfoService
	{
		public DefaultParentalControlInfoService(IPlayerToAllPlayersRestriction playerToAllPlayersRestriction, IParentalControlInfoStorage parentalControlInfoStorage, ILogger<DefaultParentalControlInfoService> logger)
		{
			this._playerToAllPlayersRestriction = playerToAllPlayersRestriction;
			this._parentalControlInfoStorage = parentalControlInfoStorage;
			this._logger = logger;
			this.StartListeningForDebugInput();
		}

		private void StartListeningForDebugInput()
		{
		}

		public IObservable<ParentalControlInfo> GetGlobalParentalControlInfo()
		{
			return HoplonObservable.ReturnDeferred<ParentalControlInfo>(() => new ParentalControlInfo
			{
				RestrictAllChat = false,
				RestrictVoiceChat = false,
				RestrictTextChat = false,
				RestrictUserGeneratedContent = false,
				RestrictCrossPlay = false
			});
		}

		public IObservable<Dictionary<PlayerIdentification, ParentalControlInfo>> GetParentalControlInfoByUniversalIds(PlayerIdentification[] players)
		{
			return Observable.DoOnError<Dictionary<PlayerIdentification, ParentalControlInfo>>(Observable.Select<PlayerToAllPlayersRestriction[], Dictionary<PlayerIdentification, ParentalControlInfo>>(this.GetPlayerRestrictions(players), (PlayerToAllPlayersRestriction[] results) => this.ConvertToParentalControlInfoByPlayers(players, results)), delegate(Exception ex)
			{
				this._logger.Error("error while getting parental control info from SF. " + ex.Message);
			});
		}

		private IObservable<PlayerToAllPlayersRestriction[]> GetPlayerRestrictions(PlayerIdentification[] players)
		{
			if (this._isPlayerUgcRestricted)
			{
				return Observable.Return<PlayerToAllPlayersRestriction[]>((from player in players
				select new PlayerToAllPlayersRestriction
				{
					Id = player.PlayerId,
					IsRestriction = true,
					UniversalId = player.UniversalId
				}).ToArray<PlayerToAllPlayersRestriction>());
			}
			return SwordfishObservable.FromSwordfishCall<PlayerToAllPlayersRestriction[]>(delegate(SwordfishClientApi.ParameterizedCallback<PlayerToAllPlayersRestriction[]> success, SwordfishClientApi.ErrorCallback error)
			{
				string[] array = (from p in players
				select p.UniversalId).ToArray<string>();
				this._playerToAllPlayersRestriction.GetPlayersByUniversalId(null, array, success, error);
			});
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

		private readonly IPlayerToAllPlayersRestriction _playerToAllPlayersRestriction;

		private readonly IParentalControlInfoStorage _parentalControlInfoStorage;

		private readonly ILogger<DefaultParentalControlInfoService> _logger;

		private bool _isPlayerUgcRestricted;
	}
}
