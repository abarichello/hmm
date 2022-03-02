using System;
using HeavyMetalMachines.CompetitiveMode.Players;
using HeavyMetalMachines.Players.Business;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.Matchmaking
{
	public class InitializeAndWatchMyPlayerCompetitiveStateProgress : IInitializeAndWatchMyPlayerCompetitiveStateProgress
	{
		public InitializeAndWatchMyPlayerCompetitiveStateProgress(IGetOrFetchPlayerState getOrFetchPlayerState, ILocalPlayerStorage playerStorage, IMyPlayerStateProgressStorage myPlayerStateProgressStorage, ICompetitiveP2pService competitiveP2PService)
		{
			this.AssertValidParameters(getOrFetchPlayerState, playerStorage, myPlayerStateProgressStorage, competitiveP2PService);
			this._playerStorage = playerStorage;
			this._getOrFetchPlayerState = getOrFetchPlayerState;
			this._myPlayerStateProgressStorage = myPlayerStateProgressStorage;
			this._competitiveP2PService = competitiveP2PService;
		}

		private void AssertValidParameters(IGetOrFetchPlayerState getOrFetchPlayerState, ILocalPlayerStorage playerStorage, IMyPlayerStateProgressStorage myPlayerStateProgressStorage, ICompetitiveP2pService competitiveP2PService)
		{
		}

		public IObservable<Unit> InitializeAndWatch()
		{
			return Observable.AsUnitObservable<Unit>(Observable.ContinueWith<PlayerCompetitiveState, Unit>(Observable.Do<PlayerCompetitiveState>(Observable.ContinueWith<Unit, PlayerCompetitiveState>(Observable.Defer<Unit>(new Func<IObservable<Unit>>(this.ClearMyPlayerStateProgressStorage)), this._getOrFetchPlayerState.GetFromPlayerId(this._playerStorage.Player.PlayerId)), new Action<PlayerCompetitiveState>(this.StorePlayerStateBeforeMatchInStorage)), new Func<PlayerCompetitiveState, IObservable<Unit>>(this.WaitForPlayerStateFromGameServer)));
		}

		private IObservable<Unit> ClearMyPlayerStateProgressStorage()
		{
			this._myPlayerStateProgressStorage.InitialState = null;
			this._myPlayerStateProgressStorage.FinalState = null;
			return Observable.ReturnUnit();
		}

		private IObservable<Unit> WaitForPlayerStateFromGameServer(PlayerCompetitiveState playerCompetitiveState)
		{
			return Observable.AsUnitObservable<PlayerCompetitiveState>(Observable.Do<PlayerCompetitiveState>(Observable.First<PlayerCompetitiveState>(this._competitiveP2PService.OnMyPlayerCompetitiveStateReceived), new Action<PlayerCompetitiveState>(this.StorePlayerStateAfterMatchInStorage)));
		}

		private void StorePlayerStateBeforeMatchInStorage(PlayerCompetitiveState playerCompetitiveState)
		{
			this._myPlayerStateProgressStorage.InitialState = new PlayerCompetitiveState?(playerCompetitiveState);
		}

		private void StorePlayerStateAfterMatchInStorage(PlayerCompetitiveState playerCompetitiveState)
		{
			this._myPlayerStateProgressStorage.FinalState = new PlayerCompetitiveState?(playerCompetitiveState);
		}

		private readonly ILocalPlayerStorage _playerStorage;

		private readonly IGetOrFetchPlayerState _getOrFetchPlayerState;

		private readonly IMyPlayerStateProgressStorage _myPlayerStateProgressStorage;

		private readonly ICompetitiveP2pService _competitiveP2PService;
	}
}
