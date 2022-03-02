using System;
using HeavyMetalMachines.Players.Business;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.Players
{
	public class GetPlayerCompetitiveState : IGetPlayerCompetitiveState
	{
		public GetPlayerCompetitiveState(IGetOrFetchPlayerState getOrFetchPlayerState, ILocalPlayerStorage playerStorage)
		{
			this._getOrFetchPlayerState = getOrFetchPlayerState;
			this._playerStorage = playerStorage;
		}

		public IObservable<PlayerCompetitiveState> GetMine()
		{
			return Observable.ContinueWith<long, PlayerCompetitiveState>(Observable.Defer<long>(() => Observable.Return<long>(this._playerStorage.Player.PlayerId)), (long playerId) => this._getOrFetchPlayerState.GetFromPlayerId(playerId));
		}

		public IObservable<PlayerCompetitiveState> GetFromPlayerId(long playerId)
		{
			return this._getOrFetchPlayerState.GetFromPlayerId(playerId);
		}

		public IObservable<PlayerCompetitiveState[]> GetFromPlayersIds(long[] playersIds)
		{
			return this._getOrFetchPlayerState.GetFromPlayersIds(playersIds);
		}

		private readonly IGetOrFetchPlayerState _getOrFetchPlayerState;

		private readonly ILocalPlayerStorage _playerStorage;
	}
}
