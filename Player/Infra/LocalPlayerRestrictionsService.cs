using System;
using System.Runtime.CompilerServices;
using ClientAPI;
using ClientAPI.Objects;
using ClientAPI.Service.Interfaces;
using HeavyMetalMachines.Players.Business;
using HeavyMetalMachines.Publishing;
using HeavyMetalMachines.Swordfish;
using UniRx;

namespace HeavymetalMachines.Player.Infra
{
	public class LocalPlayerRestrictionsService : ILocalPlayerRestrictionsService
	{
		public LocalPlayerRestrictionsService(IPublisherBlockedPlayersService publisherBlockedPlayersService, IPlayerToPlayerRestriction playerToPlayerRestriction, IGetLocalPlayer getLocalPlayer)
		{
			this._publisherBlockedPlayersService = publisherBlockedPlayersService;
			this._playerToPlayerRestriction = playerToPlayerRestriction;
			this._getLocalPlayer = getLocalPlayer;
		}

		public IObservable<PlayerToPlayerRestriction[]> GetRestrictions()
		{
			return this.GetRestrictions(null);
		}

		public IObservable<PlayerToPlayerRestriction[]> GetRestrictions(long[] playerIds)
		{
			return Observable.ContinueWith<Unit, PlayerToPlayerRestriction[]>(this._publisherBlockedPlayersService.GetThenPersist(), this.GetPlayerRestrictions(playerIds));
		}

		private IObservable<PlayerToPlayerRestriction[]> GetPlayerRestrictions(long[] playerIds = null)
		{
			return SwordfishObservable.FromSwordfishCall<PlayerToPlayerRestriction[]>(delegate(SwordfishClientApi.ParameterizedCallback<PlayerToPlayerRestriction[]> success, SwordfishClientApi.ErrorCallback error)
			{
				PlayerToPlayerRestrictionFilter playerToPlayerRestrictionFilter = this.CreatePlayerToPlayerRestrictionFilter(playerIds);
				this._playerToPlayerRestriction.GetPlayersRestrictions(null, playerToPlayerRestrictionFilter, success, error);
			});
		}

		private PlayerToPlayerRestrictionFilter CreatePlayerToPlayerRestrictionFilter(long[] playerIds = null)
		{
			long playerId = this._getLocalPlayer.Get().PlayerId;
			PlayerToPlayerRestrictionFilter playerToPlayerRestrictionFilter = new PlayerToPlayerRestrictionFilter();
			playerToPlayerRestrictionFilter.ToPlayerId = new long[]
			{
				playerId
			};
			PlayerToPlayerRestrictionFilter playerToPlayerRestrictionFilter2 = playerToPlayerRestrictionFilter;
			PlayerToPlayerRestrictionType[] array = new PlayerToPlayerRestrictionType[4];
			RuntimeHelpers.InitializeArray(array, fieldof(<PrivateImplementationDetails>.$field-7C086B50DCF601EB8EC9ABE41D17E1F25B6E4956).FieldHandle);
			playerToPlayerRestrictionFilter2.RestrictionType = array;
			PlayerToPlayerRestrictionFilter playerToPlayerRestrictionFilter3 = playerToPlayerRestrictionFilter;
			if (playerIds != null)
			{
				playerToPlayerRestrictionFilter3.FromPlayerId = playerIds;
			}
			return playerToPlayerRestrictionFilter3;
		}

		public IObservable<Unit> AddRestriction(PlayerToPlayerRestrictionType restrictionType, long playerId)
		{
			return SwordfishObservable.FromSwordfishCall(delegate(SwordfishClientApi.Callback success, SwordfishClientApi.ErrorCallback error)
			{
				PlayerToPlayerRestriction playerToPlayerRestriction = new PlayerToPlayerRestriction
				{
					PlayerToPlayerRestrictionType = restrictionType,
					FromPlayerId = playerId,
					ToPlayerId = this._getLocalPlayer.Get().PlayerId
				};
				this._playerToPlayerRestriction.AddRestriction(null, playerToPlayerRestriction, success, error);
			});
		}

		public IObservable<Unit> RemoveRestriction(PlayerToPlayerRestrictionType restrictionType, long playerId)
		{
			return SwordfishObservable.FromSwordfishCall(delegate(SwordfishClientApi.Callback success, SwordfishClientApi.ErrorCallback error)
			{
				this._playerToPlayerRestriction.RemoveRestriction(null, playerId, this._getLocalPlayer.Get().PlayerId, restrictionType, success, error);
			});
		}

		private readonly IPublisherBlockedPlayersService _publisherBlockedPlayersService;

		private readonly IPlayerToPlayerRestriction _playerToPlayerRestriction;

		private readonly IGetLocalPlayer _getLocalPlayer;
	}
}
