using System;
using HeavyMetalMachines.ExpirableStorage;
using HeavyMetalMachines.Logs;
using Hoplon.Logging;
using Hoplon.Time;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.Players
{
	public class GetOrFetchPlayerState : IGetOrFetchPlayerState
	{
		public GetOrFetchPlayerState(IPlayersStateStorage playersStateStorage, IUpdatePlayerState updatePlayerState, ICurrentTime currentTime)
		{
			this._playersStateStorage = playersStateStorage;
			this._updatePlayerState = updatePlayerState;
			this._currentTime = currentTime;
			this._logger = new BitLogLogger<GetOrFetchPlayerState>();
		}

		public IObservable<PlayerCompetitiveState> GetMine()
		{
			throw new NotImplementedException();
		}

		public IObservable<PlayerCompetitiveState> GetFromPlayerId(long playerId)
		{
			PlayerCompetitiveState playerCompetitiveState;
			if (this.TryGetPlayerCompetitiveStateFromStorage(playerId, out playerCompetitiveState))
			{
				return Observable.Return<PlayerCompetitiveState>(playerCompetitiveState);
			}
			return this.UpdateThenGet(playerId);
		}

		public IObservable<PlayerCompetitiveState[]> GetFromPlayersIds(long[] playersIds)
		{
			PlayerCompetitiveState[] array = new PlayerCompetitiveState[playersIds.Length];
			for (int i = 0; i < playersIds.Length; i++)
			{
				PlayerCompetitiveState playerCompetitiveState;
				if (!this.TryGetPlayerCompetitiveStateFromStorage(playersIds[i], out playerCompetitiveState))
				{
					return this.UpdateThenGet(playersIds);
				}
				array[i] = playerCompetitiveState;
			}
			return Observable.Return<PlayerCompetitiveState[]>(array);
		}

		private bool TryGetPlayerCompetitiveStateFromStorage(long playerId, out PlayerCompetitiveState state)
		{
			Expirable<PlayerCompetitiveState> expirable;
			bool flag = this._playersStateStorage.PlayersCompetitiveStateDictionary.TryGetValue(playerId, out expirable);
			if (!flag || expirable.IsExpired(this._currentTime))
			{
				state = default(PlayerCompetitiveState);
				return false;
			}
			state = expirable.Value;
			return true;
		}

		private IObservable<PlayerCompetitiveState> UpdateThenGet(long playerId)
		{
			return Observable.Select<Unit, PlayerCompetitiveState>(this._updatePlayerState.Update(playerId), (Unit _) => this.GetFromStorage(playerId));
		}

		private IObservable<PlayerCompetitiveState[]> UpdateThenGet(long[] playersIds)
		{
			return Observable.Select<Unit, PlayerCompetitiveState[]>(this._updatePlayerState.Update(playersIds), (Unit _) => this.GetFromStorage(playersIds));
		}

		private PlayerCompetitiveState GetFromStorage(long playerId)
		{
			if (!this._playersStateStorage.PlayersCompetitiveStateDictionary.ContainsKey(playerId))
			{
				this._logger.ErrorFormat("Fail to get PlayerId in storage. PlayerId: {0}", new object[]
				{
					playerId
				});
			}
			Expirable<PlayerCompetitiveState> expirable = this._playersStateStorage.PlayersCompetitiveStateDictionary[playerId];
			return expirable.Value;
		}

		private PlayerCompetitiveState[] GetFromStorage(long[] playerIds)
		{
			PlayerCompetitiveState[] array = new PlayerCompetitiveState[playerIds.Length];
			for (int i = 0; i < playerIds.Length; i++)
			{
				array[i] = this.GetFromStorage(playerIds[i]);
			}
			return array;
		}

		private readonly IPlayersStateStorage _playersStateStorage;

		private readonly IUpdatePlayerState _updatePlayerState;

		private readonly ICurrentTime _currentTime;

		private ILogger<GetOrFetchPlayerState> _logger;
	}
}
