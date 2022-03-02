using System;
using HeavyMetalMachines.ExpirableStorage;
using HeavyMetalMachines.Players.Business;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.Players
{
	public class FetchPlayerCompetitiveState : IFetchPlayerCompetitiveState
	{
		public FetchPlayerCompetitiveState(IUpdatePlayerState updatePlayerState, ILocalPlayerStorage playerStorage, IPlayersStateStorage playersStateStorage)
		{
			this._updatePlayerState = updatePlayerState;
			this._playersStateStorage = playersStateStorage;
			this._playerStorage = playerStorage;
		}

		public IObservable<PlayerCompetitiveState> FetchMine()
		{
			long myPlayerId = this._playerStorage.Player.PlayerId;
			return Observable.Select<Unit, PlayerCompetitiveState>(this._updatePlayerState.Update(myPlayerId), (Unit _) => this.GetFromStorage(myPlayerId));
		}

		public IObservable<PlayerCompetitiveState> Fetch(long playerId)
		{
			return Observable.Select<Unit, PlayerCompetitiveState>(this._updatePlayerState.Update(playerId), (Unit _) => this.GetFromStorage(playerId));
		}

		private PlayerCompetitiveState GetFromStorage(long playerId)
		{
			Expirable<PlayerCompetitiveState> expirable = this._playersStateStorage.PlayersCompetitiveStateDictionary[playerId];
			return expirable.Value;
		}

		private readonly IUpdatePlayerState _updatePlayerState;

		private readonly ILocalPlayerStorage _playerStorage;

		private readonly IPlayersStateStorage _playersStateStorage;
	}
}
