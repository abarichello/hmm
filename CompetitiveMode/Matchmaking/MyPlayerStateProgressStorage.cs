using System;
using HeavyMetalMachines.CompetitiveMode.Players;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.Matchmaking
{
	public class MyPlayerStateProgressStorage : IMyPlayerStateProgressStorage
	{
		public IObservable<PlayerCompetitiveProgress> OnProgressSet
		{
			get
			{
				return this._onProgressSet;
			}
		}

		public PlayerCompetitiveState? InitialState
		{
			get
			{
				return this._initialState;
			}
			set
			{
				this._initialState = value;
				this.CheckStatesAndCallProgressChanged();
			}
		}

		public PlayerCompetitiveState? FinalState
		{
			get
			{
				return this._finalState;
			}
			set
			{
				this._finalState = value;
				this.CheckStatesAndCallProgressChanged();
			}
		}

		private void CheckStatesAndCallProgressChanged()
		{
			if (this._initialState == null || this._finalState == null)
			{
				return;
			}
			PlayerCompetitiveProgress playerCompetitiveProgress = new PlayerCompetitiveProgress(this._initialState.Value, this._finalState.Value);
			this._onProgressSet.OnNext(playerCompetitiveProgress);
		}

		public void Clear()
		{
			this._initialState = null;
			this._finalState = null;
		}

		private readonly Subject<PlayerCompetitiveProgress> _onProgressSet = new Subject<PlayerCompetitiveProgress>();

		private PlayerCompetitiveState? _initialState;

		private PlayerCompetitiveState? _finalState;
	}
}
