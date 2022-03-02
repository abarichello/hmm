using System;
using HeavyMetalMachines.CompetitiveMode.Seasons;
using HeavyMetalMachines.CompetitiveMode.Seasons.Exceptions;
using HeavyMetalMachines.ExpirableStorage;
using Hoplon.Logging;
using Hoplon.Time;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.Players
{
	public class UpdatePlayerState : IUpdatePlayerState
	{
		public UpdatePlayerState(IPlayersStateStorage playersStateStorage, IPlayerCompetitiveStateProvider playerCompetitiveStateProvider, ICurrentTime currentTime, ISeasonsStorage seasonsStorage, ILogger<UpdatePlayerState> logger)
		{
			this._playersStateStorage = playersStateStorage;
			this._playerCompetitiveStateProvider = playerCompetitiveStateProvider;
			this._currentTime = currentTime;
			this._seasonsStorage = seasonsStorage;
			this._logger = logger;
		}

		public IObservable<Unit> Update(long playerId)
		{
			long seasonId = this.GetCurrentOrNextSeasonId();
			return Observable.AsUnitObservable<PlayerCompetitiveState>(Observable.Do<PlayerCompetitiveState>(Observable.Catch<PlayerCompetitiveState, Exception>(this._playerCompetitiveStateProvider.GetPlayerCompetitiveState(playerId, seasonId), (Exception exception) => Observable.Do<PlayerCompetitiveState>(this.LogErrorAndFallbackToRandomState(exception, playerId, seasonId), new Action<PlayerCompetitiveState>(this.UpdatePlayerCompetitiveStateIntoStorage))), new Action<PlayerCompetitiveState>(this.UpdatePlayerCompetitiveStateIntoStorage)));
		}

		public IObservable<Unit> Update(long[] playersIds)
		{
			long currentOrNextSeasonId = this.GetCurrentOrNextSeasonId();
			return Observable.AsUnitObservable<PlayerCompetitiveState[]>(Observable.Do<PlayerCompetitiveState[]>(this._playerCompetitiveStateProvider.GetPlayersCompetitiveState(playersIds, currentOrNextSeasonId), new Action<PlayerCompetitiveState[]>(this.UpdatePlayerCompetitiveStateIntoStorage)));
		}

		private void UpdatePlayerCompetitiveStateIntoStorage(PlayerCompetitiveState state)
		{
			DateTime expirationDate = this.GetExpirationDate();
			Expirable<PlayerCompetitiveState> value = new Expirable<PlayerCompetitiveState>(state, expirationDate);
			this._playersStateStorage.PlayersCompetitiveStateDictionary[state.PlayerId] = value;
		}

		private void UpdatePlayerCompetitiveStateIntoStorage(PlayerCompetitiveState[] states)
		{
			foreach (PlayerCompetitiveState state in states)
			{
				this.UpdatePlayerCompetitiveStateIntoStorage(state);
			}
		}

		private DateTime GetExpirationDate()
		{
			return this._currentTime.Now() + UpdatePlayerState._expirationTime;
		}

		private long GetCurrentOrNextSeasonId()
		{
			if (this._seasonsStorage.CurrentSeason != null)
			{
				return this._seasonsStorage.CurrentSeason.Id;
			}
			if (this._seasonsStorage.NextSeason != null)
			{
				return this._seasonsStorage.NextSeason.Id;
			}
			throw new NoCurrentNorNextCompetitiveSeasonException();
		}

		private IObservable<PlayerCompetitiveState> LogErrorAndFallbackToRandomState(Exception exception, long playerId, long seasonId)
		{
			this._logger.Error(exception);
			this._logger.Warn(string.Format("An error occurred when fetching the competitive state for player {0} for season {1}. Fallbacking to a random state.", playerId, seasonId));
			FakePlayerCompetitiveStateProvider fakePlayerCompetitiveStateProvider = new FakePlayerCompetitiveStateProvider();
			return fakePlayerCompetitiveStateProvider.GetPlayerCompetitiveState(playerId, seasonId);
		}

		private readonly IPlayersStateStorage _playersStateStorage;

		private readonly IPlayerCompetitiveStateProvider _playerCompetitiveStateProvider;

		private readonly ICurrentTime _currentTime;

		private readonly ISeasonsStorage _seasonsStorage;

		private readonly ILogger<UpdatePlayerState> _logger;

		private static readonly TimeSpan _expirationTime = TimeSpan.FromMinutes(5.0);
	}
}
